using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Battle.Effects
{
    /// <summary>
    /// Everything required to render a beam effect.
    /// </summary>
    [Serializable]
    public struct BeamEffect : IComponentData
    {
        public float3 start;
        public float3 end;
        public float lifetime;
    }
}