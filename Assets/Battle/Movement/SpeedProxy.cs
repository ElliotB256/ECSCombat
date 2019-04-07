using System;
using Unity.Entities;
using UnityEngine;

namespace Battle.Movement
{
    [RequiresEntityConversion]
    public class SpeedProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float MaxSpeed = 1.0f;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Speed { Value = MaxSpeed });
            dstManager.AddComponentData(entity, new MaxSpeed { Value = MaxSpeed });
        }
    }
}