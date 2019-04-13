using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.Movement
{
    /// <summary>
    /// Sets the Heading of entities according to their Rotation vector.
    /// </summary>
    [UpdateAfter(typeof(TransformSystemGroup))]
    public class UpdateHeadingSystem : JobComponentSystem
    {
        [BurstCompile]
        struct UpdateHeadingJob : IJobForEach<LocalToWorld, Heading>
        {
            public void Execute(
                [ReadOnly] ref LocalToWorld localToWorld,
                ref Heading heading                
                )
            {
                heading.Value = MathUtil.GetHeadingToPoint(localToWorld.Forward);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new UpdateHeadingJob();
            return job.Schedule(this, inputDependencies);
        }
    }
}