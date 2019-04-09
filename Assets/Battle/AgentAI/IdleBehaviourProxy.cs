using Unity.Entities;
using UnityEngine;

namespace Battle.AI
{
    [RequiresEntityConversion]
    public class IdleBehaviourProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new IdleBehaviour { });
        }
    }
}