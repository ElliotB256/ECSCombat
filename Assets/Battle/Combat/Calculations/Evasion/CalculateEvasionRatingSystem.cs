using Battle.Combat.Calculations;
using Battle.Movement;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Battle.Combat.AttackSources
{
    [UpdateInGroup(typeof(AttackSystemsGroup))]
    [UpdateBefore(typeof(DetermineAttackMissSystem))]
    public class CalculateEvasionRatingSystem : SystemBase
    {
        public const float SPEED_EVASION_FACTOR = 6f;
        public const float TURN_EVASION_FACTOR = 2f;

        protected override void OnUpdate()
        {
            Entities
                .ForEach(
                (
                    ref Evasion evasion,
                    in TurnSpeed turnSpeed,
                    in MaxTurnSpeed maxTurnSpeed,
                    in Speed speed
                    ) =>
                {
                    evasion.Rating = math.max(0f, evasion.NaturalBonus +
                        math.sqrt(
                            (math.abs(turnSpeed.RadiansPerSecond) + math.abs(maxTurnSpeed.RadiansPerSecond)) / 2f
                            * speed.Value
                            / (TURN_EVASION_FACTOR * SPEED_EVASION_FACTOR)
                        ));
                }
                )
                .Schedule();
        }
    }
}