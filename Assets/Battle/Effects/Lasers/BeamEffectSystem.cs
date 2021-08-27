using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Rendering;

using Battle.Combat;

namespace Battle.Effects
{
    /// <summary>
    /// Create beam effects between attacker and target entity.
    /// </summary>
    [UpdateInGroup(typeof(AttackResultSystemsGroup))]
    [UpdateAfter(typeof(ShieldsAbsorbDamageSystem))]
    public class BeamEffectSystem : SystemBase
    {

        PostAttackEntityBuffer _commandBufferSystem;

        protected override void OnCreate ()
        {
            _commandBufferSystem = World.GetOrCreateSystem<PostAttackEntityBuffer>();
        }

        protected override void OnUpdate ()
        {
            uint seed = (uint)UnityEngine.Random.Range(1,100000);
            var commands = _commandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithName("CreateBeamEffects")
                .ForEach( (
                    Entity e ,
                    int entityInQueryIndex ,
                    in Attack attack ,
                    in Instigator attacker ,
                    in Target target ,
                    in BeamEffectStyle style ,
                    in HitLocation hitLoc ,
                    in SourceLocation sourceLoc
                ) =>
                {
                    var laserEffect = new BeamEffect()
                    {
                        start = sourceLoc.Position,
                        end = hitLoc.Position,
                        lifetime = 0.2f
                    };

                    if( attack.Result==Attack.eResult.Miss )
                    {
                        var rnd = new Random( seed + (uint)(entityInQueryIndex*1000) );
                        var delta = new float3(rnd.NextFloat() - 0.5f, 0f, rnd.NextFloat() - 0.5f);
                        laserEffect.end = laserEffect.end + 6f * delta;
                    }

                    Entity effect = commands.CreateEntity(entityInQueryIndex);
                    commands.AddComponent(entityInQueryIndex, effect, laserEffect);
                    commands.AddComponent(entityInQueryIndex, effect, style);
                    commands.AddComponent(entityInQueryIndex, effect, attacker);
                    commands.AddComponent(entityInQueryIndex, effect, target);
                } )
                .WithBurst()
                .ScheduleParallel();

            // Update existing beam effects, eg to follow target or despawn.
            var ltwData = GetComponentDataFromEntity<LocalToWorld>( isReadOnly:true );
            float deltaTime = GetSingleton<GameTimeDelta>().dT;
            Entities
                .WithName("update_beam_effects_job")
                .WithReadOnly(ltwData)
                .ForEach( ( Entity e , int entityInQueryIndex , ref BeamEffect beamEffect , in Instigator attacker ) =>
                {
                    if (ltwData.HasComponent(attacker.Value))
                        beamEffect.start = ltwData[attacker.Value].Position;

                    beamEffect.lifetime -= deltaTime;
                    if (beamEffect.lifetime < 0f)
                        commands.DestroyEntity(entityInQueryIndex, e);
                } )
                .WithBurst()
                .ScheduleParallel();

            _commandBufferSystem.AddJobHandleForProducer(Dependency);
        }

    }
}
