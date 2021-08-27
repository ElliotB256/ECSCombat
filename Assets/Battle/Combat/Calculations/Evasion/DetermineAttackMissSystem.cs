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
    public class DetermineAttackMissSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var random = new Random((uint)UnityEngine.Random.Range(1, 100000));
            var evasions = GetComponentDataFromEntity<Evasion>(true);

            Entities
                .ForEach(
                (ref Attack attack, in Target target) =>
                {
                    if (evasions.HasComponent(target.Value))
                    {
                        float evasion = evasions[target.Value].Rating;
                        float hitChance = math.exp(-evasion / attack.Accuracy);
                        if (random.NextFloat() > hitChance)
                            attack.Result = Attack.eResult.Miss;
                    }
                }
                )
                .WithReadOnly(evasions)
                .ScheduleParallel();
        }
    }
}