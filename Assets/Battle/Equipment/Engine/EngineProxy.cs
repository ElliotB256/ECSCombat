using Unity.Entities;
using UnityEngine;

namespace Battle.Equipment
{
    [RequiresEntityConversion]
    [RequireComponent(typeof(EnabledProxy))]
    public class EngineProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float ForwardThrust;
        public float TurningThrust;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Engine { ForwardThrust = ForwardThrust, TurningThrust = TurningThrust });
        }
    }
}