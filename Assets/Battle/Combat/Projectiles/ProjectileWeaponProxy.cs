using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    [RequiresEntityConversion]
    public class ProjectileWeaponProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        [Tooltip("Projectile created by this weapon.")]
        public GameObject Projectile;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var prefab = conversionSystem.GetPrimaryEntity(Projectile);
            if (!dstManager.HasComponent<Projectile>(prefab))
                throw new Exception("ProjectileWeaponProxy Projectile archetype must have a Projectile component.");
            dstManager.AddComponentData(entity, new ProjectileWeapon { Projectile = prefab });
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(Projectile);
        }
    }
}