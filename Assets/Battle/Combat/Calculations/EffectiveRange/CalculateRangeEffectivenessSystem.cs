using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Battle.Combat.Calculations
{
    /// <summary>
    /// Modifies effectiveness based on range of source to target.
    /// </summary>
    [
        AlwaysUpdateSystem,
        UpdateInGroup(typeof(AttackSystemsGroup))
        ]
    public class CalculateRangeEffectivenessSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var positions = GetComponentDataFromEntity<LocalToWorld>(true);

            Entities.ForEach(
                (
                    Entity entity,
                    int entityInQueryIndex,
                    ref Effectiveness effectiveness,
                    in LinearEffectiveRange effectiveRange,
                    in EffectSourceLocation sourceLocation,
                    in Target target
                ) =>
                {
                    float3 targetPosition = positions[target.Value].Position;
                    float distance = math.distance(targetPosition, sourceLocation.Value);

                    //amount varies from 0 to 1 over the effective range.
                    float amount = (distance - effectiveRange.EffectiveRangeStart) / (effectiveRange.EffectiveRangeEnd - effectiveRange.EffectiveRangeStart);
                    if (effectiveRange.IsIncreasing) amount = 1f - amount;

                    effectiveness.Value = effectiveness.Value *
                        (math.clamp(amount, 0f, 1f) * (1f - effectiveRange.MinimumEffectiveness) + effectiveRange.MinimumEffectiveness);
                }
                )
                .WithReadOnly(positions)
                .ScheduleParallel();
        }
    }
}