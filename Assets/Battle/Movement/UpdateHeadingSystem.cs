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
    public class UpdateHeadingSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                (ref Heading heading, in LocalToWorld localToWorld) =>
                {
                    heading.Value = MathUtil.GetHeadingToPoint(localToWorld.Forward);
                })
                .ScheduleParallel();
        }
    }
}