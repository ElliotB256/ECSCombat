using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.Combat
{
    /// <summary>
    /// Sets any Target components that point to an invalid, non-existant entity to Entity.Null
    /// </summary>
    [
        UpdateAfter(typeof(DeleteEntitiesSystem)),
        UpdateInGroup(typeof(LateSimulationSystemGroup))
    ]
    public class RemoveDeadTargetReferencesSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var targetables = GetComponentDataFromEntity<Targetable>(true);
            var deleting = GetComponentDataFromEntity<Delete>(true);
            Entities
                .ForEach(
                (ref Target target) =>
                {
                    if (!targetables.HasComponent(target.Value))
                        target.Value = Entity.Null;
                    if (deleting.HasComponent(target.Value))
                        target.Value = Entity.Null;
                }
                )
                .WithReadOnly(targetables)
                .WithReadOnly(deleting)
                .ScheduleParallel();
        }
    }
}