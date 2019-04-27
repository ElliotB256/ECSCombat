using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// Is this entity mortal
    /// </summary>
    [Serializable]
    public struct Mortal : IComponentData
    {
    }
}