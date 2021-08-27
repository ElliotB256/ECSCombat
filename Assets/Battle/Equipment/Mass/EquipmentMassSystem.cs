using Battle.Movement;
using Unity.Entities;
using Unity.Transforms;

namespace Battle.Equipment
{
    /// <summary>
    /// Modifies Mass as entities are added.
    /// </summary>
    [UpdateInGroup(typeof(EquipmentUpdateGroup))]
    public class EquipmentMassSystem : SystemBase
    {
        protected override void OnUpdate ()
        {
            var parentData = GetComponentDataFromEntity<Parent>( isReadOnly:true );
            var massData = GetComponentDataFromEntity<Mass>( isReadOnly:false );

            Entities
                .WithAll<Enabling>()
                .WithReadOnly(parentData)
                .ForEach( ( in EquipmentMass equipmentMass , in Parent parent ) =>
                {
                    Parent ship = parent;
                    while( parentData.HasComponent(ship.Value) )
                        ship = parentData[ship.Value];
                    
                    Mass mass = massData[ship.Value];
                    mass.Value += equipmentMass.MassFractionalIncrease * mass.Base;
                    massData[ship.Value] = mass;
                } )
                .WithBurst()
                .ScheduleParallel();
        }
    }
}