using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Battle.Combat
{
    /// <summary>
    /// Destroys any entity with health for which Health < 0
    /// </summary>
    [UpdateAfter(typeof(DealAttackDamageSystem)), UpdateBefore(typeof(DeleteDeadEntitiesBuffer))]
    public class KillEntitiesWithNoHealthSystem : JobComponentSystem
    {
        [BurstCompile]
        struct KillEntitiesWithNoHealthJob : IJobForEachWithEntity<Health>
        {
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref Health health
                )
            {
                if (health.Value < 0f)
                    buffer.DestroyEntity(index, e);
            }
        }

        private DeleteDeadEntitiesBuffer m_entityBufferSystem;

        protected override void OnCreateManager()
        {
            m_entityBufferSystem = World.GetOrCreateSystem<DeleteDeadEntitiesBuffer>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new KillEntitiesWithNoHealthJob() { buffer = m_entityBufferSystem.CreateCommandBuffer().ToConcurrent() };
            var jobHandle = job.Schedule(this, inputDependencies);
            m_entityBufferSystem.AddJobHandleForProducer(jobHandle);
            return jobHandle;
        }
    }
}