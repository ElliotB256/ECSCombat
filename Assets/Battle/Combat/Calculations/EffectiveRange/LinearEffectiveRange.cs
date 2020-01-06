using System;
using Unity.Entities;

namespace Battle.Combat.Calculations
{
    /// <summary>
    /// An effectiveness that scales linearly from the origin.
    /// </summary>
    [Serializable]
    public struct LinearEffectiveRange : IComponentData
    {
        /// <summary>
        /// Distance at which effective range starts to kick in
        /// </summary>
        public float EffectiveRangeStart;

        /// <summary>
        /// Distance at which effective range stops
        /// </summary>
        public float EffectiveRangeEnd;

        /// <summary>
        /// The minimum value of the effectiveness.
        /// </summary>
        public float MinimumEffectiveness;

        /// <summary>
        /// True if the effectiveness increases from EffectiveRangeStart to EffectiveRangeEnd.
        /// </summary>
        public bool IsIncreasing;
    }
}