using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Battle.AI
{
    /// <summary>
    /// An entity idles within a circular region about the centre.
    /// </summary>
    [Serializable]
    [GenerateAuthoringComponent]
    public struct RandomWalkBehaviour : IComponentData
    {
        public float3 Centre;
        public float Radius;
    }
}