using Battle.Combat.AttackSources;
using Battle.Combat.Calculations;
using Battle.Equipment;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Battle.Combat.AttackSources
{
    /// <summary>
    /// Fires all direct weapons that are armed and in range of their target.
    /// </summary>
    [
        UpdateInGroup(typeof(WeaponSystemsGroup))
        ]
    public class FireTargettedToolsSystem : JobComponentSystem
    {
        /// <summary>
        /// The position of each Target.
        /// </summary>
        protected NativeArray<float3> m_targetPositions;

        [BurstCompile]
        struct GetTargetPositions : IJobForEachWithEntity<Target>
        {
            [WriteOnly] public NativeArray<float3> targetPositions;
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> targetWorldTransforms;

            public void Execute(Entity entity, int index, [ReadOnly] ref Target target)
            {
                targetPositions[index] = target.Value != Entity.Null ? targetWorldTransforms[target.Value].Position : new float3(0f, 0f, 0f);
            }
        }

        [BurstCompile]
        struct RemovingFiring : IJobForEach<TargettedTool>
        {
            public void Execute(ref TargettedTool tool)
            {
                tool.Firing = false;
            }
        }

        [BurstCompile]
        struct FireTargettedToolsJob : IJobForEachWithEntity<LocalToWorld, Target, TargettedTool, Cooldown>
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> targetPositions;

            public void Execute(
                Entity attacker,
                int index,
                [ReadOnly] ref LocalToWorld worldTransform,
                [ReadOnly] ref Target target,
                ref TargettedTool tool,
                ref Cooldown cooldown
                )
            {
                if (target.Value == Entity.Null)
                    return;

                if (!tool.Armed)
                    return;

                if (!cooldown.IsReady())
                    return;

                var delta = (targetPositions[index] - worldTransform.Position);

                // Cannot fire if out of weapon range
                if (math.lengthsq(delta) > tool.Range * tool.Range)
                    return;

                // Only fire when target is within weapon cone.
                var projection = math.dot(math.normalize(delta), math.normalize(worldTransform.Forward));
                if (math.cos(tool.Cone / 2f) > projection)
                    return;

                tool.Firing = true;

                // Reset the cooldown
                cooldown.Timer = cooldown.Duration;
            }
        }

        protected override void OnCreateManager()
        {
            m_query = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ReadOnly<Target>(),
                    ComponentType.ReadOnly<TargettedTool>(),
                    ComponentType.ReadWrite<Cooldown>(),
                    ComponentType.ReadOnly<Enabled>(),
                }
            });
        }

        protected EntityQuery m_query;

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            m_targetPositions = new NativeArray<float3>(m_query.CalculateEntityCount(), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var getTargetPositions = new GetTargetPositions
            {
                targetPositions = m_targetPositions,
                targetWorldTransforms = GetComponentDataFromEntity<LocalToWorld>(true)
            }.Schedule(m_query, inputDependencies);
            var removeFiring = new RemovingFiring().Schedule(this, inputDependencies);
            var fireToolsJH = new FireTargettedToolsJob()
            {
                targetPositions = m_targetPositions
            }.Schedule(m_query,          
                JobHandle.CombineDependencies(getTargetPositions, removeFiring)
            );
            return fireToolsJH;
        }
    }
}