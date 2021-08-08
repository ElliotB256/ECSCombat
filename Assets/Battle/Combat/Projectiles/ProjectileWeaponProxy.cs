using Battle.Combat.AttackSources;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    public class ProjectileWeaponProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        [Tooltip("Projectile created by this weapon.")]
        public GameObject Projectile;

        [Tooltip("Range of the weapon")]
        public float Range;

        [Tooltip("Is the weapon armed to fire?")]
        public bool Armed;

        [Tooltip("Full attack cone for the weapon, in degrees")]
        public float AttackCone;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            float coneInRad = AttackCone * Mathf.PI / 180f;
            var prefab = conversionSystem.GetPrimaryEntity(Projectile);
            //if (!dstManager.HasComponent<Projectile>(prefab))
            //    throw new Exception("ProjectileWeaponProxy Projectile archetype must have a Projectile component.");
            dstManager.AddComponentData(entity, new ProjectileWeapon { Projectile = prefab });
            dstManager.AddComponentData(entity, new TargettedTool { Armed = Armed, Range = Range, Cone = coneInRad, Firing = false });
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(Projectile);
        }
    }
}