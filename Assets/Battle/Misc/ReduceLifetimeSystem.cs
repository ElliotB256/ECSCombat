using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Battle.Combat
{
    /// <summary>
    /// Reduces lifetime, marks expired entities for deletion.
    /// </summary>
    [
        UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))
        ]
    public class ReduceLifetimeSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem BufferSystem;

        [BurstCompile]
        struct ReduceLifetimesJob : IJobForEachWithEntity<Lifetime>
        {
            public EntityCommandBuffer.Concurrent buffer;
            public float dT;

            public void Execute(
                Entity e,
                int index,
                ref Lifetime lifetime
                )
            {
                lifetime.Value = lifetime.Value - dT;
                if (lifetime.Value < 0f)
                    buffer.AddComponent(index, e, new Delete());
            }
        }

        protected override void OnCreateManager()
        {
            BufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new ReduceLifetimesJob {
                buffer = BufferSystem.CreateCommandBuffer().ToConcurrent(),
                dT = Time.DeltaTime
            };
            var jobHandle = job.Schedule(this, inputDependencies);
            BufferSystem.AddJobHandleForProducer(jobHandle);
            return jobHandle;
        }
    }
}