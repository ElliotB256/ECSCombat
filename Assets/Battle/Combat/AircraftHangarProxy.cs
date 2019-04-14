using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    [RequiresEntityConversion]
    public class AircraftHangarProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        [Tooltip("Entity type spawned by this hangar.")]
        public GameObject Archetype;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            // word of warning: this will create a prefab entity for every hangar, which might inefficient if you have thousands of hangars.
            // However, I'm not fussed.
            var prefab = conversionSystem.GetPrimaryEntity(Archetype);
            dstManager.AddComponentData(entity, new AircraftHangar { Archetype = prefab });
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(Archetype);
        }
    }
}
