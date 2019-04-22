using Battle.Combat.AttackSources;
using Battle.Combat.Calculations;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Battle.Combat.AttackSources
{
    /// <summary>
    /// Fires all direct weapons that are armed and in range of their target.
    /// </summary>
    [
        UpdateInGroup(typeof(WeaponSystemsGroup))
        ]
    public class FireDirectWeaponsSystem : JobComponentSystem
    {
        //[BurstCompile]
        struct FireDirectWeaponsJob : IJobForEachWithEntity<LocalToWorld, Target, DirectWeapon, Cooldown>
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> worldTransforms;
            public EntityCommandBuffer.Concurrent buffer;

            public void Execute(
                Entity attacker,
                int index,
                [ReadOnly] ref LocalToWorld worldTransform,
                [ReadOnly] ref Target target,
                ref DirectWeapon weapon,
                ref Cooldown cooldown
                )
            {
                if (!weapon.Armed)
                    return;

                if (!cooldown.IsReady())
                    return;

                if (target.Value == Entity.Null)
                    return;

                // Only fire when target is within weapon cone.
                var delta = math.normalize(worldTransforms[target.Value].Position - worldTransform.Position);
                var projection = math.dot(delta, math.normalize(worldTransform.Forward));
                if (math.cos(weapon.AttackCone / 2f) > projection)
                    return;

                // Create the attack.
                Entity attack = buffer.Instantiate(index, weapon.AttackTemplate);
                buffer.AddComponent(index, attack, new Attack());
                buffer.AddComponent(index, attack, target);
                buffer.AddComponent(index, attack, new Instigator() { Value = attacker });
                buffer.AddComponent(index, attack, new EffectSourceLocation { Value = worldTransform.Position });
                buffer.AddComponent(index, attack, new Effectiveness { Value = 1f });

                // Reset the cooldown
                cooldown.Timer = cooldown.Duration;
            }
        }

        private WeaponEntityBufferSystem m_entityBufferSystem;

        protected override void OnCreateManager()
        {
            m_entityBufferSystem = World.GetOrCreateSystem<WeaponEntityBufferSystem>();
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