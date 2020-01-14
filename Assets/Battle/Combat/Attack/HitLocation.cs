using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Battle.Combat
{
    /// <summary>
    /// The position to which a hit is delivered.
    /// </summary>
    [Serializable]
    public struct HitLocation : IComponentData
    {
        public float3 Position;
    }
}