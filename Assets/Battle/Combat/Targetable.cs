using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// An entity can be targetted.
    /// </summary>
    [Serializable]
    public struct Targetable : IComponentData
    {
        byte dummy;
    }
}
