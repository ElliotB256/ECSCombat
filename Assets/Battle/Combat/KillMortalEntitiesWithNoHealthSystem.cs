using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Battle.Combat
{
    /// <summary>
    /// Destroys any entity with health for which Health \> 0
    /// </summary>
    [UpdateInGroup(typeof(AttackResultSystemsGroup)), UpdateAfter(typeof(DealAttackDamageSystem))]
    public class DestroyMortalEntitiesWithNoHealthSystem : SystemBase
    {
        private PostAttackEntityBuffer m_entityBufferSystem;

        protected override void OnCreate()
        {
            m_entityBufferSystem = World.GetOrCreateSystem<PostAttackEntityBuffer>();
        }

        protected override void OnUpdate()
        {
            var buffer = m_entityBufferSystem.CreateCommandBuffer().AsParallelWriter();
            Entities
                .WithAll<Mortal>()
                .ForEach(
                (
                    Entity e,
                    int entityInQueryIndex,
                    ref Health health
                ) =>
                {
                    if (health.Value < 0f)
                        buffer.DestroyEntity(entityInQueryIndex, e);
                }
                )
                .Schedule();
            m_entityBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}