using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Battle.Movement;
using Battle.Combat;
using Unity.Transforms;
using UnityEngine;

namespace Battle.AI
{
    /// <summary>
    /// Fighter behaviour when in pursuit.
    /// </summary>
    public class PursueBehaviourSystem : JobComponentSystem
    {
        [BurstCompile]
        struct PursueBehaviourJob : IJobForEach<FighterAIState, Target, Destination, Translation>
        {
            [ReadOnly] public ComponentDataFromEntity<Translation> Positions;

            public void Execute(
                ref FighterAIState state,
                [ReadOnly] ref Target target,
                ref Destination destination,
                [ReadOnly] ref Translation pos
                )
            {
                if (state.State != FighterAIState.eState.Pursue)
                    return;

                if (target.Value == Entity.Null)
                    state.State = FighterAIState.eState.Idle;

                // Target entity towards its given target.
                destination.Value = Positions[target.Value].Value;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var pos = GetComponentDataFromEntity<Translation>(true);
            var job = new PursueBehaviourJob() { Positions = pos };
            return job.Schedule(this, inputDependencies);
        }
    }
}