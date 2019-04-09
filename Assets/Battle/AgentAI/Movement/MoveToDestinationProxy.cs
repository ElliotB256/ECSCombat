using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Battle.AI
{
    [RequiresEntityConversion]
    public class MoveToDestinationProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float3 Destination;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new MoveToDestinationBehaviour { Destination = Destination });
        }
    }
}