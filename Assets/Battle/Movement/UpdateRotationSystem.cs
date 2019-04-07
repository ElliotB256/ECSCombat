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
    public class UpdateRotationSystem : JobComponentSystem
    {
        [BurstCompile]
        struct UpdateRotationJob : IJobForEach<Heading, Rotation>
        {
            public void Execute(
                [ReadOnly] ref Heading heading,
                ref Rotation rotation
                )
            {
                rotation.Value = quaternion.AxisAngle(new float3(0f, 1f, 0f), heading.Value);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new UpdateRotationJob() { };

            return job.Schedule(this, inputDependencies);
        }
    }
}