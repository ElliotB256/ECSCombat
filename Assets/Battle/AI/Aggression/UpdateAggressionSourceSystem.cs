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
    [UpdateBefore(typeof(SelectTargetsSystem))]
    [UpdateInGroup(typeof(AISystemGroup))]
    public class UpdateAggressionSourceSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Dependency = Entities.WithNone<GuardBehaviour>().ForEach(
                (ref AggroLocation source, in Target target, in LocalToWorld l2w) =>
                {
                    source.Position = l2w.Position;
                }
                ).Schedule(Dependency);

            var positions = GetComponentDataFromEntity<LocalToWorld>(true);
            Dependency = Entities.ForEach(
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
                ).WithReadOnly(positions).Schedule(Dependency);
        }
    }
}