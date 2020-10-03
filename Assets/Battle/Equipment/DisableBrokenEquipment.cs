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
    public class DisableBrokenEquipment : SystemBase
    {
        protected EarlyEquipmentBufferSystem EquipmentBuffer;

        protected override void OnCreate()
        {
            EquipmentBuffer = World.GetOrCreateSystem<EarlyEquipmentBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var buffer = EquipmentBuffer.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithAll<Enabled>()
                .ForEach(
                (
                    Entity e,
                    int entityInQueryIndex,
                    in Health health
                    ) =>
                {
                    if (health.Value <= 0f)
                        buffer.AddComponent(entityInQueryIndex, e, new Disabling());
                }
                )
                .Schedule();

            EquipmentBuffer.AddJobHandleForProducer(Dependency);
        }
    }
}