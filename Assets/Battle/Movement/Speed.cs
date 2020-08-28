using System;
using Unity.Entities;

namespace Battle.Movement
{
    /// <summary>
    /// Indicates current movement speed of an entity.
    /// </summary>
    [Serializable]
    public struct Speed : IComponentData
    {
        public float Value;
    }
}