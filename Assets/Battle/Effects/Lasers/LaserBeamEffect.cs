using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Battle.Effects
{
    /// <summary>
    /// Everything required to render a laser beam.
    /// </summary>
    [Serializable]
    public struct LaserBeamEffect : IComponentData
    {
        public float3 start;
        public float3 end;
        public float lifetime;
    }
}