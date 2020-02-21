using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace Battle.AI
{
    [RequiresEntityConversion]
    public class AgentCategoryProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Tooltip("Type of this entity.")]
        public AgentCategory.eType AgentType;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var category = new AgentCategory { Type = AgentType };
            dstManager.AddComponentData(entity, category);
        }

        void OnGUI()
        {
            AgentType = (AgentCategory.eType)EditorGUILayout.EnumFlagsField("Type of this entity", AgentType);
        }
    }
}