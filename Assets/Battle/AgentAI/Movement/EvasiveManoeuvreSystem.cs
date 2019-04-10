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
    [UpdateBefore(typeof(MoveToDestinationSystem)), UpdateBefore(typeof(AIStateChangeBufferSystem))]
    public class EvasiveManoeuvreSystem : JobComponentSystem
    {
        public const float ENGAGEMENT_RADIUS = 10f;

        //[BurstCompile]
        struct PursueBehaviourJob : IJobForEachWithEntity<EvasiveManoeuvre, Target, Translation, TurnSpeed>
        {
            [ReadOnly] public ComponentDataFromEntity<Translation> Positions;
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref EvasiveManoeuvre pursue,
                [ReadOnly] ref Target target,
                [ReadOnly] ref Translation pos,
                ref TurnSpeed turnSpeed
                )
            {
                if (target.Value == Entity.Null)
                    return;

                if (!Positions.Exists(target.Value))
                {
                    UnityEngine.Debug.LogWarning("Could not find entity in position table.");
                    return;
                }

                // 
                turnSpeed.RadiansPerSecond = 0f;

                // Remain in evasive manoeuvre until a certain distance to target is reached.
                if (math.lengthsq(Positions[target.Value].Value - pos.Value) > ENGAGEMENT_RADIUS * ENGAGEMENT_RADIUS)
                {
                    buffer.RemoveComponent<EvasiveManoeuvre>(index, e);
                    buffer.AddComponent(index, e, new PursueBehaviour());
                    buffer.AddComponent(index, e, new MoveToDestinationBehaviour());
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