using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// A cooldown for an ability
    /// </summary>
    [Serializable]
    public struct Cooldown : IComponentData
    {
        /// <summary>
        /// The remaining time until the cooldown is ready.
        /// </summary>
        public float Timer;

        /// <summary>
        /// The duration of the cooldown, once triggered.
        /// </summary>
        public float Duration;

        public bool IsReady()
        {
            return Timer <= 0.0f;
        }
    }
}