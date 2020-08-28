using System;
using Unity.Entities;

namespace Battle.AI
{
    /// <summary>
    /// A set of hints as to how an entity should choose targets.
    /// </summary>
    [Serializable]
    [GenerateAuthoringComponent]
    public struct TargetingOrders : IComponentData
    {
        /// <summary>
        /// The categories that we prefer to engage.
        /// </summary>
        public AgentCategory.eType Preferred;

        /// <summary>
        /// The categories that we do not want to engage.
        /// </summary>
        public AgentCategory.eType Discouraged;

        public bool TargetSameTeam;
    }
}