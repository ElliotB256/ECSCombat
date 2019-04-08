using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// Instigator of an action
    /// </summary>
    [Serializable]
    public struct Instigator : IComponentData
    {
        public Entity Value;
    }
}