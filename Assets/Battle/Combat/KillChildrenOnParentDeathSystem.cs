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
    public class KillChildrenOnParentDeathSystem : SystemBase
    {

        private PostAttackEntityBuffer m_entityBufferSystem;

        protected override void OnCreate()
        {
            m_entityBufferSystem = World.GetOrCreateSystem<PostAttackEntityBuffer>();
        }

        protected override void OnUpdate()
        {
            var health = GetComponentDataFromEntity<Health>(true);
            var buffer = m_entityBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .ForEach(
                (Entity e, int entityInQueryIndex, in Parent parent) =>
                {
                    if (!health.HasComponent(parent.Value))
                        return;

                    if (health[parent.Value].Value < 0f)
                        buffer.DestroyEntity(entityInQueryIndex, e);
                }
                )
                .WithReadOnly(health)
                .Schedule();

            m_entityBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}