using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.Combat
{
    /// <summary>
    /// Fires all direct weapons that are armed and in range of their target.
    /// </summary>
    [UpdateBefore(typeof(AttackEntityBufferSystem))]
    public class FireDirectWeaponsSystem : JobComponentSystem
    {
        //[BurstCompile]
        struct FireDirectWeaponsJob : IJobForEachWithEntity<Translation, Target, Damage, DirectWeapon, Cooldown>
        {
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(
                Entity attacker,
                int index,
                [ReadOnly] ref Translation position,
                [ReadOnly] ref Target target,
                [ReadOnly] ref Damage damage,
                ref DirectWeapon weapon,
                ref Cooldown cooldown
                )
            {
                if (!weapon.Armed)
                    return;

                if (!cooldown.IsReady())
                   return;

                // TODO: fire only when within weapon cone.
                // If we are ready to fire, find out where enemy is.

                // Create the attack.
                Entity attack = buffer.CreateEntity(index);
                buffer.AddComponent(index, attack, new Attack());
                buffer.AddComponent(index, attack, target);
                buffer.AddComponent(index, attack, new Instigator() { Value = attacker });
                buffer.AddComponent(index, attack, damage);

                // Reset the cooldown
                cooldown.Timer = cooldown.Duration;
            }
        }

        private AttackEntityBufferSystem m_entityBufferSystem;

        protected override void OnCreateManager()
        {
            m_entityBufferSystem = World.GetOrCreateSystem<AttackEntityBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new FireDirectWeaponsJob() { buffer = m_entityBufferSystem.CreateCommandBuffer().ToConcurrent() };
            var jobHandle = job.Schedule(this, inputDependencies);
            m_entityBufferSystem.AddJobHandleForProducer(jobHandle);
            return jobHandle;
        }
    }
}