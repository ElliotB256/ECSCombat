using System;
using Unity.Entities;

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