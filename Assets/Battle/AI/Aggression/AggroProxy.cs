using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Battle.AI
{
    [RequiresEntityConversion]
    public class AggroProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float Radius = 10f;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new AggroRadius { Value = Radius });
            dstManager.AddComponentData(entity, new AggroLocation());
        }
    }

    /// <summary>
    /// Distance from which this entity will engage another entity.
    /// </summary>
    [Serializable]
    public struct AggroRadius : IComponentData
    {
        public float Value;

        public const float MAX_AGGRO_RADIUS = 80f;
    }

    /// <summary>
    /// The location from which targets should be sought
    /// </summary>
    [Serializable]
    public struct AggroLocation : IComponentData
    {
        public float3 Position;
    }
}