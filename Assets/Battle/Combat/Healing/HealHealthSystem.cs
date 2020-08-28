using Battle.Effects;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Battle.Combat
{
    [UpdateInGroup(typeof(AttackResultSystemsGroup))]
    public class HealHealthSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .ForEach(
                (
                    in Attack attack,
                    in Target target,
                    in Heal heal
                    ) =>
                {
                    var amount = heal.Value;
                    if (attack.Result == Attack.eResult.Miss)
                        return;

                    if (!HasComponent<Health>(target.Value))
                        return;

                    if (HasComponent<LastHitColor>(target.Value) && heal.Value > 0f)
                        SetComponent(target.Value, new LastHitColor { Value = new Unity.Mathematics.float4(0f, 1f, 0f, 1f) });
                    if (HasComponent<LastHitTimer>(target.Value) && heal.Value > 0f)
                        SetComponent(target.Value, new LastHitTimer { Value = 0f });

                    var health = GetComponent<Health>(target.Value);
                    var maxHealth = GetComponent<MaxHealth>(target.Value);
                    health.Value = math.min(maxHealth.Value, health.Value + amount);
                    SetComponent(target.Value, health);
                }
                )
                .Schedule();
        }
    }
}