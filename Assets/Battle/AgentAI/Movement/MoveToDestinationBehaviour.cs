using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Battle.AI
{
    /// <summary>
    /// An entity moves towards a given destination.
    /// </summary>
    [Serializable]
    public struct MoveToDestinationBehaviour : IComponentData
    {
        /// <summary>
        /// The target destination the entity is moving towards.
        /// </summary>
        public float3 Destination;
    }
}