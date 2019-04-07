using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// The target of this entity
    /// </summary>
    [Serializable]
    public struct Target : IComponentData
    {
        public Entity Value;
    }
}