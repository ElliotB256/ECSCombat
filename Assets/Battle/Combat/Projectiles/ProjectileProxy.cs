using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    [RequiresEntityConversion]
    public class ProjectileProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        [Tooltip("Attack transferred by this projectile to the target.")]
        public GameObject Attack;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var prefab = conversionSystem.GetPrimaryEntity(Attack);
            dstManager.AddComponentData(entity, new Projectile { AttackEntity = prefab });
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(Attack);
        }
    }
}