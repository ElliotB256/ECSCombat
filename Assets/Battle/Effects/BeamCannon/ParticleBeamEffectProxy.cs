using Unity.Entities;
using UnityEngine;

namespace Battle.Effects
{
    public class ParticleBeamEffectProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public GameObject ParticleSystem;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddSharedComponentData(entity, new ParticleBeamEffect { ParticleSystem = ParticleSystem });
        }
    }
}