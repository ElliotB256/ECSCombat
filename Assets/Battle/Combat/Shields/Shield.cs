using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// A shield absorbs a portion of incoming damage.
    /// </summary>
    [Serializable]
    public struct Shield : IComponentData
    {
        public float Health;
        public float Radius;
    }
}