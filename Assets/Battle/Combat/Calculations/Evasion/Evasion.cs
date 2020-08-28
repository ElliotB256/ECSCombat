using System;
using Unity.Entities;

namespace Battle.Combat.Calculations
{
    /// <summary>
    /// Evasion rating of an entity.
    /// 
    /// This component indicates that an entity has the ability to evade attacks.
    /// </summary>
    [GenerateAuthoringComponent]
    [Serializable]
    public struct Evasion : IComponentData
    {
        /// <summary>
        /// Base evasion rating.
        /// </summary>
        public float Rating;

        /// <summary>
        /// A quantity added/removed from the calculating rating.
        /// </summary>
        public float NaturalBonus;
    }
}