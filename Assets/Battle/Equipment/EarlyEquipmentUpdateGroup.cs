using Unity.Entities;
using Battle.Combat;
using Battle.Movement;

namespace Battle.Equipment
{
    [UpdateAfter(typeof(PostAttackEntityBuffer))]
    [UpdateBefore(typeof(EarlyEquipmentBufferSystem))]
    public class EarlyEquipmentUpdateGroup : ComponentSystemGroup
    {
    }
}
