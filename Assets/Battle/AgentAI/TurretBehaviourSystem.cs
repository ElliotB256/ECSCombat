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
    /// A turret searches for targets when it does not have one or the target is outside of range.
    /// </summary>
    [UpdateBefore(typeof(PursueBehaviourSystem))]
    public class TurretBehaviourSystem : JobComponentSystem
    {
        [BurstCompile]
        struct IdleJob : IJobForEachWithEntity<TurretBehaviour, Translation, Target, TurnToDestinationBehaviour, DirectWeapon>
        {
            [ReadOnly] public ComponentDataFromEntity<Translation> Positions;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref TurretBehaviour behaviour,
                [ReadOnly] ref Translation position,
                ref Target target,
                ref TurnToDestinationBehaviour turnTo,
                [ReadOnly] ref DirectWeapon weapon
                )
            {

                if (target.Value == Entity.Null)
                    return;

                if (!Positions.Exists(target.Value))
                    target.Value = Entity.Null;

                var targetPos = Positions[target.Value];

                // If target is outside fighter range, disengage.
                if (math.lengthsq(targetPos.Value - position.Value) > weapon.Range * weapon.Range)
                    target.Value = Entity.Null;

                // set Turret destination to be target
                turnTo.Destination = targetPos.Value;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var pos = GetComponentDataFromEntity<Translation>(true);
            var job = new IdleJob() { Positions = pos };
            var jobHandle = job.Schedule(this, inputDependencies);
            return jobHandle;
        }
    }
}