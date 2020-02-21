using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Battle.Combat;
using System;

namespace Battle.AI
{
    /// <summary>
    /// A behaviour that periodically recalculates the choice of target.
    /// </summary>
    [GenerateAuthoringComponent]
    [Serializable]
    public struct RetargetBehaviour : IComponentData
    {
        /// <summary>
        /// Interval for which retargetting should occur.
        /// </summary>
        public float Interval;

        /// <summary>
        /// RemainingTime until a retargetting event.
        /// </summary>
        public float RemainingTime;
    }

    /// <summary>
    /// Periodically clears the current target from entities with a RetargettingBehaviour component.
    /// </summary>
    [UpdateBefore(typeof(SelectTargetSystem))]
    public class RetargetBehaviourSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float dT = Time.DeltaTime;
            return Entities.ForEach(
                (ref RetargetBehaviour retarget, ref Target target) =>
                {
                    retarget.RemainingTime -= dT;
                    if (retarget.RemainingTime < 0f)
                    {
                        retarget.RemainingTime = retarget.Interval;
                        target.Value = Entity.Null;
                    }
                }
            ).Schedule(inputDeps);
        }
    }
}