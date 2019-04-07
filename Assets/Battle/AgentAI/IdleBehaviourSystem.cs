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
    /// When idle, fighter advances to the other half of the map.
    /// </summary>
    public class IdleBehaviourSystem : JobComponentSystem
    {
        public const float ARRIVAL_TOLERANCE = 1f;

        [BurstCompile]
        struct IdleJob : IJobForEach<FighterAIState, Translation, Target, Destination>
        {
            public Random random_gen;

            public void Execute(
                ref FighterAIState state,
                [ReadOnly] ref Translation position,
                [ReadOnly] ref Target target,
                ref Destination destination
                )
            {
                if (state.State != FighterAIState.eState.Idle)
                    return;

                // If the fighter has gotten close to their destination, pick another random destination.
                if (math.lengthsq(destination.Value - position.Value) < ARRIVAL_TOLERANCE)
                {
                    var dest = random_gen.NextFloat3(-10f, 10f);
                    dest.y = 0;
                    destination.Value = dest;
                }

                // If the fighter has a target, change to pursuit
                if (target.Value != Entity.Null)
                    state.State = FighterAIState.eState.Pursue;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var random_gen = new Random((uint)UnityEngine.Random.Range(1, 10000));
            var job = new IdleJob() { random_gen = random_gen };
            return job.Schedule(this, inputDependencies);
        }
    }
}