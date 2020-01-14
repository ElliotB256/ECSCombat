using System;
using Unity.Entities;
using Unity.Rendering;

namespace Battle.Effects
{
    [Serializable]
    [MaterialProperty("_HitTimer", MaterialPropertyFormat.Float)]
    public struct LastHitTimer : IComponentData
    {
        public float Value;
    }
}
