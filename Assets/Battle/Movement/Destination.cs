using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Battle.Movement
{
    /// <summary>
    /// Indicates a target destination for movement.
    /// </summary>
    [Serializable]
    public struct Destination : IComponentData
    {
        public float3 Value;
    }
}