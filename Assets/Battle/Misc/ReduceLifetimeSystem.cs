using Unity.Entities;
using Unity.Jobs;

namespace Battle.Combat
{
    /// <summary>
    /// Reduces lifetime, marks expired entities for deletion.
    /// </summary>
    [
        UpdateBefore(typeof(LateSimulationSystemGroup))
        ]
    public class ReduceLifetimeSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem BufferSystem;

        protected override void OnCreate()
        {
            BufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var buffer = BufferSystem.CreateCommandBuffer().ToConcurrent();
            float dT = Time.DeltaTime;
            Entities
                .ForEach(
                (Entity e, int entityInQueryIndex, ref Lifetime lifetime) =>
                {
                    lifetime.Value -= dT;
                    if (lifetime.Value < 0f)
                        buffer.AddComponent(entityInQueryIndex, e, new Delete());
                })
                .ScheduleParallel();
            BufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}