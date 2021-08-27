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
            var positions = GetComponentDataFromEntity<LocalToWorld>(true);

            Entities.ForEach(
                (
                    ref TurretBehaviour behaviour,
                    ref TargettedTool tool
                    ) =>
                {
                    behaviour.Range = tool.Range;
                }
                ).ScheduleParallel();

            Entities.ForEach(
                (
                Entity e,
                int entityInQueryIndex,
                ref Target target,
                ref TurnToDestinationBehaviour turnTo,
                in TurretBehaviour behaviour,
                in LocalToWorld localToWorld
                ) =>
                {
                    if (target.Value == Entity.Null)
                        return;
                    if (!positions.HasComponent(target.Value))
                    {
                        target.Value = Entity.Null;
                        return;
                    }

                    var targetPos = positions[target.Value].Position;

                    // Disengage if target is outside range.
                    if (math.lengthsq(targetPos - localToWorld.Position) > behaviour.Range * behaviour.Range)
                        target.Value = Entity.Null;

                    // Point turret to target
                    turnTo.Destination = targetPos;
                }
                )
                .WithReadOnly(positions)
                .ScheduleParallel();
        }
    }
}