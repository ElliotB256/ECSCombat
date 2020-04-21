using Battle.Movement;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Battle.AI
{
    /// <summary>
    /// Updates TurnSpeed to aim entity toward the intended Destination.
    /// </summary>
    [UpdateAfter(typeof(UpdateHeadingSystem))]
    [UpdateAfter(typeof(AISystemGroup))]
    public class TurnToDestinationSystem : SystemBase
    {

        protected override void OnUpdate()
        {
            float dT = Time.DeltaTime;

            Entities
                .ForEach(
                (
                    ref TurnSpeed turnSpeed,
                    in TurnToDestinationBehaviour dest,
                    in LocalToWorld localToWorld,
                    in Heading heading,
                    in MaxTurnSpeed maxTurnSpeed
                ) => {
                    // Determine desired heading to target
                    Vector3 dx = dest.Destination - localToWorld.Position;
                    float desiredHeading = MathUtil.GetHeadingToPoint(dx);

                    // Adjust rotation speed to aim for desired heading.
                    float diff = MathUtil.GetAngleDifference(desiredHeading, heading.Value);
                    turnSpeed.RadiansPerSecond = math.sign(diff) * math.min(maxTurnSpeed.RadiansPerSecond, math.abs(diff) / dT);
                }
                )
                .Schedule();
        }
    }
}