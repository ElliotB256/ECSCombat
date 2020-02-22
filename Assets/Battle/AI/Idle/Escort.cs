using Unity.Entities;
using System;

namespace Battle.AI
{
    /// <summary>
    /// The entity escorts the target when idle.
    /// </summary>
    [Serializable]
    [GenerateAuthoringComponent]
    public struct Escort : IComponentData
    {
        public Entity Target;
    }
}