using Unity.Entities;
using UnityEngine;

namespace Battle.Movement
{
    [RequiresEntityConversion]
    public class ThrustAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Tooltip("Thrust used to propel the ship.")]
        public float Forward = 0f;

        [Tooltip("Thrust used to turn the ship.")]
        public float Turning = 0f;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, 
                new Thrust {
                    Forward = Forward,
                    ForwardBase = Forward,
                    Turning = Turning,
                    TurningBase = Turning
                });
        }
    }
}