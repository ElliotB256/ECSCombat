using System;
using Unity.Entities;

namespace Battle.AI
{
    /// <summary>
    /// A set of hints as to how an entity should choose targets.
    /// </summary>
    [Serializable]
    public struct TargetingOrders : IComponentData
    {
        /// <summary>
        /// The categories that we prefer to engage.
        /// </summary>
        public AgentCategory Preferred;

        /// <summary>
        /// The categories that we do not want to engage.
        /// </summary>
        public AgentCategory Discouraged;
    }
}