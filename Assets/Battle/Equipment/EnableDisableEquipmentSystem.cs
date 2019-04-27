using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.Equipment
{
    /// <summary>
    /// Enable/Disable equipment
    /// </summary>
    [
        UpdateAfter(typeof(EquipmentUpdateGroup)),
        UpdateBefore(typeof(EquipmentBufferSystem))
        ]
    public class EnableDisableEquipmentSystem : JobComponentSystem
    {
        protected EquipmentBufferSystem EquipmentBuffer;

        protected override void OnCreate()
        {
            EquipmentBuffer = World.GetOrCreateSystem<EquipmentBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var enableBuffer = EquipmentBuffer.CreateCommandBuffer();
            var disableBuffer = EquipmentBuffer.CreateCommandBuffer();

            var enableJH = new EnableEntities { Buffer = enableBuffer.ToConcurrent() }.Schedule(this, inputDependencies);
            var disableJH = new DisableEntities { Buffer = disableBuffer.ToConcurrent() }.Schedule(this, inputDependencies);
            var combinedJH = JobHandle.CombineDependencies(enableJH, disableJH);

            EquipmentBuffer.AddJobHandleForProducer(enableJH);
            EquipmentBuffer.AddJobHandleForProducer(disableJH);

            return combinedJH;
        }

        /// <summary>
        /// Enables entities
        /// </summary>
        //[BurstCompile]
        struct EnableEntities : IJobForEachWithEntity<Enabling>
        {
            public EntityCommandBuffer.Concurrent Buffer;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref Enabling enabling
                )
            {
                Buffer.RemoveComponent<Enabling>(index, e);
                Buffer.AddComponent(index, e, new Enabled());
            }
        }

        /// <summary>
        /// Disables entities
        /// </summary>
        //[BurstCompile]
        struct DisableEntities : IJobForEachWithEntity<Disabling>
        {
            public EntityCommandBuffer.Concurrent Buffer;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref Disabling enabling
                )
            {
                Buffer.RemoveComponent<Disabling>(index, e);
                Buffer.RemoveComponent<Enabled>(index, e);
            }
        }
    }
}