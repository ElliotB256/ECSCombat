using Battle.Combat;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.Equipment
{
    /// <summary>
    /// Essentially a workaround until I implement a more permanent solution using initialisation tag components.
    /// </summary>
    [UpdateBefore(typeof(AI.AISystemGroup))]
    public class SetTeamToParentTeam : SystemBase
    {
        protected override void OnUpdate ()
        {
            var teamData = GetComponentDataFromEntity<Team>( isReadOnly:false );

            Entities
                .WithAll<Team>()
                .WithChangeFilter<Parent>()
                .ForEach( ( Entity entity , in Parent parent ) =>
                {
                    if( teamData.HasComponent(parent.Value) )
                        teamData[entity] = teamData[parent.Value];
                } )
                .WithBurst()
                .Schedule();
        }
    }
}
