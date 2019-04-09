using System;
using Unity.Entities;

namespace Battle.AI
{
    /// <summary>
    /// An entity, having gotten too close, pulls an evasive manoeuvre to get some distance.
    /// </summary>
    [Serializable]
    public struct EvasiveManoeuvre : IComponentData
    {
        byte dummy;
    }
}