using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Battle.Combat.Calculations
{
    /// <summary>
    /// A world-space position that denotes the origin of an effect.
    /// 
    /// For instance, for a direct damage weapon, it is the attacker.
    /// For a radial attack, it is the location of the explosion.
    /// </summary>
    [Serializable]
    public struct EffectSourceLocation : IComponentData
    {
        public float3 Value;
    }
}