using Unity.Entities;
using UnityEngine;

namespace Battle.Combat.Calculations
{
    [RequiresEntityConversion]
    public class EvasionProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Tooltip("Base Evasion of this entity. For instance, smaller craft may have higher base ratings.")]
        public float BaseEvasionRating;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity,
                new Evasion
                {
                    BaseRating = BaseEvasionRating,
                    SpeedBonus = 0f,
                    TurnRateBonus = 0f
                });
        }
    }
}