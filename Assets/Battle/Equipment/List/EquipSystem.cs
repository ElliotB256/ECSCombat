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
    public class EquipSystem : SystemBase
    {
        protected EntityQuery EntitiesToEquip;
        protected EarlyEquipmentBufferSystem EquipmentBuffer;

        /// <summary>
        /// Key is parent, values are Equipment entities.
        /// </summary>
        protected NativeMultiHashMap<Entity, Entity> EquipmentMap;

        protected override void OnCreate()
        {
            EquipmentBuffer = World.GetOrCreateSystem<EarlyEquipmentBufferSystem>();
        }

        protected override void OnUpdate()
        {
            EquipmentMap = new NativeMultiHashMap<Entity, Entity>(EntitiesToEquip.CalculateEntityCount(), Allocator.TempJob);

            // Start by sorting newly added equipment by parent entity.
            var equipmentMapWriter = EquipmentMap.AsParallelWriter();
            var buffer = EquipmentBuffer.CreateCommandBuffer().ToConcurrent();
            Dependency = 
                Entities
                .WithAll<Equipment>()
                .WithNone<Equipped>()
                .WithStoreEntityQueryInField(ref EntitiesToEquip)
                .ForEach(
                (
                    Entity e,
                    int entityInQueryIndex,
                    ref Parent parent
                    ) =>
                {
                    equipmentMapWriter.Add(parent.Value, e);
                    buffer.AddComponent(entityInQueryIndex, e, new Equipping());
                    buffer.AddComponent(entityInQueryIndex, e, new Equipped());
                }
                )
                .Schedule(Dependency);

            EquipmentBuffer.AddJobHandleForProducer(Dependency);

            Dependency = new UpdateEquipmentLists
            {
                EquipmentLists = GetBufferFromEntity<EquipmentList>(false),
                EquipmentMap = EquipmentMap
            }.Schedule(Dependency);

            EquipmentMap.Dispose(Dependency);
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

                for (int i = 0; i < parents.Length; i++)
                {
                    var parent = parents[i];

                    if (!EquipmentLists.Exists(parent))
                        continue;

                    var equipmentList = EquipmentLists[parent];
                    equipmentList.Add(values[i]);
                }

                values.Dispose();
                parents.Dispose();
            }
        }
    }
}