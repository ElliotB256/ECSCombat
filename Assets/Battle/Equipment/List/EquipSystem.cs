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

        protected override void OnCreate()
        {
            EquipmentBuffer = World.GetOrCreateSystem<EarlyEquipmentBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var count = EntitiesToEquip.CalculateEntityCount();
            var EquipmentEntities = new NativeArray<Entity>(count, Allocator.TempJob);
            var ParentEntities = new NativeArray<Entity>(count, Allocator.TempJob);

            // Start by sorting newly added equipment by parent entity.
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
                    EquipmentEntities[entityInQueryIndex] = e;
                    ParentEntities[entityInQueryIndex] = parent.Value;
                    buffer.AddComponent(entityInQueryIndex, e, new Equipping());
                    buffer.AddComponent(entityInQueryIndex, e, new Equipped());
                }
                )
                .Schedule(Dependency);

            EquipmentBuffer.AddJobHandleForProducer(Dependency);

            Dependency = new UpdateEquipmentLists
            {
                EquipmentLists = GetBufferFromEntity<EquipmentList>(false),
                Equipments = EquipmentEntities,
                Parents = ParentEntities
            }.Schedule(Dependency);

            EquipmentEntities.Dispose(Dependency);
            ParentEntities.Dispose(Dependency);
        }

        /// <summary>
        /// Maps TEquipment components to the parent of their entities.
        /// </summary>
        //[BurstCompile]
        struct UpdateEquipmentLists : IJob
        {
            [ReadOnly] public NativeArray<Entity> Parents;
            [ReadOnly] public NativeArray<Entity> Equipments;
            public BufferFromEntity<EquipmentList> EquipmentLists;

            public void Execute()
            {
                for (int i = 0; i < Parents.Length; i++)
                {
                    var parent = Parents[i];

                    if (!EquipmentLists.Exists(parent))
                        continue;

                    var equipmentList = EquipmentLists[parent];
                    equipmentList.Add(Equipments[i]);
                }
            }
        }
    }
}