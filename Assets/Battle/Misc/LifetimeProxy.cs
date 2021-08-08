using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    public class LifetimeProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float Lifetime;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Lifetime { Value = Lifetime });
        }
    }
}