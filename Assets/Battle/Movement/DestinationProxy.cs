using Unity.Entities;
using UnityEngine;

namespace Battle.Movement
{
    [RequiresEntityConversion]
    public class DestinationProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Vector3 Destination;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new Destination { Value = Destination };
            dstManager.AddComponentData(entity, data);
        }
    }
}