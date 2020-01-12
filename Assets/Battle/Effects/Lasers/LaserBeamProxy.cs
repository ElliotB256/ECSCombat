using Unity.Entities;
using UnityEngine;

namespace Battle.Effects
{
    [RequiresEntityConversion]
    public class LaserBeamProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float Width = 0.1f;
        public Color PrimaryColor;
        public Color SecondaryColor;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new BeamEffectStyle { Width = Width });
        }
    }
}