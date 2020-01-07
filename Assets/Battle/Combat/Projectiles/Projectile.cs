using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// A projectile entity carries an attack
    /// </summary>
    [Serializable]
    public struct Projectile : IComponentData
    {
        /// <summary>
        /// The attack transferred by this projectile
        /// </summary>
        public Entity AttackEntity;
    }
}