using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Battle.Effects
{
    /// <summary>
    /// A particle beam special effect.
    /// </summary>
    [Serializable]
    public struct ParticleBeamEffect : ISharedComponentData, IEquatable<ParticleBeamEffect>
    {
        /// <summary>
        /// Particle system that is created on death.
        /// </summary>
        public GameObject ParticleSystem;

        public bool Equals(ParticleBeamEffect other)
        {
            return other.ParticleSystem == ParticleSystem;
        }

        public override int GetHashCode()
        {
            return ParticleSystem.GetHashCode();
        }
    }
}