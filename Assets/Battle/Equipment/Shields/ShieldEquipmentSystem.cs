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
        protected override void OnUpdate()
        {
            Entities
                .WithAll<Enabling>()
                .ForEach(
                (in ShieldEquipment shieldEquipment, in Parent parent) =>
                {
                    var maxHealth = GetComponent<MaxHealth>(parent.Value);
                    var shieldHP = shieldEquipment.HealthFractionBonus * maxHealth.Base;

                    var shield = GetComponent<Shield>(parent.Value);
                    var maxShield = GetComponent<MaxShield>(parent.Value);
                    shield.Health += shieldHP;
                    maxShield.Value += shieldHP;
                    SetComponent(parent.Value, shield);
                    SetComponent(parent.Value, maxShield);
                }
                ).Schedule();

            Entities
                .WithAll<Disabling>()
                .ForEach(
                (in ShieldEquipment shieldEquipment, in Parent parent) =>
                {
                    var maxHealth = GetComponent<MaxHealth>(parent.Value);
                    var shieldHP = shieldEquipment.HealthFractionBonus * maxHealth.Base;

                    var shield = GetComponent<Shield>(parent.Value);
                    var maxShield = GetComponent<MaxShield>(parent.Value);
                    shield.Health = math.max(0f, shield.Health - shieldHP);
                    maxShield.Value = math.max(0f, maxShield.Value - shieldHP);
                    SetComponent(parent.Value, shield);
                    SetComponent(parent.Value, maxShield);
                }
                ).Schedule();
        }
    }
}