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
    [UpdateInGroup(typeof(AttackResultSystemsGroup))]
    public class LaserEffectSystem : JobComponentSystem
    {
        /// <summary>
        /// Spawns laser effects for each laser-based attack.
        /// </summary>
        //[BurstCompile]
        protected struct CreateLaserEffectJob : IJobForEachWithEntity<Attack, Instigator, Target, BeamEffectStyle, HitLocation, SourceLocation>
        {
            public EntityCommandBuffer.Concurrent buffer;
            public Unity.Mathematics.Random random;

            public void Execute(Entity e, int index, ref Attack attack, ref Instigator attacker, ref Target target, ref BeamEffectStyle style, ref HitLocation hitLoc, ref SourceLocation sourceLoc)
            {
                var laserEffect = new LaserBeamEffect()
                {
                    start = sourceLoc.Position,
                    end = hitLoc.Position,
                    lifetime = 0.2f
                };

                // if attack missed, move the end position randomly.
                if (attack.Result == Attack.eResult.Miss)
                {
                    var delta = new Unity.Mathematics.float3(random.NextFloat() - 0.5f, 0f, random.NextFloat() - 0.5f);
                    laserEffect.end = laserEffect.end + 6f * delta;
                }

                Entity effect = buffer.CreateEntity(index);
                buffer.AddComponent(index, effect, laserEffect);
                buffer.AddComponent(index, effect, style);
                buffer.AddComponent(index, effect, new Instigator() { Value = attacker.Value });
                buffer.AddComponent(index, effect, new Target() { Value = target.Value });
            }
        }

        /// <summary>
        /// Updates laser effects, and despawns them once they are dead.
        /// </summary>
        [BurstCompile]
        protected struct UpdateLaserEffectsJob : IJobForEachWithEntity<LaserBeamEffect, Instigator, Target>
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> worldTransforms;
            public float deltaTime;
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(Entity e, int index, ref LaserBeamEffect beamEffect, ref Instigator attacker, ref Target target)
            {
                if (worldTransforms.Exists(attacker.Value))
                    beamEffect.start = worldTransforms[attacker.Value].Position;

                beamEffect.lifetime -= deltaTime;
                if (beamEffect.lifetime < 0f)
                    buffer.DestroyEntity(index, e);
            }
        }

        private PostAttackEntityBuffer m_laserEffectBufferSystem;

        protected override void OnCreateManager()
        {
            m_laserEffectBufferSystem = World.GetOrCreateSystem<PostAttackEntityBuffer>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 100000));
            // Create laser effects
            var pos = GetComponentDataFromEntity<LocalToWorld>(true);
            var createJob = new CreateLaserEffectJob() {
                buffer = m_laserEffectBufferSystem.CreateCommandBuffer().ToConcurrent(),
                random = random
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