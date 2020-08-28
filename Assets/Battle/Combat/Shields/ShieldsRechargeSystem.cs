using Battle.Effects;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Battle.Combat
{
    [UpdateInGroup(typeof(Equipment.EquipmentUpdateGroup))]
    public class ShieldsRechargeSystem : SystemBase
    {

        public const float RECHARGE_DEAD_TIME = 2f;
        public const float SECONDS_TO_RECHARGE = 3f;

        protected override void OnUpdate()
        {
            float dT = GetSingleton<GameTimeDelta>().dT;

            Entities
                .ForEach(
                (ref Shield shield, in LastHitTimer timer, in MaxShield maxShield) =>
                {
                    if (timer.Value < SECONDS_TO_RECHARGE)
                        return;

                    var amount = maxShield.Value * dT / SECONDS_TO_RECHARGE;
                    shield.Health = math.min(maxShield.Value, shield.Health + amount);
                }
                ).Schedule();

        }
    }
}
