using Battle.AI;
using Battle.Movement;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    public class ProjectileProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        [Tooltip("Attack transferred by this projectile to the target.")]
        public GameObject Attack;

        public bool Homing;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var prefab = conversionSystem.GetPrimaryEntity(Attack);
            dstManager.AddComponentData(entity, new Projectile { AttackEntity = prefab, ReachedTarget = false });
            dstManager.AddComponentData(entity, new Instigator());
            dstManager.AddComponentData(entity, new Target());

            if (Homing)
            {
                dstManager.AddComponentData(entity, new Homing());
                dstManager.AddComponentData(entity, new TurnToDestinationBehaviour());
                dstManager.AddComponentData(entity, new Heading());
            }
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(Attack);
        }
    }
}