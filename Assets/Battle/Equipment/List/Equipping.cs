using System;
using Unity.Entities;

namespace Battle.Equipment
{
    /// <summary>
    /// Flags a component as in the process of being equipped.
    /// </summary>
    [Serializable]
    public struct Equipping : IComponentData
    {
    }
}