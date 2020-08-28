using System;
using Unity.Entities;

namespace Battle.Movement
{
    /// <summary>
    /// Maximum speed at which an entity can move.
    /// </summary>
    [Serializable]
    [GenerateAuthoringComponent]
    public struct MaxSpeed : IComponentData
    {
        public float Value;
    }
}