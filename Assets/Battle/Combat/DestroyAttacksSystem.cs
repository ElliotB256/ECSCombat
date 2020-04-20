using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// Deletes all entities with Attack component.
    /// </summary>
    [
        UpdateAfter(typeof(AttackResultSystemsGroup)),
        UpdateBefore(typeof(PostAttackEntityBuffer))
        ]
    public class DestroyAttacksSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem m_endSimBufferSystem;
        private EntityQuery AttackQuery;

        protected override void OnCreate()
        {
            m_endSimBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            AttackQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Attack>());
        }

        protected override void OnUpdate()
        {
            var buffer = m_endSimBufferSystem.CreateCommandBuffer();
            buffer.DestroyEntity(AttackQuery);
        }
    }
}