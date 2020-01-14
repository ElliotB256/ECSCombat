using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    [RequiresEntityConversion]
    public class ShieldProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Tooltip("Hit points of this shield.")]
        public float Capacity;

        [Tooltip("Radius of the shield.")]
        public float Radius;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Shield { Health = Capacity, Radius = Radius });
        }
    }
}
