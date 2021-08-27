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
    public class ProjectileSpawnSystem : SystemBase
    {
        protected WeaponEntityBufferSystem m_entityBufferSystem;

        protected override void OnCreate()
        {
            m_entityBufferSystem = World.GetOrCreateSystem<WeaponEntityBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var buffer = m_entityBufferSystem.CreateCommandBuffer().AsParallelWriter();

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
                    buffer.SetComponent(entityInQueryIndex, projectile, new Rotation { Value = quaternion.LookRotation(worldTransform.Forward, new float3(0f,1f,0f)) });
                    buffer.SetComponent(entityInQueryIndex, projectile, new Instigator() { Value = attacker });
                    buffer.AddComponent(entityInQueryIndex, projectile, team);
                })
                .ScheduleParallel();
            m_entityBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}