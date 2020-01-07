using System;
using Unity.Entities;

namespace Battle.Effects
{
    /// <summary>
    /// A laser beam
    /// </summary>
    [Serializable]
    public struct LaserBeam : IComponentData
    {
        byte dummy;
    }
}