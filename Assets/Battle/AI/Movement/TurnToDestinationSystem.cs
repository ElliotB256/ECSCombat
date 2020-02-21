using Battle.Movement;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Battle.AI
{
    /// <summary>
    /// Updates TurnSpeed to aim entity toward the intended Destination.
    /// </summary>
    [UpdateAfter(typeof(UpdateHeadingSystem))]
    public class TurnToDestinationSystem : JobComponentSystem
    {
        [BurstCompile]
        struct TurnToDestinationJob : IJobForEach<TurnToDestinationBehaviour, LocalToWorld, Heading, MaxTurnSpeed, TurnSpeed>
        {
            public float DeltaTime;

            public void Execute(
                [ReadOnly] ref TurnToDestinationBehaviour dest,
                [ReadOnly] ref LocalToWorld localToWorld,
                [ReadOnly] ref Heading heading,
                [ReadOnly] ref MaxTurnSpeed maxTurnSpeed,
                ref TurnSpeed turnSpeed
                )
            {
                // Determine desired heading to target
                Vector3 dx = dest.Destination - localToWorld.Position;
                float desiredHeading = MathUtil.GetHeadingToPoint(dx);

                // Adjust rotation speed to aim for desired heading.
                float diff = MathUtil.GetAngleDifference(desiredHeading, heading.Value);
                turnSpeed.RadiansPerSecond = math.sign(diff) * math.min(maxTurnSpeed.RadiansPerSecond, math.abs(diff) / DeltaTime);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new TurnToDestinationJob() { DeltaTime = Time.fixedDeltaTime };
            return job.Schedule(this, inputDependencies);
        }
    }
}