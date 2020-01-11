using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Battle.Combat
{
    /// <summary>
    /// Deletes all entities with Destroy component
    /// </summary>
    [
        UpdateInGroup(typeof(LateSimulationSystemGroup))
        ]
    public class DeleteEntitiesSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem BufferSystem;

        [BurstCompile]
        struct DeleteEntitiesJob : IJobForEachWithEntity<Delete>
        {
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(
                Entity attack,
                int index,
                [ReadOnly] ref Delete destroy
                )
            {
                buffer.DestroyEntity(index, attack);
            }
        }

        protected override void OnCreateManager()
        {
            BufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new DeleteEntitiesJob() { buffer = BufferSystem.CreateCommandBuffer().ToConcurrent() };
            var jobHandle = job.Schedule(this, inputDependencies);
            BufferSystem.AddJobHandleForProducer(jobHandle);
            return jobHandle;
        }
    }
}