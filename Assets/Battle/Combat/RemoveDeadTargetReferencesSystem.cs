using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Battle.Combat
{
    /// <summary>
    /// Sets any Target components that point to an invalid, non-existant entity to Entity.Null
    /// </summary>
    [UpdateAfter(typeof(PostAttackEntityBuffer))]
    public class RemoveDeadTargetReferencesSystem : JobComponentSystem
    {
        [BurstCompile]
        struct RemoveDeadTargetReferencesJob : IJobForEach<Target>
        {
            [ReadOnly] public ComponentDataFromEntity<Targetable> targetables;

            public void Execute(ref Target target)
            {
                //If target cannot be targetted, detarget it.
                if (targetables.Exists(target.Value))
                    return;
                target.Value = Entity.Null;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var targets = GetComponentDataFromEntity<Targetable>(true);
            return new RemoveDeadTargetReferencesJob() { targetables = targets }.Schedule(this, inputDependencies);
        }
    }
}