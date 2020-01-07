using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Battle.Combat
{
    [
        UpdateInGroup(typeof(WeaponSystemsGroup))
        ]
    public class FireProjectileWeaponsSystem : JobComponentSystem
    {
        struct SpawnProjectileJob : IJobForEachWithEntity<LocalToWorld, ProjectileWeapon, Target, Cooldown>
        {
            public EntityCommandBuffer CommandBuffer;

            public void Execute(
                Entity attacker,
                int index,
                [ReadOnly] ref LocalToWorld localToWorld,
                [ReadOnly] ref ProjectileWeapon weapon,
                [ReadOnly] ref Target target,
                ref Cooldown cooldown
                )
            {
                if (target.Value == Entity.Null)
                    return;

                if (!cooldown.IsReady())
                    return;

                cooldown.Timer = cooldown.Duration;

                //var ship = CommandBuffer.Instantiate(hangar.Archetype);
                //CommandBuffer.SetComponent(ship, new Translation { Value = localToWorld.Position - new float3(0f, -0.1f, 0f) });
                //CommandBuffer.SetComponent(ship, new Rotation { Value = new quaternion(localToWorld.Value) });
                //CommandBuffer.SetComponent(ship, target);
                //CommandBuffer.SetComponent(ship, new Instigator { Value = attacker });
            }
        }

        WeaponEntityBufferSystem m_EntityCommandBufferSystem;

        protected override void OnCreate()
        {
            m_EntityCommandBufferSystem = World.GetOrCreateSystem<WeaponEntityBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //var jobHandle = new AircraftHangerSpawnJob { CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer() }.ScheduleSingle(this, inputDeps);
            //m_EntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);
            //return jobHandle;
            return inputDeps;
        }
    }
}