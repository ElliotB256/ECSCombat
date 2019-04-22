using System;
using Unity.Entities;

namespace Battle.Combat.AttackSources
{
    /// <summary>
    /// A weapon that can attack an entity.
    /// A direct weapon applies the attack instantly, without eg a projectile.
    /// </summary>
    [Serializable]
    public struct DirectWeapon : IComponentData
    {
        /// <summary>
        /// The AttackCone defines the angular width of a cone, in radians, within which the weapon may hit its Target.
        /// </summary>
        public float AttackCone;

        /// <summary>
        /// Range of this direct damage weapon.
        /// </summary>
        public float Range;

        /// <summary>
        /// Whether this weapon is armed. If true, the weapon will attack as soon as possible.
        /// </summary>
        public bool Armed;

        /// <summary>
        /// Attack template spawned by this weapon.
        /// </summary>
        public Entity AttackTemplate;
    }
}