using Battle.Combat;
using Battle.Effects;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.Battle.Effects.Damage
{
    public class IncreaseLastHitTimerSystem : SystemBase
    {
        protected override void OnUpdate ()
        {
            float dt = GetSingleton<GameTimeDelta>().dT;
            Entities
                .ForEach( ( ref LastHitTimer timer ) => timer.Value += dt )
                .WithBurst()
                .ScheduleParallel();
        }
    }
}
