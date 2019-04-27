using Unity.Entities;
using Battle.Combat;
using Battle.Movement;

namespace Battle.Equipment
{
    /// <summary>
    /// A group for systems which modify the state of attacks.
    /// </summary>
    [UpdateAfter(typeof(EarlyEquipmentBufferSystem))]
    [UpdateBefore(typeof(MovementUpdateSystemsGroup))]
    public class EquipmentUpdateGroup : ComponentSystemGroup
    {
    }
}
