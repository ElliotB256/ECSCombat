using Battle.Combat.Calculations;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Battle.Combat.AttackSources
{
    /// <summary>
    /// Determines whether attacks hit or miss their target.
    /// </summary>
    [UpdateInGroup(typeof(AttackSystemsGroup))]
    public class DetermineAttackMissSystem : JobComponentSystem
    {
        struct DetermineAttackMissJob : IJobForEach<Attack, Target>
        {
            public Random random;
            [ReadOnly] public ComponentDataFromEntity<Evasion> Evasions;

            public void Execute(
                ref Attack attack,
                [ReadOnly] ref Target target
                )
            {
                if (Evasions.HasComponent(target.Value))
                {
                    float evasion = Evasions[target.Value].GetEvasionRating();
                    float hitChance = math.exp(-evasion / attack.Accuracy);
                    if (random.NextFloat() > hitChance)
                        attack.Result = Attack.eResult.Miss;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var random = new Random((uint)UnityEngine.Random.Range(1, 100000));
            var job = new DetermineAttackMissJob {
                Evasions = GetComponentDataFromEntity<Evasion>(true),
                random = random
            };
            return job.Schedule(this, inputDependencies);
        }
    }
}