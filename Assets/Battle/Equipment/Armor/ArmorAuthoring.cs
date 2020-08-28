using Battle.Equipment;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class ArmorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float HealthPercentBonus;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Armor { HealthFractionBonus = HealthPercentBonus / 100f});
    }
}
