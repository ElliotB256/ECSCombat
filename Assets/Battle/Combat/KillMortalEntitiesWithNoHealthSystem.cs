using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Battle.Combat
{
    /// <summary>
    /// Destroys any entity with health for which Health < 0
    /// </summary>
    [UpdateInGroup(typeof(AttackResultSystemsGroup)), UpdateAfter(typeof(DealAttackDamageSystem))]
    public class KillMortalEntitiesWithNoHealthSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(Mortal))]
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

        private PostAttackEntityBuffer m_entityBufferSystem;

        protected override void OnCreateManager()
        {
            m_entityBufferSystem = World.GetOrCreateSystem<PostAttackEntityBuffer>();
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