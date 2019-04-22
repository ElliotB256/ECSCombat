using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Battle.Combat.AttackSources
{
    [RequiresEntityConversion]
    public class DirectWeaponProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        [Tooltip("Ammunition used by this direct weapon.")]
        public GameObject Ammo;

        [Tooltip("Range of the weapon")]
        public float Range;

        [Tooltip("Is the weapon armed to fire?")]
        public bool Armed;

        [Tooltip("Full attack cone for the weapon, in degrees")]
        public float AttackCone;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            float coneInRad = AttackCone * Mathf.PI / 180f;
            var prefab = conversionSystem.GetPrimaryEntity(Ammo);
            dstManager.AddComponentData(entity, new DirectWeapon { Armed = Armed, Range = Range, AttackCone = coneInRad, AttackTemplate = prefab });
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(Ammo);
        }
    }
}