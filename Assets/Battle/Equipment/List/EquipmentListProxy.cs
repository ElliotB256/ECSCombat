using Unity.Entities;
using UnityEngine;

namespace Battle.Equipment
{
    [RequiresEntityConversion]
    public class EquipmentListProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddBuffer<EquipmentList>(entity);
        }
    }
}