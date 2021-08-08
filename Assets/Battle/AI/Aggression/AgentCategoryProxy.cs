using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace Battle.AI
{
    public class AgentCategoryProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Tooltip("Type of this entity.")]
        public AgentCategory.eType AgentType;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var category = new AgentCategory { Type = AgentType };
            dstManager.AddComponentData(entity, category);
        }

#if UNITY_EDITOR
        void OnGUI()
        {
            AgentType = (AgentCategory.eType)EditorGUILayout.EnumFlagsField("Type of this entity", AgentType);
        }
#endif
    }
}