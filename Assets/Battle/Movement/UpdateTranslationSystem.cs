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
    /// Updates the Translation of all entities with speed.
    /// </summary>
    public class UpdateTranslationSystem : JobComponentSystem
    {
        [BurstCompile]
        struct UpdateTranslationJob : IJobForEach<Translation, Rotation, Speed>
        {
            public float DeltaTime;

            public void Execute(
                ref Translation translation,
                [ReadOnly] ref Rotation rot,
                [ReadOnly] ref Speed speed)
            {
                float3 displacement = DeltaTime * speed.Value * math.forward(rot.Value);
                translation.Value = translation.Value + displacement;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new UpdateTranslationJob() { DeltaTime = Time.fixedDeltaTime };
            return job.Schedule(this, inputDependencies);
        }
    }
}