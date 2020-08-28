using Unity.Entities;
using UnityEngine;

namespace Battle.AI
{
    [RequiresEntityConversion]
    public class TurretBehaviourProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float Range;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new TurretBehaviour { Range = Range });
        }
    }
}