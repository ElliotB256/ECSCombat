using Unity.Entities;
using UnityEngine;

namespace Battle.AI
{
    [RequiresEntityConversion]
    public class FighterStateProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public FighterAIState.eState State;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new FighterAIState { State = State });
        }
    }
}