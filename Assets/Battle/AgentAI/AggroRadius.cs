using System;
using Unity.Entities;

namespace Battle.AI
{
    /// <summary>
    /// Distance from which this entity will engage another entity.
    /// </summary>
    [Serializable]
    public struct AggroRadius : IComponentData
    {
        public float Value;

        public const float MAX_AGGRO_RADIUS = 80f;
    }
}