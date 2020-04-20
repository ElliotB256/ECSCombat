using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
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
    public class ProjectileSpawnSystem : SystemBase
    {
        protected WeaponEntityBufferSystem m_entityBufferSystem;

        protected override void OnCreate()
        {
            m_entityBufferSystem = World.GetOrCreateSystem<WeaponEntityBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var buffer = m_entityBufferSystem.CreateCommandBuffer().ToConcurrent();

            Entities
                .ForEach((
                Entity attacker,
                int entityInQueryIndex,
                in Target target,
                in LocalToWorld worldTransform,
                in TargettedTool tool,
                in ProjectileWeapon weapon,
                in Team team
                ) =>
                {
                    if (!tool.Firing)
                        return;

                    // Create the projectile
                    Entity projectile = buffer.Instantiate(entityInQueryIndex, weapon.Projectile);
                    buffer.SetComponent(entityInQueryIndex, projectile, target);
                    buffer.SetComponent(entityInQueryIndex, projectile, new Translation { Value = worldTransform.Position });
                    buffer.SetComponent(entityInQueryIndex, projectile, new Rotation { Value = worldTransform.Rotation });
                    buffer.SetComponent(entityInQueryIndex, projectile, worldTransform);
                    buffer.SetComponent(entityInQueryIndex, projectile, new Instigator() { Value = attacker });
                    buffer.AddComponent(entityInQueryIndex, projectile, team);
                })
                .Schedule();
            m_entityBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}