using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Battle.Combat;

namespace Battle.AI
{
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(AISystemGroup))]
    public class SelectTargetsSystem : SystemBase
    {
        private EntityQuery TargetQuery;

        struct Target
        {
            public Entity Entity;
            public float3 Position;
            public byte Team;
            public AgentCategory Category;
            public float HealthPercent;
        }

        struct Targetter
        {
            public float3 Position;
            public byte Team;
            public TargetingOrders Orders;
            public float Radius;
            public float CurrentScore;
            public Entity CurrentTarget;

            public void Consider(Target target)
            {
                if (Orders.TargetSameTeam)
                    ConsiderAlly(target);
                else
                    ConsiderEnemy(target);
            }

            public void ConsiderEnemy(Target target)
            {
                if (Team == target.Team)
                    return;

                // Cannot target if outside aggro radius.
                var newScore = math.lengthsq(target.Position - Position);
                if (newScore > Radius * Radius)
                    return;

                // Favored and discouraged targets.
                if ((Orders.Preferred & target.Category.Type) > 0)
                    newScore /= 5f;
                if ((Orders.Discouraged & target.Category.Type) > 0)
                    newScore *= 5f;

                if (newScore > CurrentScore)
                    return;
                CurrentScore = newScore;
                CurrentTarget = target.Entity;
            }

            public void ConsiderAlly(Target target)
            {
                if (Team != target.Team)
                    return;

                // Cannot target if outside aggro radius.
                if (math.lengthsq(target.Position - Position) > Radius * Radius)
                    return;

                var newScore = target.HealthPercent;

                // Favored and discouraged targets.
                if ((Orders.Preferred & target.Category.Type) > 0)
                    newScore /= 5f;
                if ((Orders.Discouraged & target.Category.Type) > 0)
                    newScore *= 5f;

                if (newScore > CurrentScore)
                    return;
                CurrentScore = newScore;
                CurrentTarget = target.Entity;
            }
        }

        /// <summary>
        /// Cell size used for hash map sorting
        /// </summary>
        public const float HASH_CELL_SIZE = 10.0f;

        protected override void OnUpdate()
        {
            int targetNumber = TargetQuery.CalculateEntityCount();
            var targetMap = new NativeMultiHashMap<int, Target>(targetNumber, Allocator.TempJob);
            var targetMapWriter = targetMap.AsParallelWriter();

            // Store target information in hash map.
            Entities
                .WithAll<Targetable>()
                .WithStoreEntityQueryInField(ref TargetQuery)
                .ForEach( (
                    Entity targetEntity,
                    in LocalToWorld localToWorld,
                    in Team team,
                    in AgentCategory category,
                    in Health health,
                    in MaxHealth maxHealth
                ) =>
                {
                    var healthFraction = health.Value / maxHealth.Value;
                    var pos = localToWorld.Position;
                    targetMapWriter.Add(
                        HashPosition(pos), new Target
                        {
                            Entity = targetEntity,
                            Category = category,
                            HealthPercent = healthFraction,
                            Position = pos,
                            Team = team.ID
                        });
                } )
                .WithBurst()
                .ScheduleParallel();

            // Targeters select best target.
            Entities
                .WithReadOnly( targetMap ).WithDisposeOnCompletion( targetMap )
                .ForEach(
                (
                    ref Combat.Target currentTarget ,
                    in AggroLocation location ,
                    in AggroRadius aggroRadius ,
                    in Team team ,
                    in TargetingOrders orders
                ) =>
                {
                    // ignore those which already have targets
                    if( currentTarget.Value!=Entity.Null )
                        return;

                    var targetter = new Targetter{
                        Team            = team.ID,
                        Position        = location.Position,
                        Orders          = orders,
                        Radius          = aggroRadius.Value,
                        CurrentScore    = float.PositiveInfinity,
                        CurrentTarget   = Entity.Null
                    };

                    float radius = targetter.Radius;
                    int2 minCoords = Coordinates( targetter.Position - radius );
                    int2 maxCoords = Coordinates( targetter.Position + radius );

                    for( int x=minCoords.x ; x<=maxCoords.x ; x++ )
                    for( int y=minCoords.y ; y<=maxCoords.y ; y++ )
                    {
                        // Identify bucket to search
                        var currentBinID = HashCoordinates(new int2(x, y));

                        // Check targets within each bucket.
                        if (!targetMap.TryGetFirstValue(currentBinID, out Target candidate, out NativeMultiHashMapIterator<int> iterator))
                            continue;

                        targetter.Consider(candidate);
                        while (targetMap.TryGetNextValue(out candidate, ref iterator))
                            targetter.Consider(candidate);
                    }

                    currentTarget.Value = targetter.CurrentTarget;
                } )
                .WithBurst()
                .ScheduleParallel();
        }

        public static int HashPosition(float3 position)
        {
            return HashCoordinates(Coordinates(position));
        }

        public static int HashCoordinates(int2 coords) => (int)math.hash(coords);

        public static int2 Coordinates(float3 position)
        {
            return new int2(math.floor(position.xz / HASH_CELL_SIZE));
        }
    }
}