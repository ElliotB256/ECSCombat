using System;
using Unity.Entities;

namespace Battle.Equipment
{
    /// <summary>
    /// Marks an entity as engine equipment.
    /// </summary>
    [Serializable]
    public struct Engine : IComponentData
    {
        /// <summary>
        /// Amount of thrust produced by this engine
        /// </summary>
        public float Thrust;

        /// <summary>
        /// Amount of turning thrust produced by this engine.
        /// </summary>
        public float TurnThrust;
    }
}