using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.AI
{
    /// <summary>
    /// Causes entities to walk randomly about.
    /// </summary>
    [UpdateBefore(typeof(PursueBehaviourSystem))]
    [UpdateInGroup(typeof(AISystemGroup))]
    public class RandomWalkSystem : SystemBase
    {
        public const float ARRIVAL_TOLERANCE = 3f;

        protected override void OnUpdate ()
        {
            uint seed = (uint)UnityEngine.Random.Range(1,10000);

            Entities
                .WithAll<IdleBehaviour>()
                .ForEach( ( Entity entity , int entityInQueryIndex , ref TurnToDestinationBehaviour movement , in LocalToWorld transform , in RandomWalkBehaviour walk ) =>
                {
                    bool newLocation = (math.lengthsq(movement.Destination - transform.Position) < ARRIVAL_TOLERANCE) ||
                        (math.lengthsq(walk.Centre - transform.Position) > math.pow(walk.Radius, 2f));

                    if( newLocation )
                    {
                        var rnd = new Random( seed + (uint)(entityInQueryIndex*1000) );
                        var direction = rnd.NextFloat2Direction();
                        var dest = walk.Centre + walk.Radius * rnd.NextFloat(0f, 1f) * new float3{ x=direction.x , z= direction.y };
                        movement.Destination = dest;
                    }
                } )
                .WithBurst()
                .ScheduleParallel();
        }
    }
}