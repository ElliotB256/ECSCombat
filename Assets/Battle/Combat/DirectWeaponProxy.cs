using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    [RequiresEntityConversion]
    public class DirectWeaponProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float Range;
        public bool Armed;

        [Tooltip("Full attack cone for the weapon, in degrees")]
        public float AttackCone;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            float coneInRad = (float)(AttackCone * Mathf.PI / 180);
            dstManager.AddComponentData(entity, new DirectWeapon { Armed = Armed, Range = Range, AttackCone = coneInRad });
        }
    }
}