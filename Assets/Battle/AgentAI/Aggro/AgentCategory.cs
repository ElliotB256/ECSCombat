using System;
using Unity.Entities;

namespace Battle.AI
{
    /// <summary>
    /// Descriptive category of an entity 
    /// </summary>
    [Serializable]
    public struct AgentCategory : IComponentData
    {
        [Flags] public enum eType : byte
        {
            None = 0,
            Fighters = (1 << 0),
            Frigates = (1 << 1),
            Cruisers = (1 << 2),
            Turrets = (1 << 3),
            Missile = (1 << 4)
        }

        public eType Type;
    }
}