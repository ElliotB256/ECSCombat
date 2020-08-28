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
            Entities
                .WithAll<Enabling>()
                .ForEach(
                (in Armor armor, in Parent parent) =>
                {
                    var health = GetComponent<Health>(parent.Value);
                    var maxHealth = GetComponent<MaxHealth>(parent.Value);
                    var fraction = health.Value / maxHealth.Value;

                    var bonusHealth = armor.HealthFractionBonus * maxHealth.Base;
                    maxHealth.Value += bonusHealth;
                    SetComponent(parent.Value, maxHealth);
                    health.Value = fraction * maxHealth.Value;
                    SetComponent(parent.Value, health);
                }
                ).Schedule();

            // Currently no plans to remove armor, so removal not implemented.
        }
    }
}