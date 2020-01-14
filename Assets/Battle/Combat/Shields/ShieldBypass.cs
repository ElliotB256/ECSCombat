using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// Attacks that bypass the shield.
    /// </summary>
    [Serializable]
    public struct ShieldBypass : IComponentData
    {
    }
}