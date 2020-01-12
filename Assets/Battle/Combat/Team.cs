using System;
using Unity.Entities;
using Unity.Rendering;

namespace Battle.Combat
{
    /// <summary>
    /// Team this entity is associated with
    /// </summary>
    [Serializable]
    public struct Team : IComponentData
    {
        public byte ID;
    }
}