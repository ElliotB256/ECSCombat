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
    /// Create beam effects between attacker and target entity.
    /// </summary>
    [UpdateInGroup(typeof(AttackResultSystemsGroup))]
    public class BeamEffectSystem : JobComponentSystem
    {
        /// <summary>
        /// Spawns beam effects for each beam-based attack.
        /// </summary>
        //[BurstCompile]
        protected struct CreateBeamEffectJob : IJobForEachWithEntity<Attack, Instigator, Target, BeamEffectStyle, HitLocation, SourceLocation>
        {
            public EntityCommandBuffer.Concurrent buffer;
            public Unity.Mathematics.Random random;

            public void Execute(Entity e, int index, ref Attack attack, ref Instigator attacker, ref Target target, ref BeamEffectStyle style, ref HitLocation hitLoc, ref SourceLocation sourceLoc)
            {
                var laserEffect = new BeamEffect()
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
        protected struct UpdateBeamEffectsJob : IJobForEachWithEntity<BeamEffect, Instigator, Target>
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> worldTransforms;
            public float deltaTime;
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(Entity e, int index, ref BeamEffect beamEffect, ref Instigator attacker, ref Target target)
            {
                if (worldTransforms.Exists(attacker.Value))
                    beamEffect.start = worldTransforms[attacker.Value].Position;

                beamEffect.lifetime -= deltaTime;
                if (beamEffect.lifetime < 0f)
                    buffer.DestroyEntity(index, e);
            }
        }

        private PostAttackEntityBuffer PostEntityBuffer;

        protected override void OnCreateManager()
        {
            PostEntityBuffer = World.GetOrCreateSystem<PostAttackEntityBuffer>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 100000));
            // Create laser effects
            var pos = GetComponentDataFromEntity<LocalToWorld>(true);
            var createJob = new CreateBeamEffectJob() {
                buffer = PostEntityBuffer.CreateCommandBuffer().ToConcurrent(),
                random = random
            };
            var createJobHandle = createJob.Schedule(this, inputDependencies);
            PostEntityBuffer.AddJobHandleForProducer(createJobHandle);

            // Expire beam effects
            var expireJob = new UpdateBeamEffectsJob()
            {
                worldTransforms = pos,
                buffer = PostEntityBuffer.CreateCommandBuffer().ToConcurrent(),
                deltaTime = Time.fixedDeltaTime
            };
            var expireJobHandle = expireJob.Schedule(this, createJobHandle);
            PostEntityBuffer.AddJobHandleForProducer(expireJobHandle);

            return expireJobHandle;
        }
    }
}