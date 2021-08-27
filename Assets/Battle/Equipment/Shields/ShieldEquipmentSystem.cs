using Battle.Combat;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Battle.Equipment
{
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(EquipmentUpdateGroup))]
    public class ShieldEquipmentSystem : SystemBase
    {
        protected override void OnUpdate ()
        {
            var maxHealthData = GetComponentDataFromEntity<MaxHealth>( isReadOnly:true );
            var shieldData = GetComponentDataFromEntity<Shield>( isReadOnly:false );
            var maxShieldData = GetComponentDataFromEntity<MaxShield>( isReadOnly:false );

            Entities
                .WithAll<Enabling>()
                .WithReadOnly( maxHealthData )
                .ForEach( ( in ShieldEquipment shieldEquipment , in Parent parent ) =>
                {
                    MaxHealth maxHealth = maxHealthData[parent.Value];
                    float shieldHP = shieldEquipment.HealthFractionBonus * maxHealth.Base;

                    Shield shield = shieldData[parent.Value];
                    shield.Health += shieldHP;
                    shieldData[parent.Value] = shield;

                    MaxShield maxShield = maxShieldData[parent.Value];
                    maxShield.Value += shieldHP;
                    maxShieldData[parent.Value] = maxShield;
                } )
                .WithBurst()
                .Schedule();

            Entities
                .WithAll<Disabling>()
                .WithReadOnly( maxHealthData )
                .ForEach( ( in ShieldEquipment shieldEquipment , in Parent parent ) =>
                {
                    MaxHealth maxHealth = maxHealthData[parent.Value];
                    float shieldHP = shieldEquipment.HealthFractionBonus * maxHealth.Base;

                    Shield shield = shieldData[parent.Value];
                    shield.Health = math.max(0f, shield.Health - shieldHP);
                    shieldData[parent.Value] = shield;

                    MaxShield maxShield = maxShieldData[parent.Value];
                    maxShield.Value = math.max(0f, maxShield.Value - shieldHP);
                    maxShieldData[parent.Value] = maxShield;
                } )
                .WithBurst()
                .Schedule();
        }
    }
}