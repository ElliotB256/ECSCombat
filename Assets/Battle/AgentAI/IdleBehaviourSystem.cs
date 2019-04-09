using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using Battle.Movement;

using Battle.Combat;

namespace Battle.AI
{
    /// <summary>
    /// When idle, entity random walks
    /// </summary>
    [UpdateBefore(typeof(PursueBehaviourSystem))]
    public class IdleBehaviourSystem : JobComponentSystem
    {
        public const float ARRIVAL_TOLERANCE = 1f;

        //[BurstCompile]
        struct IdleJob : IJobForEachWithEntity<IdleBehaviour, MoveToDestinationBehaviour, Translation, Target>
        {
            public Random random_gen;
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref IdleBehaviour idle,
                [ReadOnly] ref MoveToDestinationBehaviour movement,
                [ReadOnly] ref Translation position,
                [ReadOnly] ref Target target
                )
            {
                // If the fighter has gotten close to their destination, pick another random destination.
                if (math.lengthsq(movement.Destination - position.Value) < ARRIVAL_TOLERANCE)
                {
                    var dest = random_gen.NextFloat3(-10f, 10f);
                    dest.y = 0;
                    movement.Destination = dest;
                }

                // If the fighter has a target, change from idle to pursuit. This requires a lazy update to the AI states.
                if (target.Value != Entity.Null)
                {
                    buffer.AddComponent(index, e, new PursueBehaviour());
                    buffer.RemoveComponent<IdleBehaviour>(index, e);
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
            var random_gen = new Random((uint)UnityEngine.Random.Range(1, 10000));
            var job = new IdleJob() { random_gen = random_gen, buffer = m_AIStateBuffer.CreateCommandBuffer().ToConcurrent() };
            var jobHandle = job.Schedule(this, inputDependencies);
            m_AIStateBuffer.AddJobHandleForProducer(jobHandle);
            return jobHandle;
        }
    }
}