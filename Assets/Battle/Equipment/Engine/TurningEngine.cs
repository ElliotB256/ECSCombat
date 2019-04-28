using System;
using Unity.Entities;

namespace Battle.Equipment
{
    /// <summary>
    /// Marks an entity as engine equipment.
    /// </summary>
    [Serializable]
    public struct TurningEngine : IComponentData
    {
        /// <summary>
        /// Turn rate in radians
        /// </summary>
        public float TurnSpeedRadians;
    }
}