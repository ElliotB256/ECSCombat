using System;
using Unity.Entities;
using UnityEngine;

namespace Battle.Effects
{
    /// <summary>
    /// An explosion that occurs when an entity dies.
    /// </summary>
    [Serializable]
    public struct DeathExplosionEffect : ISharedComponentData
    {
        /// <summary>
        /// Particle system that is created on death.
        /// </summary>
        public GameObject ParticleSystem;
    }
}