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
    [UpdateInGroup(typeof(EquipmentUpdateGroup))]
    public class SetEquipmentTeamOnEquipSystem : SystemBase
    {
        protected override void OnUpdate ()
        {
            var teamData = GetComponentDataFromEntity<Team>( isReadOnly:false );

            Entities
                .WithAll<Team,Equipment,Equipping>()
                .ForEach( ( Entity entity , in Parent parent ) =>
                {
                    if( !teamData.HasComponent(parent.Value) )
                        return;
                    
                    teamData[entity] = teamData[parent.Value];
                } )
                .WithBurst()
                .ScheduleParallel();
        }
    }
}
