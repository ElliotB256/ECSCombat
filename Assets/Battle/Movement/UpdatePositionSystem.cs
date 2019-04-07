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
    /// Updates the position of all moving entities.
    /// </summary>
    public class UpdatePositionSystem : JobComponentSystem
    {
        [BurstCompile]
        struct UpdatePositionJob : IJobForEach<Translation, Heading, TurnSpeed, Speed>
        {
            public float DeltaTime;

            public void Execute(ref Translation translation, ref Heading heading, [ReadOnly] ref TurnSpeed rotSpeed, [ReadOnly] ref Speed speed)
            {
                float3 displacement = DeltaTime * speed.Value * math.forward(quaternion.AxisAngle(new float3(0.0f, 1.0f, 0.0f), heading.Value));
                translation.Value = translation.Value + displacement;
                float newHeading = heading.Value + rotSpeed.RadiansPerSecond * DeltaTime;
                float fpi = (float)math.PI;
                newHeading = newHeading >= 2 * fpi ? newHeading - 2 * fpi : newHeading;
                newHeading = newHeading < 0 ? newHeading + 2 * fpi : newHeading;
                heading.Value = newHeading;
            }
        }

        // OnUpdate runs on the main thread.
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