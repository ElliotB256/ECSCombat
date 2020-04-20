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
    public class IdleToCombatSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var buffer = m_AIStateBuffer.CreateCommandBuffer().ToConcurrent();

            var jobHandle = Entities.WithAll<IdleBehaviour>().ForEach(
                (int entityInQueryIndex, Entity e, in Target target) =>
                {
                    if (target.Value != Entity.Null)
                    {
                        buffer.RemoveComponent<IdleBehaviour>(entityInQueryIndex, e);
                        buffer.AddComponent(entityInQueryIndex, e, new PursueBehaviour());
                    }
                }
                ).Schedule(inputDependencies);

            m_AIStateBuffer.AddJobHandleForProducer(jobHandle);
            return jobHandle;
        }

        private AIStateChangeBufferSystem m_AIStateBuffer;

        protected override void OnCreate()
        {
            m_AIStateBuffer = World.GetOrCreateSystem<AIStateChangeBufferSystem>();
        }
    }
}