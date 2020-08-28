using Battle.Combat;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.Equipment
{
    /// <summary>
    /// Adds parent team to the equipment as it is being equiped.
    /// 
    /// Very similar to EquipmentTargetsParentTarget
    /// </summary>
    [
        UpdateInGroup(typeof(EquipmentUpdateGroup))
    ]
    public class SetEquipmentTeamOnEquipSystem : SystemBase
    {
        public EntityQuery Query;

        protected override void OnUpdate()
        {
            Entities
                .WithAll<Team, Equipment, Equipping>()
                .ForEach(
                (Entity e, in Parent parent) =>
                {
                    if (!HasComponent<Team>(parent.Value))
                        return;
                    SetComponent(e, GetComponent<Team>(parent.Value));
                }
                )
                .Schedule();
        }
    }
}
