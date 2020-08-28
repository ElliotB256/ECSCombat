using Battle.Movement;
using Unity.Entities;
using Unity.Transforms;

namespace Battle.Equipment
{
    /// <summary>
    /// Modifies Mass as entities are added.
    /// </summary>
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(EquipmentUpdateGroup))]
    public class EquipmentMassSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<Enabling>()
                .ForEach(
                (in EquipmentMass equipmentMass, in Parent parent) =>
                {
                    var ship = parent;
                    while (HasComponent<Parent>(ship.Value))
                        ship = GetComponent<Parent>(ship.Value);
                    var mass = GetComponent<Mass>(ship.Value);
                    mass.Value += equipmentMass.MassFractionalIncrease * mass.Base;
                    SetComponent(ship.Value, mass);
                }
                ).Schedule();
        }
    }
}