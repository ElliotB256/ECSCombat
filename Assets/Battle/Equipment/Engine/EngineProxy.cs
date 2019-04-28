using Unity.Entities;
using UnityEngine;

namespace Battle.Equipment
{
    [RequiresEntityConversion]
    [RequireComponent(typeof(EnabledProxy))]
    public class EngineProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float Thrust;

        [Tooltip("Turning rate, in degrees")]
        public float TurningRate;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Engine { Thrust = Thrust });
            dstManager.AddComponentData(entity, new TurningEngine { TurnSpeedRadians = Mathf.Deg2Rad * TurningRate });
        }
    }
}