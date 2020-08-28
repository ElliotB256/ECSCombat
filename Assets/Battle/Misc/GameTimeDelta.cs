using System;
using Unity.Entities;

namespace Battle.Combat
{
    [GenerateAuthoringComponent]
    [Serializable]
    public struct GameTimeDelta : IComponentData
    {
        public float dT;
        public float RateFactor;
        public bool Paused;
    }
}