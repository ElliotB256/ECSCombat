using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Battle.Equipment
{
    /// <summary>
    /// Removes the Equipping/Dequipping flag components
    /// </summary>
    [
        UpdateAfter(typeof(EquipmentUpdateGroup)),
        UpdateBefore(typeof(EquipmentBufferSystem))
        ]
    public class DeleteEquippingDequippingSystem : JobComponentSystem
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

            var enableJH = new DeleteEquippingJob { Buffer = equipBuffer.ToConcurrent() }.Schedule(this, inputDependencies);
            var disableJH = new DeleteDequippingJob { Buffer = dequipBuffer.ToConcurrent() }.Schedule(this, inputDependencies);
            var combinedJH = JobHandle.CombineDependencies(enableJH, disableJH);

            EquipmentBuffer.AddJobHandleForProducer(enableJH);
            EquipmentBuffer.AddJobHandleForProducer(disableJH);

            return combinedJH;
        }

        internal struct DeleteEquippingJob : IJobForEachWithEntity<Equipping>
        {
            public EntityCommandBuffer.Concurrent Buffer;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref Equipping comp
                )
            {
                Buffer.RemoveComponent<Equipping>(index, e);
            }
        }

        internal struct DeleteDequippingJob : IJobForEachWithEntity<Dequipping>
        {
            public EntityCommandBuffer.Concurrent Buffer;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref Dequipping comp
                )
            {
                Buffer.RemoveComponent<Dequipping>(index, e);
            }
        }
    }
}