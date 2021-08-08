using Battle.Effects;
using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    public class HealthProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float MaxHealth;
        public bool IsMortal = true;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Health { Value = MaxHealth });
            dstManager.AddComponentData(entity, new MaxHealth { Value = MaxHealth, Base = MaxHealth });
            if (IsMortal)
                dstManager.AddComponentData(entity, new Mortal());
            dstManager.AddComponentData(entity, new LastHitTimer { Value = 0f });
            dstManager.AddComponentData(entity, new LastHitColor { Value = new Unity.Mathematics.float4(1f,1f,1f,1f) });
        }
    }
}