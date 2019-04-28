using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// Represents the size of an entity for combat purposes
    /// </summary>
    [Serializable]
    public struct CombatSize : IComponentData
    {
        public float Value;
    }
}