using Unity.Entities;
using Unity.Jobs;
using Battle.Combat;
using System;

namespace Battle.AI
{
    /// <summary>
    /// A behaviour that periodically recalculates the choice of target.
    /// </summary>
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
    [UpdateBefore(typeof(SelectTargetsSystem))]
    [UpdateInGroup(typeof(AISystemGroup))]
    public class RetargetBehaviourSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dT = GetSingleton<GameTimeDelta>().dT;
            Dependency = Entities.ForEach(
                (ref RetargetBehaviour retarget, ref Target target) =>
                {
                    retarget.RemainingTime -= dT;
                    if (retarget.RemainingTime < 0f)
                    {
                        retarget.RemainingTime = retarget.Interval;
                        target.Value = Entity.Null;
                    }
                }
            ).Schedule(Dependency);
        }
    }
}