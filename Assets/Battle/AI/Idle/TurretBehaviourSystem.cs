using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using Battle.Combat;
using Battle.Combat.AttackSources;

namespace Battle.AI
{
    /// <summary>
    /// A turret searches for targets when it does not have one or the target is outside of range.
    /// </summary>
    [UpdateBefore(typeof(PursueBehaviourSystem))]
    [UpdateInGroup(typeof(AISystemGroup))]
    public class TurretBehaviourSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ltwData = GetComponentDataFromEntity<LocalToWorld>( isReadOnly:true );

            Entities
                .ForEach( ( ref TurretBehaviour behaviour, ref TargettedTool tool ) => behaviour.Range = tool.Range )
                .WithBurst()
                .ScheduleParallel();

            Entities
                .WithReadOnly(ltwData)
                .ForEach( (
                    Entity entity ,
                    int entityInQueryIndex ,
                    ref Target target ,
                    ref TurnToDestinationBehaviour turnTo ,
                    in TurretBehaviour behaviour ,
                    in LocalToWorld localToWorld
                ) =>
                {
                    if( target.Value==Entity.Null )
                        return;
                    if( !ltwData.HasComponent(target.Value) )
                    {
                        target.Value = Entity.Null;
                        return;
                    }

                    var targetPos = ltwData[target.Value].Position;

                    // Disengage if target is outside range.
                    if (math.lengthsq(targetPos - localToWorld.Position) > behaviour.Range * behaviour.Range)
                        target.Value = Entity.Null;

                    // Point turret to target
                    turnTo.Destination = targetPos;
                } )
                .WithBurst()
                .ScheduleParallel();
        }
    }
}
