using System;
using Unity.Entities;

namespace Battle.Movement
{
    /// <summary>
    /// Direction/facing of an entity
    /// </summary>
    [Serializable]
    public struct Heading : IComponentData
    {
        public float Value;
    }
}