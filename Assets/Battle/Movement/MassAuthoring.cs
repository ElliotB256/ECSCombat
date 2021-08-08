using Unity.Entities;
using UnityEngine;

namespace Battle.Movement
{
    public class MassAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Tooltip("Mass of a ship.")]
        public float Mass = 1.0f;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Mass { Value = Mass, Base = Mass });
        }
    }
}