using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.Combat
{
    /// <summary>
    /// Destroys any entity whose Parent transform has died
    /// </summary>
    [UpdateInGroup(typeof(AttackResultSystemsGroup)), UpdateAfter(typeof(DealAttackDamageSystem))]
    public class KillChildrenOnParentDeathSystem : JobComponentSystem
    {
        /// <summary>
        /// God this sounds horrible
        /// </summary>
        [BurstCompile]
        struct KillChildrenWithDeadParents : IJobForEachWithEntity<Parent>
        {
            [ReadOnly] public ComponentDataFromEntity<Health> health;
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref Parent parent
                )
            {
                if (!health.Exists(parent.Value))
                    return;

                if (health[parent.Value].Value < 0f)
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
            var health = GetComponentDataFromEntity<Health>(true);
            var job = new KillChildrenWithDeadParents() { buffer = m_entityBufferSystem.CreateCommandBuffer().ToConcurrent(), health = health };
            var jobHandle = job.Schedule(this, inputDependencies);
            m_entityBufferSystem.AddJobHandleForProducer(jobHandle);
            return jobHandle;
        }
    }
}