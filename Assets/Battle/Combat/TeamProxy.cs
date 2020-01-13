using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Battle.Combat
{
    [RequiresEntityConversion]
    public class TeamProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public byte TeamID = 0;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new Team { ID = TeamID };
            dstManager.AddComponentData(entity, data);

            // Define team colors
            float4 color;
            switch (TeamID)
            {
                default: color = new float4(1.0f, 1.0f, 1.0f, 1.0f); break;
                case 1: color = new float4(0.5f, 0.7f, 1.0f, 1.0f); break;
                case 2: color = new float4(1.0f, 0.0f, 0.0f, 1.0f); break;
            }
            dstManager.AddComponentData(entity, new MaterialColor { Value = color });
        }
    }

    /// <summary>
    /// Team Color associated with the ship.
    /// </summary>
    [Serializable]
    [MaterialProperty("_TeamColor", MaterialPropertyFormat.Float4)]
    public struct TeamColor : IComponentData
    {
        public float4 Color;
    }
}