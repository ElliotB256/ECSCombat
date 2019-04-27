using Unity.Entities;
using UnityEngine;

namespace Battle.Equipment
{
    [RequiresEntityConversion]
    public class EnabledProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new Enabling();
            dstManager.AddComponentData(entity, data);
        }
    }
}