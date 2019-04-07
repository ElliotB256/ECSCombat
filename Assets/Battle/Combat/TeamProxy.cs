using Unity.Entities;
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
        }
    }
}