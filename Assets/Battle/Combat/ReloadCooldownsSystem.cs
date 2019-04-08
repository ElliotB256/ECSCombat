using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Battle.Combat
{
    /// <summary>
    /// Reduces the timers of all active Cooldowns towards a value of zero.
    /// </summary>
    public class ReloadCooldownSystem : JobComponentSystem
    {
        [BurstCompile]
        struct ReloadCooldownSystemJob : IJobForEach<Cooldown>
        {
            public float DeltaTime;

            public void Execute(
                ref Cooldown cooldown
                )
            {
                if (!cooldown.IsReady())
                    cooldown.Timer = math.max(0f, cooldown.Timer - DeltaTime);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new ReloadCooldownSystemJob()
            {
                DeltaTime = Time.fixedDeltaTime
            };

            return job.Schedule(this, inputDependencies);
        }
    }
}