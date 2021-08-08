using Unity.Entities;
using UnityEngine;

namespace Battle.Combat.Calculations
{
    public class EffectiveRangeProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Tooltip("Start of the range over which effectiveness changes.")]
        public float EffectiveRangeStart = 1f;

        [Tooltip("End of the range over which effectiveness changes.")]
        public float EffectiveRangeEnd = 3f;

        [Tooltip("Effectiveness when far outside the effective range.")]
        public float MinimumEffectiveness = 0.2f;

        [Tooltip("Does effectiveness increase or decrease over the effective range?")]
        public bool IsIncreasing;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity,
                new LinearEffectiveRange {
                    EffectiveRangeStart = EffectiveRangeStart,
                    EffectiveRangeEnd = EffectiveRangeEnd,
                    MinimumEffectiveness = MinimumEffectiveness,
                    IsIncreasing = IsIncreasing
                });
        }
    }
}