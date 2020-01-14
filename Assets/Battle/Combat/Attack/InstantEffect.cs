using System;
using Unity.Entities;

namespace Battle.Combat.AttackSources
{
    /// <summary>
    /// Applies an effect instantly, without eg a projectile.
    /// </summary>
    [Serializable]
    public struct InstantEffect : IComponentData
    {
        /// <summary>
        /// Attack template spawned by this weapon.
        /// </summary>
        public Entity AttackTemplate;

        /// <summary>
        /// Accuracy of the direct weapon
        /// </summary>
        public float Accuracy;
    }
}