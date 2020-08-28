using System.Collections.Generic;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace Battle.AI
{
    [RequiresEntityConversion]
    public class TargetingOrdersProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Tooltip("Types of entity we are encouraged to target.")]
        public AgentCategory.eType PreferredTargets;

        [Tooltip("Types of entity we are discouraged from targeting.")]
        public AgentCategory.eType DiscouragedTargets;

        public bool TargetSameTeam = false;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var targetOrders = new TargetingOrders { Discouraged = DiscouragedTargets, Preferred = PreferredTargets, TargetSameTeam = TargetSameTeam };
            dstManager.AddComponentData(entity, targetOrders);
        }

#if UNITY_EDITOR
        void OnGUI()
        {
            PreferredTargets = (AgentCategory.eType)EditorGUILayout.EnumFlagsField("Preferred Targets", PreferredTargets);
            DiscouragedTargets = (AgentCategory.eType)EditorGUILayout.EnumFlagsField("Discouraged Targets", DiscouragedTargets);
        }
#endif
    }
}