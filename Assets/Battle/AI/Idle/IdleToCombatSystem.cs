using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using Battle.Combat;

namespace Battle.AI
{
    /// <summary>
    /// Transitions entities from Idle state into Combat.
    /// </summary>
    [UpdateBefore(typeof(PursueBehaviourSystem))]
    [UpdateInGroup(typeof(AISystemGroup))]
    public class IdleToCombatSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var buffer = m_AIStateBuffer.CreateCommandBuffer().AsParallelWriter();

            Dependency = Entities.WithAll<IdleBehaviour>().ForEach(
                (int entityInQueryIndex, Entity e, in Target target) =>
                {
                    if (target.Value != Entity.Null)
                    {
                        buffer.RemoveComponent<IdleBehaviour>(entityInQueryIndex, e);
                        buffer.AddComponent(entityInQueryIndex, e, new PursueBehaviour());
                    }
                }
                ).Schedule(Dependency);

            m_AIStateBuffer.AddJobHandleForProducer(Dependency);
        }

        private AIStateChangeBufferSystem m_AIStateBuffer;

        protected override void OnCreate()
        {
            m_AIStateBuffer = World.GetOrCreateSystem<AIStateChangeBufferSystem>();
        }
    }
}