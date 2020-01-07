using Unity.Entities;
using UnityEngine;

namespace Battle.Effects
{
    [RequiresEntityConversion]
    public class LaserBeamProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new LaserBeam());
        }
    }
}