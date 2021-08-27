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

        AIStateChangeBufferSystem _commandBufferSystem;

        protected override void OnCreate()
        {
            _commandBufferSystem = World.GetOrCreateSystem<AIStateChangeBufferSystem>();
        }

        protected override void OnUpdate ()
        {
            var commands = _commandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithAll<IdleBehaviour>()
                .WithChangeFilter<Target>()
                .ForEach( ( Entity entity , int entityInQueryIndex , in Target target ) =>
                {
                    if( target.Value!=Entity.Null )
                    {
                        commands.RemoveComponent<IdleBehaviour>( entityInQueryIndex , entity );
                        commands.AddComponent<PursueBehaviour>( entityInQueryIndex , entity );
                    }
                } )
                .WithBurst()
                .ScheduleParallel();

            _commandBufferSystem.AddJobHandleForProducer( Dependency );
        }

    }
}
