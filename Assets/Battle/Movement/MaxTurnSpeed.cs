using System;
using Unity.Entities;
using UnityEngine;

namespace Battle.Movement
{
    /// <summary>
    /// Indicates max turning speed of an entity
    /// </summary>
    [Serializable]
    public struct MaxTurnSpeed : IComponentData
    {
        public float RadiansPerSecond;
    }

    [RequiresEntityConversion]
    public class MaxTurnSpeedProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new Speed { Value = 0.0f };
            dstManager.AddComponentData(entity, data);
        }
    }
}