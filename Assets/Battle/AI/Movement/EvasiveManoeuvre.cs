using System;
using Unity.Entities;

namespace Battle.AI
{
    /// <summary>
    /// An entity, having gotten too close, peels away to get some distance.
    /// </summary>
    [Serializable]
    public struct PeelManoeuvre : IComponentData
    {
    }
}