using Unity.Entities;
using UnityEngine;

namespace Battle.AI
{
    [RequiresEntityConversion]
    public class AggroRadiusProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float Radius = 10f;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new AggroRadius { Value = Radius });
        }
    }
}