using System;
using Unity.Entities;

namespace Battle.Spawner
{
    [Serializable]
    public struct SpawnWave : IBufferElementData
    {
        public Entity Wave;
    }
}