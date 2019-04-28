using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.Equipment
{
    /// <summary>
    /// Deletes the Equipping/Dequipping flag components
    /// </summary>
    [
        UpdateAfter(typeof(EquipmentUpdateGroup)),
        UpdateBefore(typeof(EquipmentBufferSystem))
        ]
    public class DeleteEquippingDequipping : JobComponentSystem
    {
        protected EquipmentBufferSystem EquipmentBuffer;

        protected override void OnCreate()
        {
            EquipmentBuffer = World.GetOrCreateSystem<EquipmentBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var equipBuffer = EquipmentBuffer.CreateCommandBuffer();
            var dequipBuffer = EquipmentBuffer.CreateCommandBuffer();

            var enableJH = new DeleteJob<Equipping> { Buffer = equipBuffer.ToConcurrent() }.Schedule(this, inputDependencies);
            var disableJH = new DeleteJob<Dequipping> { Buffer = dequipBuffer.ToConcurrent() }.Schedule(this, inputDependencies);
            var combinedJH = JobHandle.CombineDependencies(enableJH, disableJH);

            EquipmentBuffer.AddJobHandleForProducer(enableJH);
            EquipmentBuffer.AddJobHandleForProducer(disableJH);

            return combinedJH;
        }

        struct DeleteJob<T> : IJobForEachWithEntity<T>
            where T : struct,IComponentData
        {
            public EntityCommandBuffer.Concurrent Buffer;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref T comp
                )
            {
                Buffer.RemoveComponent<T>(index, e);
            }
        }
    }
}