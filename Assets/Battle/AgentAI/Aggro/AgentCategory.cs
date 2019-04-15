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
            Small,
            Medium,
            Large
        }

        public eType Type;
    }
}