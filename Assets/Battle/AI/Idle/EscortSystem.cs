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
    public class EscortSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var transforms = GetComponentDataFromEntity<LocalToWorld>(true);
            return Entities.ForEach(
                (ref RandomWalkBehaviour walk, in Escort escort) =>
                {
                    if (transforms.HasComponent(escort.Target))
                        walk.Centre = transforms[escort.Target].Position;
                }
                )
                .WithReadOnly(transforms)
                .Schedule(inputDependencies);
        }
    }
}