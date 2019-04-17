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
    /// Updates the rotation of all entities with TurnSpeed
    /// </summary>
    [UpdateInGroup(typeof(MovementUpdateSystems))]
    public class UpdateRotationSystem : JobComponentSystem
    {
        [BurstCompile]
        struct UpdateRotationJob : IJobForEach<Rotation, TurnSpeed>
        {
            public float DeltaTime;

            public void Execute(
                ref Rotation rot,
                [ReadOnly] ref TurnSpeed turnSpeed
                )
            {
                rot.Value = math.mul(math.normalize(rot.Value), quaternion.AxisAngle(math.up(), turnSpeed.RadiansPerSecond * DeltaTime));
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new UpdateRotationJob() { DeltaTime = Time.fixedDeltaTime };
            return job.Schedule(this, inputDependencies);
        }
    }
}