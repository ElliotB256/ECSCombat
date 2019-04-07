using System;
using Unity.Entities;

namespace Battle.Stats
{
    /// <summary>
    /// Health of an entity
    /// </summary>
    [Serializable]
    public class Health : IComponentData
    {
        public float Value;
    }
}