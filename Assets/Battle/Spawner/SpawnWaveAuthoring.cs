using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Battle.Spawner
{
    [DisallowMultipleComponent]
    public class SpawnWaveAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public List<GameObject> SpawnWaves;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddBuffer<SpawnWave>(entity);

            var buffer = dstManager.GetBuffer<SpawnWave>(entity);

            for (int i = 0; i < SpawnWaves.Count; i++)
            {
                var wave = conversionSystem.GetPrimaryEntity(SpawnWaves[i]);
                buffer.Add(new SpawnWave
                {
                    Wave = wave
                });
            }
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.AddRange(SpawnWaves);
        }
    }
}