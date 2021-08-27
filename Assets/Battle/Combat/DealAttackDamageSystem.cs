using Battle.Effects;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;

namespace Battle.Combat
{
    /// <summary>
    /// Deals damage for all Attack entities with a Damage component.
    /// </summary>
    [UpdateInGroup(typeof(AttackResultSystemsGroup))]
    public class DealAttackDamageSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var lastHitTimerData = GetComponentDataFromEntity<LastHitTimer>( isReadOnly:false );
            var healthData = GetComponentDataFromEntity<Health>( isReadOnly:false );
            var lastHitColorData = GetComponentDataFromEntity<LastHitColor>( isReadOnly:false );

            Entities
                .ForEach( ( in Attack attack , in Target target , in Damage damage ) =>
                {
                    if( attack.Result==Attack.eResult.Miss )
                        return;

                    if( lastHitTimerData.HasComponent(target.Value) && damage.Value>0f )
                        lastHitTimerData[target.Value] = new LastHitTimer{ Value = 0f };
                    if( lastHitColorData.HasComponent(target.Value) && damage.Value>0f )
                        lastHitColorData[target.Value] = new LastHitColor{ Value = new float4(1f,1f,1f,1f) };

                    if( healthData.HasComponent(target.Value) )
                    {
                        Health health = healthData[target.Value];
                        health.Value -= damage.Value;
                        healthData[target.Value] = health;
                    }
                } )
                .WithBurst()
                .Schedule();
        }
    }
}