using Unity.Burst;
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
    [UpdateBefore(typeof(TurnToDestinationSystem)), UpdateBefore(typeof(AIStateChangeBufferSystem))]
    public class EvasiveManoeuvreSystem : JobComponentSystem
    {
        public const float ENGAGEMENT_RADIUS = 10f;

        //[BurstCompile]
        struct PursueBehaviourJob : IJobForEachWithEntity<EvasiveManoeuvre, Target, Translation, TurnSpeed, Heading, MaxTurnSpeed>
        {
            [ReadOnly] public ComponentDataFromEntity<Translation> Positions;
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref EvasiveManoeuvre pursue,
                [ReadOnly] ref Target target,
                [ReadOnly] ref Translation pos,
                ref TurnSpeed turnSpeed,
                [ReadOnly] ref Heading heading,
                [ReadOnly] ref MaxTurnSpeed maxTurnSpeed
                )
            {
                if (target.Value == Entity.Null || !Positions.Exists(target.Value))
                {
                    buffer.RemoveComponent<EvasiveManoeuvre>(index, e);
                    buffer.AddComponent(index, e, new IdleBehaviour());
                    buffer.AddComponent(index, e, new TurnToDestinationBehaviour());
                    return;
                }

                //Target position
                var targetPos = Positions[target.Value];

                // Turn away from the enemy.
                float angleDiff = MathUtil.GetAngleDifference(MathUtil.GetHeadingToPoint(targetPos.Value - pos.Value), heading.Value);
                if (math.abs(angleDiff) < 0.3*math.PI)
                    turnSpeed.RadiansPerSecond = - maxTurnSpeed.RadiansPerSecond * math.sign(angleDiff);
                else
                    turnSpeed.RadiansPerSecond = 0f;

                // Remain in evasive manoeuvre until a certain distance to target is reached.
                if (math.lengthsq(targetPos.Value - pos.Value) > ENGAGEMENT_RADIUS * ENGAGEMENT_RADIUS)
                {
                    buffer.RemoveComponent<EvasiveManoeuvre>(index, e);
                    buffer.AddComponent(index, e, new PursueBehaviour());
                    buffer.AddComponent(index, e, new TurnToDestinationBehaviour());
                }
            }
        }

        private AIStateChangeBufferSystem m_AIStateBuffer;

        protected override void OnCreateManager()
        {
            m_AIStateBuffer = World.GetOrCreateSystem<AIStateChangeBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var pos = GetComponentDataFromEntity<Translation>(true);
            var job = new PursueBehaviourJob() { Positions = pos, buffer = m_AIStateBuffer.CreateCommandBuffer().ToConcurrent() };
            var jobHandle = job.Schedule(this, inputDependencies);
            m_AIStateBuffer.AddJobHandleForProducer(jobHandle);
            return jobHandle;
        }
    }
}