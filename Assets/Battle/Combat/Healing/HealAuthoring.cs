using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    [RequiresEntityConversion]
    public class HealAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float Heal;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Heal { Value = Heal });
        }
    }
}