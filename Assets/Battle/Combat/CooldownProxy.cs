using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    public class CooldownProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float Duration = 1.0f;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Cooldown { Duration = Duration, Timer = 0.0f });
        }
    }
}