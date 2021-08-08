using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Battle.Spawner
{
    [DisallowMultipleComponent]
    public class SpawnWaveComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public List<GameObject> Templates;
        public List<float> Number;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddBuffer<SpawnWaveComponent>(entity);

            var buffer = dstManager.GetBuffer<SpawnWaveComponent>(entity);

            if (Templates.Count != Number.Count)
                throw new Exception("Template and Number lists must be of matching length.");

            for (int i=0; i < Templates.Count; i++)
            {
                var toSpawn = conversionSystem.GetPrimaryEntity(Templates[i]);
                buffer.Add(new SpawnWaveComponent
                {
                    Number = Number[i],
                    Template = toSpawn
                });
            }
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.AddRange(Templates);
        }
    }
}