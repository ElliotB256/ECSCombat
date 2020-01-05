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
    public class FireDirectWeaponsSystem : JobComponentSystem
    {
        protected WeaponEntityBufferSystem m_entityBufferSystem;

        /// <summary>
        /// Holds the world location of each weapon's target entity.
        /// </summary>
        protected NativeArray<float3> m_targetPositions;

        //[BurstCompile]
        struct FireDirectWeaponsJob : IJobForEachWithEntity<LocalToWorld, Target, DirectWeapon, Cooldown>
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> worldTransforms;
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(
                Entity attacker,
                int index,
                [ReadOnly] ref LocalToWorld worldTransform,
                [ReadOnly] ref Target target,
                ref DirectWeapon weapon,
                ref Cooldown cooldown
                )
            {
                if (!weapon.Armed)
                    return;

                if (!cooldown.IsReady())
                    return;

                if (target.Value == Entity.Null)
                    return;

                var delta = (worldTransforms[index] - worldTransform.Position);

                // Cannot fire if out of weapon range
                if (math.lengthsq(delta) > weapon.Range * weapon.Range)
                    return;

                // Only fire when target is within weapon cone.
                var projection = math.dot(math.normalize(delta), math.normalize(worldTransform.Forward));
                if (math.cos(weapon.AttackCone / 2f) > projection)
                    return;

                // Create the attack.
                Entity attack = buffer.Instantiate(index, weapon.AttackTemplate);
                buffer.AddComponent(index, attack, new Attack());
                buffer.AddComponent(index, attack, target);
                buffer.AddComponent(index, attack, new Instigator() { Value = attacker });
                buffer.AddComponent(index, attack, new EffectSourceLocation { Value = worldTransform.Position });
                buffer.AddComponent(index, attack, new Effectiveness { Value = 1f });

                // Reset the cooldown
                cooldown.Timer = cooldown.Duration;
            }
        }

        [BurstCompile]
        struct GetTargetPositions : IJobForEachWithEntity<Target>
        {
            [WriteOnly] public NativeArray<float3> targetPositions;
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> targetWorldTransforms;

            public void Execute(Entity entity, int index, [ReadOnly] ref Target target)
            {
                targetPositions[index] = target.Value != Entity.Null ? targetWorldTransforms[target.Value].Position : new float3(0f,0f,0f);
            }
        }

        protected override void OnCreateManager()
        {
            m_entityBufferSystem = World.GetOrCreateSystem<WeaponEntityBufferSystem>();
            m_query = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ReadOnly<Target>(),
                    ComponentType.ReadOnly<DirectWeapon>(),
                    ComponentType.ReadWrite<Cooldown>(),
                    ComponentType.ReadOnly<Enabled>(),
                }
            });
        }

        protected EntityQuery m_query;

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            m_targetPositions = new NativeArray<float3>(m_query.CalculateEntityCount(), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var getTargetPositionsJH = new GetTargetPositions
            {
                targetPositions = m_targetPositions,
                targetWorldTransforms = GetComponentDataFromEntity<LocalToWorld>(true)
            }.Schedule(m_query, inputDependencies);
            var fireDirectWeaponsHandle = new FireDirectWeaponsJob()
            {
                buffer = m_entityBufferSystem.CreateCommandBuffer().ToConcurrent(),
                worldTransforms = m_targetPositions
            }.Schedule(m_query, getTargetPositionsJH);
            m_entityBufferSystem.AddJobHandleForProducer(fireDirectWeaponsHandle);
            return fireDirectWeaponsHandle;
        }
    }
}