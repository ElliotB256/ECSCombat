using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Battle.Combat
{
    /// <summary>
    /// Reduces the timers of all active Cooldowns towards a value of zero.
    /// </summary>
    public class ReloadCooldownSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dT = GetSingleton<GameTimeDelta>().dT;
            Entities
                .ForEach(
                (ref Cooldown cooldown) =>
                {
                    if (!cooldown.IsReady())
                        cooldown.Timer = math.max(0f, cooldown.Timer - dT);
                })
                .ScheduleParallel();
        }
    }
}