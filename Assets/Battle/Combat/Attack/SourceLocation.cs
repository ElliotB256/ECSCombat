using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Battle.Combat
{
    /// <summary>
    /// The position from which an effect is delivered.
    /// </summary>
    [Serializable]
    public struct SourceLocation : IComponentData
    {
        public float3 Position;
    }
}