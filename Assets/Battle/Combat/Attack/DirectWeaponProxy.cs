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

        [Tooltip("Base accuracy rating of the weapon")]
        public float Accuracy;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            float coneInRad = AttackCone * Mathf.PI / 180f;
            var prefab = conversionSystem.GetPrimaryEntity(Ammo);
            dstManager.AddComponentData(entity, new InstantEffect { AttackTemplate = prefab, Accuracy = Accuracy });
            dstManager.AddComponentData(entity, new TargettedTool { Armed = Armed, Range = Range, Cone = coneInRad, Firing = false });
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(Ammo);
        }
    }
}