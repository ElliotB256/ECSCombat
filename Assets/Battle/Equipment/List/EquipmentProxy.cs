using Unity.Entities;
using UnityEngine;

namespace Battle.Equipment
{
    public class EquipmentProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Equipment());
        }
    }
}