using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    [ExecuteAlways, UpdateAfter(typeof(WeaponSystemsGroup))]
    public class WeaponEntityBufferSystem : EntityCommandBufferSystem
    {
    }
}
