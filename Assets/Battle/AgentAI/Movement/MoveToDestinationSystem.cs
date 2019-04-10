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
    public class MoveToDestinationSystem : JobComponentSystem
    {
        [BurstCompile]
        struct MoveToDestinationJob : IJobForEach<MoveToDestinationBehaviour, Translation, Heading, MaxTurnSpeed, TurnSpeed>
        {
            public float DeltaTime;

            public void Execute(
                [ReadOnly] ref MoveToDestinationBehaviour dest,
                [ReadOnly] ref Translation translation,
                [ReadOnly] ref Heading heading,
                [ReadOnly] ref MaxTurnSpeed maxTurnSpeed,
                ref TurnSpeed turnSpeed
                )
            {
                // Determine desired heading to target
                Vector3 dx = dest.Destination - translation.Value;
                //float desiredHeading = math.atan2(dx.x, dx.z);
                float desiredHeading = MathUtil.GetHeadingToPoint(dx);

                // Adjust rotation speed to aim for desired heading.
                float diff = MathUtil.GetAngleDifference(desiredHeading, heading.Value);

                // For now - hardcoded turn speed of 10.
                turnSpeed.RadiansPerSecond = math.sign(diff) * math.min(maxTurnSpeed.RadiansPerSecond, math.abs(diff) / DeltaTime);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new MoveToDestinationJob() { DeltaTime = Time.fixedDeltaTime };
            return job.Schedule(this, inputDependencies);
        }
    }
}