using System;
using Unity.Entities;

namespace Battle.Movement
{
    /// <summary>
    /// The weight of an entity
    /// </summary>
    [Serializable]
    public struct Thrust : IComponentData
    {
        /// <summary>
        /// Current value of forward thrust.
        /// </summary>
        public float Forward;

        /// <summary>
        /// Base value of forward thrust.
        /// </summary>
        public float ForwardBase;

        /// <summary>
        /// Current value of turning thrust.
        /// </summary>
        public float Turning;

        /// <summary>
        /// Base value of turning thrust.
        /// </summary>
        public float TurningBase;
    }
}