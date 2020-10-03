using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Battle.Movement;

namespace Battle.Combat
{
    /// <summary>
    /// Checks if projectiles have reached their target.
    /// </summary>
    [
        UpdateInGroup(typeof(WeaponSystemsGroup))
        ]
    public class ProjectileHitTargetSystem : SystemBase
    {
        WeaponEntityBufferSystem CommandBufferSystem;

        protected override void OnCreate()
        {
            CommandBufferSystem = World.GetOrCreateSystem<WeaponEntityBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var transforms = GetComponentDataFromEntity<LocalToWorld>(true);
            var sizeRadii = GetComponentDataFromEntity<SizeRadius>(true);
            float dT = UnityEngine.Time.fixedDeltaTime;
            var buffer = CommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .ForEach(
                (
                Entity e,
                int entityInQueryIndex,
                ref Projectile projectile,
                in Target target,
                in LocalToWorld transform,
                in Speed speed,
                in TurnSpeed turnSpeed
                ) =>
                {

                    if (!transforms.HasComponent(target.Value))
                        return; // Target does not exist?

                    var tPos = transforms[target.Value].Position;
                    var delta = (tPos - transform.Position);

                    var projectileDistance = speed.Value * dT;

                    float radius = (sizeRadii.HasComponent(target.Value)) ? sizeRadii[target.Value].Value : 0f;

                    // If target out of range, return
                    if (math.length(delta) - radius > projectileDistance)
                        return;

                    // Projectile has reached the target.
                    projectile.ReachedTarget = true;

                    buffer.AddComponent(entityInQueryIndex, e, new Delete());
                    var effect = buffer.Instantiate(entityInQueryIndex, projectile.AttackEntity);
                    buffer.AddComponent(entityInQueryIndex, effect, target);
                    buffer.AddComponent(entityInQueryIndex, effect, new Instigator { Value = e });
                })
                .WithReadOnly(sizeRadii)
                .WithReadOnly(transforms)
                .Schedule();

            CommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}