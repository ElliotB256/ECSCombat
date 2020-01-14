using Unity.Entities;
using UnityEngine;

namespace Battle.Combat.AttackSources
{
    [RequiresEntityConversion]
    public class AttackProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float Accuracy;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, Attack.New(Accuracy));
        }
    }
}