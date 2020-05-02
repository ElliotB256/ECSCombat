using System;
using Unity.Entities;

namespace Battle.Spawner
{
    [Serializable]
    public struct SpawnWaveComponent : IBufferElementData
    {
        public Entity Template;
        public float Number;
    }
}