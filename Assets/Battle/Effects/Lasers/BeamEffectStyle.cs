using System;
using Unity.Entities;

namespace Battle.Effects
{
    /// <summary>
    /// Aesthetic properties of a Beam effect.
    /// </summary>
    [Serializable]
    public struct BeamEffectStyle : IComponentData
    {
        public float Width;
        //public float PrimaryColor;
        //public float SecondaryColor;
        //public float Pattern;
    }
}