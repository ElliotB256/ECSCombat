using Battle.Combat;
using Battle.Movement;
using Unity.Entities;
using UnityEngine;

namespace Battle.Equipment
{
    [ExecuteAlways]
    [UpdateAfter(typeof(PostAttackEntityBuffer))]
    [UpdateBefore(typeof(EquipmentUpdateGroup))]
    public class EarlyEquipmentBufferSystem : EntityCommandBufferSystem
    {
    }
}
