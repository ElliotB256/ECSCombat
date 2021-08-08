using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    public class DamageProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float Damage;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Damage { Value = Damage });
        }
    }
}