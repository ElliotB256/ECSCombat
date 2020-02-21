using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Battle.Combat;
using System;

namespace Battle.AI
{
    /// <summary>
    /// The entity searches for aggressors in the vicinity of the Guard.Target.
    /// </summary>
    [GenerateAuthoringComponent]
    [Serializable]
    public struct GuardBehaviour : IComponentData
    {
        public Entity Target;
    }
}