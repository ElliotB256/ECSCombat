using Battle.Combat;
using Battle.Effects;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.Battle.Effects.Damage
{
    public class IncreaseLastHitTimerSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            float dT = GetSingleton<GameTimeDelta>().dT;
            return Entities.ForEach(
                (ref LastHitTimer timer) =>
                {
                    timer.Value += dT;
                }).Schedule(inputDependencies);
        }
    }
}
