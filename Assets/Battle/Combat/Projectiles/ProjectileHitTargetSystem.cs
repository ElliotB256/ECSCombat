using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Battle.AI;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Battle.Movement;

namespace Battle.Combat
{
    /// <summary>
    /// Checks if projectiles have reached their target.
    /// </summary>
    [
        UpdateBefore(typeof(WeaponEntityBufferSystem)),
        UpdateInGroup(typeof(WeaponSystemsGroup))
        ]
    public class ProjectileHitTargetSystem : JobComponentSystem
    {
        EntityQuery ProjectileQuery;
        WeaponEntityBufferSystem CommandBufferSystem;

        protected override void OnCreate()
        {
            ProjectileQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadWrite<Projectile>(),
                    ComponentType.ReadOnly<Target>(),
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ReadOnly<Speed>(),
                    ComponentType.ReadOnly<TurnSpeed>()
                }
            });
            CommandBufferSystem = World.GetOrCreateSystem<WeaponEntityBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var jobHandle = new CheckProjectileReachedTargetJob
            {
                Transforms = GetComponentDataFromEntity<LocalToWorld>(true),
                Radii = GetComponentDataFromEntity<SizeRadius>(true),
                dT = UnityEngine.Time.fixedDeltaTime,
                CommandBuffer = CommandBufferSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(ProjectileQuery, inputDependencies);
            CommandBufferSystem.AddJobHandleForProducer(jobHandle);
            return jobHandle;
        }

        [BurstCompile]
        struct CheckProjectileReachedTargetJob : IJobForEachWithEntity<Projectile, Target, LocalToWorld, Speed, TurnSpeed>
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Transforms;
            [ReadOnly] public ComponentDataFromEntity<SizeRadius> Radii;
            public float dT;
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(
                Entity e,
                int index,
                ref Projectile projectile,
                [ReadOnly] ref Target target,
                [ReadOnly] ref LocalToWorld transform,
                [ReadOnly] ref Speed speed,
                [ReadOnly] ref TurnSpeed turnSpeed
                )
            {
                if (!Transforms.Exists(target.Value))
                    return; // Target does not exist?

                var tPos = Transforms[target.Value].Position;
                var delta = (tPos - transform.Position);

                var projectileDistance = speed.Value * dT;

                float radius = (Radii.HasComponent(target.Value)) ? Radii[target.Value].Value : 0f;

                // If target out of range, return
                if (math.length(delta) - radius > projectileDistance)
                    return;

                // Check if projectile could steer to reach target.
                //float transverse = math.sqrt(math.lengthsq(delta) - math.pow(math.dot(delta, transform.Forward), 2.0f));
                //float transverseRange = (speed.Value * dT) * (turnSpeed.RadiansPerSecond * dT);
                //if (transverse - radius > transverseRange)
                //    return;

                // Projectile has reached the target.
                projectile.ReachedTarget = true;

                CommandBuffer.AddComponent(index, e, new Delete());
                var effect = CommandBuffer.Instantiate(index, projectile.AttackEntity);
                CommandBuffer.AddComponent(index, effect, target);
                CommandBuffer.AddComponent(index, effect, new Instigator { Value = e });
            }
        }
    }
}