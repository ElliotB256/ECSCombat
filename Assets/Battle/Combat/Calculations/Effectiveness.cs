using System;
using Unity.Entities;

namespace Battle.Combat.Calculations
{
    /// <summary>
    /// The effectiveness of an entity.
    /// </summary>
    [Serializable]
    public struct Effectiveness : IComponentData
    {
        public float Value;
    }
}