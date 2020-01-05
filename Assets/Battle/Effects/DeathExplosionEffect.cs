using System;
using Unity.Entities;
using UnityEngine;

namespace Battle.Effects
{
    /// <summary>
    /// An explosion that occurs when an entity dies.
    /// </summary>
    [Serializable]
    public struct DeathExplosionEffect : ISharedComponentData, IEquatable<DeathExplosionEffect>
    {
        /// <summary>
        /// Particle system that is created on death.
        /// </summary>
        public GameObject ParticleSystem;

        public bool Equals(DeathExplosionEffect other)
        {
            return other.ParticleSystem == ParticleSystem;
        }

        public override int GetHashCode()
        {
            return ParticleSystem.GetHashCode();
        }
    }
}