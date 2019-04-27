using Battle.Combat;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.Equipment
{
    /// <summary>
    /// Disables any enabled equipment with less than zero health.
    /// </summary>
    [
        UpdateInGroup(typeof(EarlyEquipmentUpdateGroup))
        ]
    public class DisableBrokenEquipment : JobComponentSystem
    {
        protected EarlyEquipmentBufferSystem EquipmentBuffer;

        protected override void OnCreate()
        {
            EquipmentBuffer = World.GetOrCreateSystem<EarlyEquipmentBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var disableBuffer = EquipmentBuffer.CreateCommandBuffer();
            var disableJH = new DisableEntities { Buffer = disableBuffer.ToConcurrent() }.Schedule(this, inputDependencies);
            EquipmentBuffer.AddJobHandleForProducer(disableJH);
            return disableJH;
        }

        /// <summary>
        /// Disable enabled equipment with less than zero health.
        /// </summary>
        [RequireComponentTag(typeof(Enabled))]
        struct DisableEntities : IJobForEachWithEntity<Health>
        {
            public EntityCommandBuffer.Concurrent Buffer;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref Health health
                )
            {
                if (health.Value <= 0f)
                    Buffer.AddComponent(index, e, new Disabling());
            }
        }
    }
}