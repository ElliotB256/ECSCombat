using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Battle.Combat;
using Unity.Transforms;

namespace Battle.AI
{
    /// <summary>
    /// For entities without a target, updates the location from which the target search should be taken.
    /// </summary>
    [UpdateBefore(typeof(SelectTargetSystem))]
    [UpdateInGroup(typeof(AISystemGroup))]
    public class UpdateAggressionSourceSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var standard = Entities.WithNone<GuardBehaviour>().ForEach(
                (ref AggroLocation source, in Target target, in LocalToWorld l2w) =>
                {
                    if (target.Value == Entity.Null)
                        return;

                    source.Position = l2w.Position;
                }
                ).Schedule(inputDeps);

            var positions = GetComponentDataFromEntity<LocalToWorld>(true);
            var guarding = Entities.ForEach(
                (Entity e, ref AggroLocation source, in GuardBehaviour guard, in Target target) =>
                {
                    if (target.Value == Entity.Null)
                        return;

                    if (!positions.HasComponent(guard.Target))
                    {
                        source.Position = positions[e].Position;
                        return;
                    }

                    source.Position = positions[guard.Target].Position;
                }
                ).WithReadOnly(positions).Schedule(standard);

            return guarding;
        }
    }
}