using System;
using Unity.Entities;

namespace Battle.Equipment
{
    /// <summary>
    /// Flags a component as in the process of being dequipped.
    /// </summary>
    [Serializable]
    public struct Dequipping : IComponentData
    {
    }
}