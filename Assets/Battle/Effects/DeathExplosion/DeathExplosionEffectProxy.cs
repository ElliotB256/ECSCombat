using Unity.Entities;
using UnityEngine;

namespace Battle.Effects
{
    public class DeathExplosionEffectProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Tooltip("Particle system generated when the entity dies.")]
        public GameObject ParticleSystem;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddSharedComponentData(entity, new DeathExplosionEffect { ParticleSystem = ParticleSystem });
        }
    }
}