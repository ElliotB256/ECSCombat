using Battle.Combat;
using Unity.Entities;
using Unity.Transforms;

namespace Battle.Equipment
{
    /// <summary>
    /// Modifies max health as armor is added or removed.
    /// </summary>
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(EquipmentUpdateGroup))]
    public class ArmorSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var healthData = GetComponentDataFromEntity<Health>( isReadOnly:false );
            var maxHealthData = GetComponentDataFromEntity<MaxHealth>( isReadOnly:false );

            Entities
                .WithAll<Enabling>()
                .ForEach( ( in Armor armor , in Parent parent ) =>
                {
                    var health = healthData[parent.Value];
                    var maxHealth = maxHealthData[parent.Value];
                    float fraction = health.Value / maxHealth.Value;

                    float bonusHealth = armor.HealthFractionBonus * maxHealth.Base;
                    maxHealth.Value += bonusHealth;
                    maxHealthData[parent.Value] = maxHealth;

                    health.Value = fraction * maxHealth.Value;
                    healthData[parent.Value] = health;
                } )
                .WithBurst()
                .Schedule();

            // Currently no plans to remove armor, so removal not implemented.
        }
    }
}