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
        protected override void OnUpdate ()
        {
            Entities
                .WithNone<GuardBehaviour>()
                .ForEach( ( ref AggroLocation source , in Target target , in LocalToWorld ltw ) =>
                {
                    source.Position = ltw.Position;
                } )
                .WithBurst()
                .ScheduleParallel();

            var ltwData = GetComponentDataFromEntity<LocalToWorld>( isReadOnly:true );
            Entities
                .WithReadOnly(ltwData)
                .ForEach( ( Entity e , ref AggroLocation source , in GuardBehaviour guard , in Target target ) =>
                {
                    if (target.Value == Entity.Null)
                        return;

                    if (!ltwData.HasComponent(guard.Target))
                    {
                        source.Position = ltwData[e].Position;
                        return;
                    }

                    source.Position = ltwData[guard.Target].Position;
                } )
                .WithBurst()
                .ScheduleParallel();
        }
    }
}