using System;
using Unity.Entities;

namespace Battle.Movement
{
    /// <summary>
    /// The weight of an entity
    /// </summary>
    [Serializable]
    public struct Mass : IComponentData
    {
        /// <summary>
        /// Current value of ship mass.
        /// </summary>
        public float Value;

        /// <summary>
        /// Base, unmodified value of the mass.
        /// </summary>
        public float Base;
    }
}