using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

using Battle.Combat;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Rendering;

namespace Battle.Effects
{
    /// <summary>
    /// Create laser effects between attacker and target entity.
    /// </summary>
    [
        UpdateBefore(typeof(CleanUpAttacksSystem)),
        UpdateBefore(typeof(LaserEffectBufferSystem)),
        UpdateAfter(typeof(AttackEntityBufferSystem)),
        UpdateBefore(typeof(KillEntitiesWithNoHealthSystem))
    ]
    public class LaserEffectSystem : JobComponentSystem
    {
        /// <summary>
        /// Spawns laser effects for each laser-based attack.
        /// </summary>
        //[BurstCompile]
        protected struct CreateLaserEffectJob : IJobForEachWithEntity<Attack, Instigator, Target>
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> worldTransforms;
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(Entity e, int index, ref Attack attack, ref Instigator attacker, ref Target target)
            {
                var laserEffect = new LaserBeamEffect()
                {
                    start = worldTransforms[attacker.Value].Position,
                    end = worldTransforms[target.Value].Position,
                    lifetime = 0.2f,
                    width = 0.3f
                };

                Entity effect = buffer.CreateEntity(index);
                buffer.AddComponent(index, effect, laserEffect);
                buffer.AddComponent(index, effect, new Instigator() { Value = attacker.Value });
                buffer.AddComponent(index, effect, new Target() { Value = target.Value });
            }
        }

        /// <summary>
        /// Updates laser effects, and despawns them once they are dead.
        /// </summary>
        //[BurstCompile]
        protected struct UpdateLaserEffectsJob : IJobForEachWithEntity<LaserBeamEffect, Instigator, Target>
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> worldTransforms;
            public float deltaTime;
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(Entity e, int index, ref LaserBeamEffect beamEffect, ref Instigator attacker, ref Target target)
            {
                if (worldTransforms.Exists(attacker.Value))
                    beamEffect.start = worldTransforms[attacker.Value].Position;
                if (worldTransforms.Exists(target.Value))
                    beamEffect.end = worldTransforms[target.Value].Position;

                beamEffect.lifetime -= deltaTime;
                if (beamEffect.lifetime < 0f)
                    buffer.DestroyEntity(index, e);
            }
        }

        private LaserEffectBufferSystem m_laserEffectBufferSystem;

        protected override void OnCreateManager()
        {
            m_laserEffectBufferSystem = World.GetOrCreateSystem<LaserEffectBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            // Create laser effects
            var pos = GetComponentDataFromEntity<LocalToWorld>(true);
            var createJob = new CreateLaserEffectJob() {
                worldTransforms = pos,
                buffer = m_laserEffectBufferSystem.CreateCommandBuffer().ToConcurrent()
            };
            var createJobHandle = createJob.Schedule(this, inputDependencies);
            m_laserEffectBufferSystem.AddJobHandleForProducer(createJobHandle);

            // Expire laser effects
            var expireJob = new UpdateLaserEffectsJob()
            {
                worldTransforms = pos,
                buffer = m_laserEffectBufferSystem.CreateCommandBuffer().ToConcurrent(),
                deltaTime = Time.fixedDeltaTime
            };
            var expireJobHandle = expireJob.Schedule(this, createJobHandle);
            m_laserEffectBufferSystem.AddJobHandleForProducer(expireJobHandle);

            return expireJobHandle;
        }
    }
}