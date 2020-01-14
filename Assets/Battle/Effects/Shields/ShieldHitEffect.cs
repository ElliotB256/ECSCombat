using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Battle.Effects
{
    [Serializable]
    public struct ShieldHitEffect : IComponentData
    {
        public float3 HitDirection;
    }
}
