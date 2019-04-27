using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    [RequiresEntityConversion]
    public class HealthProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float MaxHealth;
        public bool IsMortal = true;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Health { Value = MaxHealth });
            if (IsMortal)
                dstManager.AddComponentData(entity, new Mortal());
        }
    }
}