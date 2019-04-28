using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    [RequiresEntityConversion]
    public class CombatSizeProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Tooltip("Characteristic length of the entity for purposes of hit chance in combat.")]
        public float Value = 0f;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new CombatSize { Value = Value });
        }
    }
}