using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// A weapon that launches projectiles
    /// </summary>
    [Serializable]
    public struct ProjectileWeapon : IComponentData
    {
        /// <summary>
        /// The projectile created by this weapon
        /// </summary>
        public Entity Projectile;
    }
}