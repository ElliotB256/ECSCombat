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
    public class CalculateRangeEffectivenessSystem : JobComponentSystem
    {
        [BurstCompile]
        struct CalculateRangeEffectivenessJob : IJobForEachWithEntity<Attack, LinearEffectiveRange, EffectSourceLocation, Target, Effectiveness>
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> targetPositions;

            public void Execute(
                Entity entity,
                int index,
                [ReadOnly] ref Attack attack,
                [ReadOnly] ref LinearEffectiveRange effectiveRange,
                [ReadOnly] ref EffectSourceLocation sourceLocation,
                [ReadOnly] ref Target target,
                ref Effectiveness effectiveness
                )
            {
                float3 targetPosition = targetPositions[index];
                float distance = math.distance(targetPosition, sourceLocation.Value);

                //amount varies from 0 to 1 over the effective range.
                float amount = (distance - effectiveRange.EffectiveRangeStart) / (effectiveRange.EffectiveRangeEnd - effectiveRange.EffectiveRangeStart);
                if (effectiveRange.IsIncreasing) amount = 1f - amount;

                effectiveness.Value = effectiveness.Value * 
                    (math.clamp(amount, 0f, 1f) * (1f - effectiveRange.MinimumEffectiveness) + effectiveRange.MinimumEffectiveness);
            }
        }

        [BurstCompile]
        struct GetTargetPositions : IJobForEachWithEntity<Target>
        {
            [WriteOnly] public NativeArray<float3> targetPositions;
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> targetWorldTransforms;

            public void Execute(Entity entity, int index, [ReadOnly] ref Target target)
            {
                targetPositions[index] = targetWorldTransforms[target.Value].Position;
            }
        }

        private EntityQuery m_attackQuery;

        protected override void OnCreate()
        {
            m_attackQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<Attack>(),
                    ComponentType.ReadOnly<LinearEffectiveRange>(),
                    ComponentType.ReadOnly<EffectSourceLocation>(),
                    ComponentType.ReadOnly<Target>(),
                    ComponentType.ReadWrite<Effectiveness>()
                }
            });
        }

        protected NativeArray<float3> m_targetPositions;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_targetPositions = new NativeArray<float3>(m_attackQuery.CalculateLength(), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var getTargetPositionsJH = new GetTargetPositions()
            {
                targetPositions = m_targetPositions,
                targetWorldTransforms = GetComponentDataFromEntity<LocalToWorld>(true)
            }.Schedule(m_attackQuery, inputDeps);

            var calculateRangeEffectivenessJH = new CalculateRangeEffectivenessJob()
            {
                targetPositions = m_targetPositions
            }.Schedule(m_attackQuery, getTargetPositionsJH);

            return calculateRangeEffectivenessJH;
        }
    }
}