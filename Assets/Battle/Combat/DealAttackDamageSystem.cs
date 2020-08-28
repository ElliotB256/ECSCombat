using Battle.Effects;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Battle.Combat
{
    /// <summary>
    /// Deals damage for all Attack entities with a Damage component.
    /// </summary>
    [
        UpdateInGroup(typeof(AttackResultSystemsGroup))
        ]
    public class DealAttackDamageSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var lastHitTimers = GetComponentDataFromEntity<LastHitTimer>();
            var healths = GetComponentDataFromEntity<Health>();

            Entities
                .ForEach(
                (
                    in Attack attack,
                    in Target target,
                    in Damage damage
                    ) =>
                {
                    var amount = damage.Value;
                    if (attack.Result == Attack.eResult.Miss)
                        return;

                    if (lastHitTimers.HasComponent(target.Value) && damage.Value > 0f)
                        lastHitTimers[target.Value] = new LastHitTimer { Value = 0f };
                    if (HasComponent<LastHitColor>(target.Value) && damage.Value > 0f)
                        SetComponent(target.Value, new LastHitColor { Value = new Unity.Mathematics.float4(1f,1f,1f,1f) });

                    if (healths.HasComponent(target.Value))
                    {
                        var health = healths[target.Value];
                        health.Value -= amount;
                        healths[target.Value] = health;
                    }
                }
                )
                .Schedule();
        }
    }
}