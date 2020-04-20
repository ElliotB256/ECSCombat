using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Battle.AI;
using Unity.Transforms;

namespace Battle.Combat
{
    [UpdateBefore(typeof(TurnToDestinationSystem))]
    public class ProjectilePursueTargetSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var translations = GetComponentDataFromEntity<Translation>(true);
            Entities
                .WithAll<Homing, Projectile>()
                .ForEach(
                (
                    Entity e,
                    int entityInQueryIndex,
                    ref TurnToDestinationBehaviour destination,
                    in Target target
                    ) =>
                {
                    if (target.Value == Entity.Null || !translations.Exists(target.Value))
                        return;

                    destination.Destination = translations[target.Value].Value;
                })
                .WithReadOnly(translations)
                .Schedule();
        }
    }
}