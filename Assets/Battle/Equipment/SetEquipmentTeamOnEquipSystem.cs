using Battle.Combat;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

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
            var teams = GetComponentDataFromEntity<Team>();

            Entities
                .WithAll<Equipment, Equipping>()
                .ForEach(
                (ref Team team, in Parent parent) =>
                {
                    if (!teams.Exists(parent.Value))
                    {
                        Debug.LogWarning("Parent entity does not have a team component.");
                        return;
                    }
                    team = teams[parent.Value];
                }
                )
                .Schedule();
        }
    }
}
