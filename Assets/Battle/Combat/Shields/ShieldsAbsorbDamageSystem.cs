using Battle.Effects;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Battle.Combat
{
    [
        UpdateInGroup(typeof(AttackResultSystemsGroup)),
        UpdateBefore(typeof(DealAttackDamageSystem))
        ]
    public class ShieldsAbsorbDamageSystem : SystemBase
    {
        PostAttackEntityBuffer _commandBufferSystem;

        protected override void OnCreate()
        {
            _commandBufferSystem = World.GetOrCreateSystem<PostAttackEntityBuffer>();
        }

        protected override void OnUpdate()
        {
            var commands = _commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            var shieldData = GetComponentDataFromEntity<Shield>( isReadOnly:false );
            var ltwData = GetComponentDataFromEntity<LocalToWorld>( isReadOnly:true );
            
            Entities
                .WithName("enumerate_over_all_attacks_job")
                .WithNone<ShieldBypass>()
                .WithAll<Instigator>()
                .WithReadOnly(ltwData)
                .ForEach(
                (Entity entity,
                int entityInQueryIndex,
                ref Attack attack,
                ref Damage damage,
                ref SourceLocation sourceLocation,
                ref HitLocation hitLocation,
                ref Target target) =>
            {
                if( !shieldData.HasComponent(target.Value) || !ltwData.HasComponent(target.Value) )
                    return;
                var shield = shieldData[target.Value];

                // depleted shields do not block attacks.
                if( shield.Health<=0f )
                    return;

                // if attack comes from within shield there is no shield protection.
                var targetPosition = ltwData[target.Value];
                float3 delta = targetPosition.Position - sourceLocation.Position;
                if( math.lengthsq(delta)<math.pow(shield.Radius,2f) )
                    return;

                // Shield reduces incoming damage.
                float absorbed = math.min( shield.Health , damage.Value );
                shield.Health = shield.Health - absorbed;
                damage.Value = damage.Value - absorbed;
                hitLocation.Position = targetPosition.Position + shield.Radius * -math.normalize(delta);
                shieldData[target.Value] = shield;

                // generate aesthetic effect.
                bool blocked = absorbed > 0f;
                if( blocked )
                {
                    Entity effect = commands.CreateEntity( entityInQueryIndex );
                    commands.AddComponent( entityInQueryIndex, effect, new ShieldHitEffect{ HitDirection = -math.normalize(delta) } );
                    commands.AddComponent( entityInQueryIndex, effect, shield );
                    commands.AddComponent( entityInQueryIndex, effect, targetPosition );
                    commands.AddComponent( entityInQueryIndex, effect, new Delete() );
                }
            } )
            .WithBurst()
            .Schedule();

            _commandBufferSystem.AddJobHandleForProducer( Dependency );
        }
    }
}
