using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Battle.Combat
{
    /// <summary>
    /// Deletes all attacks (entities with Attack component).
    /// </summary>
    public class CleanUpAttacksSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem m_endSimBufferSystem;

        [BurstCompile]
        struct CleanUpAttackJob : IJobForEachWithEntity<Attack>
        {
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(
                Entity attack,
                int index,
                [ReadOnly] ref Attack attackComp
                )
            {
                buffer.DestroyEntity(index, attack);
            }
        }

        protected override void OnCreateManager()
        {
            m_endSimBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new CleanUpAttackJob() { buffer = m_endSimBufferSystem.CreateCommandBuffer().ToConcurrent() };
            var jobHandle = job.Schedule(this, inputDependencies);
            m_endSimBufferSystem.AddJobHandleForProducer(jobHandle);
            return jobHandle;
        }
    }
}