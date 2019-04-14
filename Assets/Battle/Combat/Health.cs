using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// Health of an entity
    /// </summary>
    [Serializable]
    public struct Health : IComponentData
    {
        public float Value;
    }
}