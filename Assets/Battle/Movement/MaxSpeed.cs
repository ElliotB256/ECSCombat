using System;
using Unity.Entities;
using UnityEngine;

namespace Battle.Movement
{
    /// <summary>
    /// Maximum speed at which an entity can move.
    /// </summary>
    [Serializable]
    public struct MaxSpeed : IComponentData
    {
        public float Value;
    }
}