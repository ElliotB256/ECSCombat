using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

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
            Entities
                .ForEach(
                (ref Target target) =>
                {
                    if (!targetables.Exists(target.Value))
                        target.Value = Entity.Null;
                }
                )
                .WithReadOnly(targetables)
                .Schedule();
        }
    }
}