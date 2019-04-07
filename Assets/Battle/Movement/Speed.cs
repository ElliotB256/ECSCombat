using System;
using Unity.Entities;
using UnityEngine;

namespace Battle.Movement
{
    /// <summary>
    /// Indicates current movement speed of an entity.
    /// </summary>
    [Serializable]
    public struct Speed : IComponentData
    {
        public float Value;
    }
}