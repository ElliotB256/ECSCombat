using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Battle.Movement;
using Battle.Combat;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

namespace Battle.AI
{
    /// <summary>
    /// Fighter behaviour when in pursuit of a target.
    /// </summary>
    [UpdateBefore(typeof(TurnToDestinationSystem)), UpdateBefore(typeof(AIStateChangeBufferSystem))]
    public class PursueBehaviourSystem : JobComponentSystem
    {
        public const float PROXIMITY_RADIUS = 4f;

        //[BurstCompile]
        struct PursueBehaviourJob : IJobForEachWithEntity<PursueBehaviour, Target, Translation, TurnToDestinationBehaviour>
        {
            [ReadOnly] public ComponentDataFromEntity<Translation> Positions;
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref PursueBehaviour pursue,
                [ReadOnly] ref Target target,
                [ReadOnly] ref Translation pos,
                ref TurnToDestinationBehaviour destination
                )
            {
                if (target.Value == Entity.Null || !Positions.Exists(target.Value))
                {
                    // Go to idle state
                    buffer.RemoveComponent<PursueBehaviour>(index, e);
                    buffer.AddComponent(index, e, new IdleBehaviour());
                    return;
                }

                // Set entity destination to target position
                destination.Destination = Positions[target.Value].Value;

                // if too close to target, evasive manoeuvre
                if (math.lengthsq(destination.Destination - pos.Value) < PROXIMITY_RADIUS * PROXIMITY_RADIUS)
                {
                    buffer.RemoveComponent<PursueBehaviour>(index, e);
                    buffer.RemoveComponent<TurnToDestinationBehaviour>(index, e);
                    buffer.AddComponent(index, e, new PeelManoeuvre());
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