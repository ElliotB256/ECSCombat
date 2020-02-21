using Battle.Combat;
using Unity.Entities;
using UnityEngine;

namespace Battle.AI
{
    /// <summary>
    /// Contains all AI systems.
    /// </summary>
    [ExecuteAlways]
    [UpdateBefore(typeof(WeaponSystemsGroup))]
    [UpdateBefore(typeof(AIStateChangeBufferSystem))]
    public class AISystemGroup : ComponentSystemGroup
    {
    }
}
