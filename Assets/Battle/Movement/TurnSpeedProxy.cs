using Unity.Entities;
using UnityEngine;

namespace Battle.Movement
{
    [RequiresEntityConversion]
    public class TurnSpeedProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float MaxRadiansPerSecond = 2.0f;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new TurnSpeed { RadiansPerSecond = 0.0f });
            dstManager.AddComponentData(entity, new MaxTurnSpeed { RadiansPerSecond = MaxRadiansPerSecond });
        }
    }
}