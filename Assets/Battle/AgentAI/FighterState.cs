using System;
using Unity.Entities;

namespace Battle.AI
{
    /// <summary>
    /// State of a fighter AI's state machine.
    /// </summary>
    [Serializable]
    public struct FighterAIState : IComponentData
    {
        public enum eState : byte {
            /// <summary>
            /// An undefined state.
            /// </summary>
            Undefined = 0,
            /// <summary>
            /// The fighter is not currently engaged with an enemy.
            /// </summary>
            Idle = 1,
            /// <summary>
            /// The fighter flees the field.
            /// </summary>
            Flee = 2,
            /// <summary>
            /// The fighter returns to the battle field, having strayed too far.
            /// </summary>
            Return_To_Field = 3,
            /// <summary>
            /// The fighter pursues its target
            /// </summary>
            Pursue = 4,
        }

        /// <summary>
        /// Current state
        /// </summary>
        public eState State;

        /// <summary>
        /// Previous state.
        /// </summary>
        public eState PreviousState;
    }
}