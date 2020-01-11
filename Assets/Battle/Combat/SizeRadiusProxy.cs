using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    [RequiresEntityConversion]
    public class SizeRadiusProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float Size;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new SizeRadius { Value = Size });
        }
    }
}