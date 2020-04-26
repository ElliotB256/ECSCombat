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
    [
        UpdateInGroup(typeof(AttackResultSystemsGroup)),
        UpdateAfter(typeof(ShieldsAbsorbDamageSystem))
        ]
    public class BeamEffectSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var random = new Unity.Mathematics.Random((uint)Random.Range(1, 100000));
            var buffer = PostAttackEntityBuffer.CreateCommandBuffer().ToConcurrent();

            Dependency = Entities.ForEach(
                (Entity e, int entityInQueryIndex, in Attack attack, in Instigator attacker, in Target target, in BeamEffectStyle style, in HitLocation hitLoc, in SourceLocation sourceLoc) =>
                {
                        var laserEffect = new BeamEffect()
                        {
                            start = sourceLoc.Position,
                            end = hitLoc.Position,
                            lifetime = 0.2f
                        };

                        if (attack.Result == Attack.eResult.Miss)
                        {
                            var delta = new Unity.Mathematics.float3(random.NextFloat() - 0.5f, 0f, random.NextFloat() - 0.5f);
                            laserEffect.end = laserEffect.end + 6f * delta;
                        }

                        Entity effect = buffer.CreateEntity(entityInQueryIndex);
                        buffer.AddComponent(entityInQueryIndex, effect, laserEffect);
                        buffer.AddComponent(entityInQueryIndex, effect, style);
                        buffer.AddComponent(entityInQueryIndex, effect, attacker);
                        buffer.AddComponent(entityInQueryIndex, effect, target);
                }
                )
                .WithName("CreateBeamEffects")
                .Schedule(Dependency);

            // Update existing beam effects, eg to follow target or despawn.
            buffer = PostAttackEntityBuffer.CreateCommandBuffer().ToConcurrent();
            var worldTransforms = GetComponentDataFromEntity<LocalToWorld>(true);
            var deltaTime = Time.DeltaTime;
            Dependency = Entities
                .ForEach(
                (Entity e, int entityInQueryIndex, ref BeamEffect beamEffect, in Instigator attacker) =>
                {
                    if (worldTransforms.Exists(attacker.Value))
                        beamEffect.start = worldTransforms[attacker.Value].Position;

                    beamEffect.lifetime -= deltaTime;
                    if (beamEffect.lifetime < 0f)
                        buffer.DestroyEntity(entityInQueryIndex, e);
                }
                )
                .WithName("UpdateBeamEffects")
                .WithReadOnly(worldTransforms)
                .Schedule(Dependency);

            //Required as a workaround - https://issuetracker.unity3d.com/issues/job-complete-must-be-called-before-scheduling-another-job-with-the-same-jobhandle
            Dependency.Complete();

            PostAttackEntityBuffer.AddJobHandleForProducer(Dependency);
        }

        private PostAttackEntityBuffer PostAttackEntityBuffer;

        protected override void OnCreate()
        {
            PostAttackEntityBuffer = World.GetOrCreateSystem<PostAttackEntityBuffer>();
        }
    }
}