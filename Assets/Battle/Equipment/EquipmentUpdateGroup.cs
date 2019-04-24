using Unity.Entities;
using Battle.Combat;
using Battle.Movement;

namespace Battle.Equipment
{
    /// <summary>
    /// A group for systems which modify the state of attacks.
    /// </summary>
    [UpdateAfter(typeof(AttackResultSystemsGroup))]
    [UpdateBefore(typeof(MovementUpdateSystemsGroup))]
    public class EquipmentUpdateGroup : ComponentSystemGroup
    {
    }
}
