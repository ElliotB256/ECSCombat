using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Battle.AI
{
    /// <summary>
    /// Causes entities to walk randomly about.
    /// </summary>
    [UpdateBefore(typeof(PursueBehaviourSystem))]
    [UpdateInGroup(typeof(AISystemGroup))]
    public class RandomWalkSystem : JobComponentSystem
    {
        public const float ARRIVAL_TOLERANCE = 3f;

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var randomGenerator = new Random((uint)UnityEngine.Random.Range(1, 10000));

            return Entities.WithAll<IdleBehaviour>().ForEach(
                (ref TurnToDestinationBehaviour movement, in LocalToWorld transform, in RandomWalkBehaviour walk) =>
                {
                    bool newLocation = (math.lengthsq(movement.Destination - transform.Position) < ARRIVAL_TOLERANCE) ||
                                       (math.lengthsq(walk.Centre - transform.Position) > math.pow(walk.Radius, 2f));

                    if (newLocation)
                    {
                        var direction = randomGenerator.NextFloat2Direction();
                        var dest = walk.Centre + walk.Radius * randomGenerator.NextFloat(0f, 1f) * new float3(direction.x, 0f, direction.y);
                        movement.Destination = dest;
                    }
                }
                ).Schedule(inputDependencies);
        }
    }
}