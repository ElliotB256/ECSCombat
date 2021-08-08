using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Battle.AI
{
    public class AggroProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float Radius = 10f;

        [Tooltip("Time in seconds between retargetting. 0 to disable.")]
        public float RetargetTime = 0.0f;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new AggroRadius { Value = Radius });
            dstManager.AddComponentData(entity, new AggroLocation());
            if (RetargetTime > 0.0f)
                dstManager.AddComponentData(entity, new RetargetBehaviour { Interval = RetargetTime, RemainingTime = RetargetTime });
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