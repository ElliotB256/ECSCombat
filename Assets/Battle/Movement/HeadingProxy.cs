using Unity.Entities;
using UnityEngine;

namespace Battle.Movement
{
    public class HeadingProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new Heading { Value = 0.0f };
            dstManager.AddComponentData(entity, data);
        }
    }
}