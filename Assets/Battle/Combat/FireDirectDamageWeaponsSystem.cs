using Battle.Movement;
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
    /// Fires all direct weapons that are armed and in range of their target.
    /// </summary>
    [UpdateBefore(typeof(AttackEntityBufferSystem))]
    public class FireDirectWeaponsSystem : JobComponentSystem
    {
        //[BurstCompile]
        struct FireDirectWeaponsJob : IJobForEachWithEntity<LocalToWorld, Target, Damage, DirectWeapon, Cooldown>
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> worldTransforms;
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(
                Entity attacker,
                int index,
                [ReadOnly] ref LocalToWorld worldTransform,
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

                if (target.Value == Entity.Null || !worldTransforms.Exists(target.Value))
                    return;

                // Only fire when target is within weapon cone.
                var delta = worldTransforms[target.Value].Position - worldTransform.Position;
                var projection = math.dot(delta, worldTransform.Forward);
                if (math.cos(weapon.AttackCone / 2f) > projection)
                    return;
                
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
            var pos = GetComponentDataFromEntity<LocalToWorld>(true);
            var job = new FireDirectWeaponsJob() { buffer = m_entityBufferSystem.CreateCommandBuffer().ToConcurrent(), worldTransforms = pos };
            var jobHandle = job.Schedule(this, inputDependencies);
            m_entityBufferSystem.AddJobHandleForProducer(jobHandle);
            return jobHandle;
        }
    }
}