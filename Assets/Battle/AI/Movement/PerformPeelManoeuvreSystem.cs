using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Battle.Combat;
using Unity.Transforms;
using Unity.Mathematics;
using Battle.Movement;

namespace Battle.AI
{
    /// <summary>
    /// Fighter behaviour when in pursuit of a target.
    /// </summary>
    [UpdateInGroup(typeof(AISystemGroup))]
    public class PerformPeelManoeuvreSystem : SystemBase
    {
        public const float ENGAGEMENT_RADIUS = 10f;

        private AIStateChangeBufferSystem m_AIStateBuffer;

        protected override void OnCreate()
        {
            m_AIStateBuffer = World.GetOrCreateSystem<AIStateChangeBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var positions = GetComponentDataFromEntity<Translation>(true);
            var buffer = m_AIStateBuffer.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithAll<PeelManoeuvre>()
                .ForEach(
                (
                Entity e,
                int entityInQueryIndex,
                ref TurnSpeed turnSpeed,
                in Target target,
                in Translation pos,
                in Heading heading,
                in MaxTurnSpeed maxTurnSpeed
                ) =>
                {
                    if (target.Value == Entity.Null || !positions.HasComponent(target.Value))
                    {
                        buffer.RemoveComponent<PeelManoeuvre>(entityInQueryIndex, e);
                        buffer.AddComponent(entityInQueryIndex, e, new IdleBehaviour());
                        buffer.AddComponent(entityInQueryIndex, e, new TurnToDestinationBehaviour());
                        return;
                    }

                    //Target position
                    var targetPos = positions[target.Value];

                    // Turn away from the enemy.
                    float angleDiff = MathUtil.GetAngleDifference(MathUtil.GetHeadingToPoint(targetPos.Value - pos.Value), heading.Value);
                    if (math.abs(angleDiff) < 0.3 * math.PI)
                        turnSpeed.RadiansPerSecond = -maxTurnSpeed.RadiansPerSecond * math.sign(angleDiff);
                    else
                        turnSpeed.RadiansPerSecond = 0f;

                    // Remain in evasive manoeuvre until a certain distance to target is reached.
                    if (math.lengthsq(targetPos.Value - pos.Value) > ENGAGEMENT_RADIUS * ENGAGEMENT_RADIUS)
                    {
                        buffer.RemoveComponent<PeelManoeuvre>(entityInQueryIndex, e);
                        buffer.AddComponent(entityInQueryIndex, e, new PursueBehaviour());
                        buffer.AddComponent(entityInQueryIndex, e, new TurnToDestinationBehaviour());
                    }
                }
                )
                .WithReadOnly(positions)
                .ScheduleParallel();
            m_AIStateBuffer.AddJobHandleForProducer(Dependency);
        }
    }
}