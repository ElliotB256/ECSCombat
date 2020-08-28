using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Battle.Effects
{
    /// <summary>
    /// Aesthetic properties of a Beam effect.
    /// </summary>
    [Serializable]
    public struct BeamEffectStyle : IComponentData
    {
        public float Width;
        public float4 PrimaryColor;
        //public float SecondaryColor;
        //public float Pattern;
    }
}