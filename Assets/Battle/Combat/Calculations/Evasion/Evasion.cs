using System;
using Unity.Entities;

namespace Battle.Combat.Calculations
{
    /// <summary>
    /// Evasion rating of an entity.
    /// 
    /// This component indicates that an entity has the ability to evade attacks.
    /// </summary>
    [Serializable]
    public struct Evasion : IComponentData
    {
        /// <summary>
        /// Base evasion rating.
        /// </summary>
        public float BaseRating;

        /// <summary>
        /// Increase in evasion rating due to speed bonus.
        /// </summary>
        public float SpeedBonus;

        /// <summary>
        /// Increase in evasion rating due to turning speed.
        /// </summary>
        public float TurnRateBonus;

        public float GetEvasionRating()
        {
            return BaseRating + SpeedBonus + TurnRateBonus;
        }
    }
}