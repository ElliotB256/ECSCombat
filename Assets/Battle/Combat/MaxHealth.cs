using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// Max health of an entity
    /// </summary>
    [Serializable]
    public struct MaxHealth : IComponentData
    {
        public float Value;
        public float Base;
    }
}