using Battle.Equipment;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class EquipmentMassAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("Percentage increase the entitie's mass when the equipment is added.")]
    public float MassPercentageIncrease;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new EquipmentMass { MassFractionalIncrease = MassPercentageIncrease / 100f });
    }
}
