using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Battle.AI
{
    public class TurnToDestinationProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float3 Destination;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new TurnToDestinationBehaviour { Destination = Destination });
        }
    }
}