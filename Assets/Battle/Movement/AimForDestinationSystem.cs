using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Battle.Movement
{
    /// <summary>
    /// Updates TurnSpeed to aim toward the intended Destination.
    /// </summary>
    public class AimForDestinationSystem : JobComponentSystem
    {
        [BurstCompile]
        struct UpdatePositionJob : IJobForEach<Translation, Heading, Destination, MaxTurnSpeed, TurnSpeed, Speed>
        {
            public float DeltaTime;

            public void Execute(
                [ReadOnly] ref Translation translation,
                [ReadOnly] ref Heading heading,
                [ReadOnly] ref Destination dest,
                [ReadOnly] ref MaxTurnSpeed maxTurnSpeed,
                ref TurnSpeed rotSpeed,
                ref Speed speed
                )
            {
                // Determine desired heading to target
                Vector3 dx = dest.Value - translation.Value;
                float desiredHeading = math.atan2(dx.x, dx.z);

                // Adjust rotation speed to aim for desired heading.
                float diff = desiredHeading - heading.Value;
                float fpi = Mathf.PI;
                while (diff > fpi) { diff -= 2 * fpi; }
                while (diff < -fpi) { diff += 2 * fpi; }

                // For now - hardcoded turn speed of 10.
                rotSpeed.RadiansPerSecond = math.sign(diff) * math.min(maxTurnSpeed.RadiansPerSecond, math.abs(diff) / DeltaTime);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new UpdatePositionJob()
            {
                DeltaTime = Time.fixedDeltaTime
            };

            return job.Schedule(this, inputDependencies);
        }
    }
}