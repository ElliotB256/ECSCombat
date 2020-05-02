using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Battle.Spawner
{
    [GenerateAuthoringComponent]
    [Serializable]
    public struct WaveSpawner : IComponentData
    {
        public float TimeBetweenWaves;
        public float CurrentTime;

        public float2 SpawnerBounds;
    }
}