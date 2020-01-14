using Battle.Effects;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.Battle.Effects.Damage
{
    public class IncreaseLastHitTimerSystem : JobComponentSystem
    {
        [BurstCompile]
        struct IncreaseTimers : IJobForEach<LastHitTimer>
        {
            public float dT;
            public void Execute(ref LastHitTimer timer)
            {
                timer.Value = timer.Value + dT;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            return new IncreaseTimers { dT = Time.DeltaTime }.Schedule(this, inputDependencies);
        }
    }
}
