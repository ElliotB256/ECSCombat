using System;
using Unity.Entities;

namespace Battle.Equipment
{
    /// <summary>
    /// Marks an entity as engine equipment.
    /// </summary>
    [Serializable]
    public struct TurningEngine : IComponentData, ICombineable<TurningEngine>
    {
        /// <summary>
        /// Turn rate in radians
        /// </summary>
        public float TurnSpeedRadians;

        public void Combine(TurningEngine other)
        {
            TurnSpeedRadians += other.TurnSpeedRadians;
        }

        public void Decombine(TurningEngine other)
        {
            TurnSpeedRadians -= other.TurnSpeedRadians;
        }
    }
}