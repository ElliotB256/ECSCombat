using System;
using Unity.Entities;
using UnityEngine;

namespace Battle.Movement
{
    /// <summary>
    /// Indicates max turning speed of an entity
    /// </summary>
    [Serializable]
    [GenerateAuthoringComponent]
    public struct MaxTurnSpeed : IComponentData
    {
        public float RadiansPerSecond;
    }
}