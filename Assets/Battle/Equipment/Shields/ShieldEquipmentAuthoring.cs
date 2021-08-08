using Battle.Equipment;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class ShieldEquipmentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float HealthPercentBonus;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new ShieldEquipment { HealthFractionBonus = HealthPercentBonus / 100f });
    }
}
