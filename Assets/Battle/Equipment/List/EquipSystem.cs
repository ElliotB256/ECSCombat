using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace Battle.Equipment
{
    /// <summary>
    /// Equips equipment to an entity.
    /// 
    /// Equipment entities are selected using the EntitiesToEquip query.
    /// The equipment entities are first sorted into a hashmap by their parent entity's id.
    /// The entities are concurrently marked as Equipped using an entity command buffer, to make this a one-time system per equipment.
    /// Equipped entities are added to their parent's EquipmentList.
    /// </summary>
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(EarlyEquipmentUpdateGroup))]
    public class EquipSystem : JobComponentSystem
    {
        protected EntityQuery EntitiesToEquip;
        protected EarlyEquipmentBufferSystem EquipmentBuffer;

        /// <summary>
        /// Key is parent, values are Equipment entities.
        /// </summary>
        protected NativeMultiHashMap<Entity, Entity> EquipmentMap;

        /// <summary>
        /// Boolean that stores if system has run previously.
        /// </summary>
        bool hasRunBefore = false;

        protected override void OnCreate()
        {
            EntitiesToEquip = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<Equipment>(),
                    ComponentType.ReadOnly<Parent>(),
                },
                None = new []
                {
                    ComponentType.ReadOnly<Equipped>()
                },
            });
            EquipmentBuffer = World.GetOrCreateSystem<EarlyEquipmentBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            if (hasRunBefore)
                EquipmentMap.Dispose();
            EquipmentMap = new NativeMultiHashMap<Entity, Entity>(EntitiesToEquip.CalculateEntityCount(), Allocator.TempJob);

            // Start by sorting newly added equipment by parent entity.
            var mapJH = new MapEquipmentToParent
            {
                EquipmentMap = EquipmentMap.ToConcurrent(),
                Buffer = EquipmentBuffer.CreateCommandBuffer().ToConcurrent()
            }.Schedule(EntitiesToEquip, inputDependencies);
            EquipmentBuffer.AddJobHandleForProducer(mapJH);

            var updateJH = new UpdateEquipmentLists
            {
                EquipmentLists = GetBufferFromEntity<EquipmentList>(false),
                EquipmentMap = EquipmentMap
            }.Schedule(mapJH);

            hasRunBefore = true;
            return updateJH;
        }

        protected override void OnStopRunning()
        {
            if (hasRunBefore)
            {
                EquipmentMap.Dispose();
            }
        }

        struct MapEquipmentToParent : IJobForEachWithEntity<Parent>
        {
            public EntityCommandBuffer.Concurrent Buffer;

            /// <summary>
            /// Key is parent entity, value is equipment entity.
            /// </summary>
            public NativeMultiHashMap<Entity, Entity>.Concurrent EquipmentMap;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref Parent parent
                )
            {
                EquipmentMap.Add(parent.Value, e);
                Buffer.AddComponent(index, e, new Equipping());
                Buffer.AddComponent(index, e, new Equipped());
            }
        }

        /// <summary>
        /// Maps TEquipment components to the parent of their entities.
        /// </summary>
        //[BurstCompile]
        struct UpdateEquipmentLists : IJob
        {
            [ReadOnly] public NativeMultiHashMap<Entity, Entity> EquipmentMap;
            public BufferFromEntity<EquipmentList> EquipmentLists;

            public void Execute()
            {
                // Note: The key array is not an array of unique keys. There will be duplicate values in there!
                // There exists an extension method GetUniqueKeyArray, but at the time of writing it does not work
                // with HashMaps using Entity Key:
                // NativeHashMapExtensions.GetUniqueKeyArray(EquipmentMap, Allocator.Temp);
                // For now, I'll fall back to doing this one entity at a time. In future we can make it loop over parents.
                var parents = EquipmentMap.GetKeyArray(Allocator.Temp);
                var values = EquipmentMap.GetValueArray(Allocator.Temp);
                
                //foreach (Entity parent in parents)
                //{
                //    if (!EquipmentLists.Exists(parent))
                //        continue;

                //    DynamicBuffer<EquipmentList> equipmentList = EquipmentLists[parent];
                //    Debug.Log(string.Format("Equipment list of length={0}", equipmentList.Length));

                //    EquipmentMap.TryGetFirstValue(parent, out Entity item, out var iter);
                //    do
                //    {
                //        equipmentList.Add(item);
                //    } while (EquipmentMap.TryGetNextValue(out item, ref iter));
                //}

                for  (int i = 0; i < parents.Length; i++)
                {
                    var parent = parents[i];

                    if (!EquipmentLists.Exists(parent))
                        continue;

                    DynamicBuffer<EquipmentList> equipmentList = EquipmentLists[parent];
                    equipmentList.Add(values[i]);
                }

                values.Dispose();
                parents.Dispose();
            }
        }
    }
}