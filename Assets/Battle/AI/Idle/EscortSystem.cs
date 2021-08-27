using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.AI
{
    /// <summary>
    /// Causes entities to walk randomly about.
    /// </summary>
    [UpdateBefore(typeof(RandomWalkSystem))]
    [UpdateInGroup(typeof(AISystemGroup))]
    public class EscortSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ltwData = GetComponentDataFromEntity<LocalToWorld>( isReadOnly:true );

            Entities
                .WithReadOnly(ltwData)
                .ForEach( ( ref RandomWalkBehaviour walk , in Escort escort ) =>
                {
                    if( ltwData.HasComponent(escort.Target) )
                        walk.Centre = ltwData[escort.Target].Position;
                } )
                .WithBurst()
                .ScheduleParallel();
        }
    }
}