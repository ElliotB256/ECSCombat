using Battle.Combat.AttackSources;
using Battle.Combat.Calculations;
using Battle.Equipment;
using Battle.Movement;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Battle.Combat.AttackSources
{
    /// <summary>
    /// Applies all instant effects.
    /// </summary>
    [
        UpdateInGroup(typeof(WeaponSystemsGroup)),
        UpdateAfter(typeof(FireTargettedToolsSystem))
        ]
    public class ProjectileSpawnSystem : JobComponentSystem
    {
        protected WeaponEntityBufferSystem m_entityBufferSystem;

        struct ProjectileSpawnJob : IJobForEachWithEntity<Target, LocalToWorld, TargettedTool, ProjectileWeapon, Team>
        {
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(
                Entity attacker,
                int index,
                [ReadOnly] ref Target target,
                [ReadOnly] ref LocalToWorld worldTransform,
                [ReadOnly] ref TargettedTool tool,
                [ReadOnly] ref ProjectileWeapon weapon,
                [ReadOnly] ref Team team
                )
            {
                if (!tool.Firing)
                    return;

                // Create the projectile
                Entity projectile = buffer.Instantiate(index, weapon.Projectile);
                buffer.SetComponent(index, projectile, target);
                buffer.SetComponent(index, projectile, new Translation { Value = worldTransform.Position });
                buffer.SetComponent(index, projectile, new Rotation { Value = worldTransform.Rotation });
                buffer.SetComponent(index, projectile, worldTransform);
                buffer.SetComponent(index, projectile, new Instigator() { Value = attacker });
                buffer.AddComponent(index, projectile, team);
            }
        }

        protected override void OnCreateManager()
        {
            m_entityBufferSystem = World.GetOrCreateSystem<WeaponEntityBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var spawnJH = new ProjectileSpawnJob()
            {
                buffer = m_entityBufferSystem.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, inputDependencies);
            m_entityBufferSystem.AddJobHandleForProducer(spawnJH);
            return spawnJH;
        }
    }
}