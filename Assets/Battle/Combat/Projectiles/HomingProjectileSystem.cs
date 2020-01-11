using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Battle.AI;
using Unity.Transforms;
using Unity.Burst;

namespace Battle.Combat
{
    [UpdateBefore(typeof(TurnToDestinationSystem))]
    public class ProjectilePursueTargetSystem : JobComponentSystem
    {
        [BurstCompile]
        struct PursueJob : IJobForEachWithEntity<Homing, Projectile, Target, Translation, TurnToDestinationBehaviour>
        {
            [ReadOnly] public ComponentDataFromEntity<Translation> Positions;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref Homing pursue,
                [ReadOnly] ref Projectile projectile,
                [ReadOnly] ref Target target,
                [ReadOnly] ref Translation pos,
                ref TurnToDestinationBehaviour destination
                )
            {
                if (target.Value == Entity.Null || !Positions.Exists(target.Value))
                {
                    return;
                }

                // Set entity destination to target position
                destination.Destination = Positions[target.Value].Value;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var pos = GetComponentDataFromEntity<Translation>(true);
            var job = new PursueJob() { Positions = pos };
            var jobHandle = job.Schedule(this, inputDependencies);
            return jobHandle;
        }
    }
}