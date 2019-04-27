using System;
using Unity.Entities;

namespace Battle.Equipment
{
    /// <summary>
    /// Flags an entity to be disabled
    /// </summary>
    [Serializable]
    public struct Disabling : IComponentData
    {
    }
}