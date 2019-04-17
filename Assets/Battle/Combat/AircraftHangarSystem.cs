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
    /// Spawns ships from the aircraft hangar.
    /// </summary>
    [
        UpdateInGroup(typeof(WeaponSystemsGroup))
        ]
    public class AircraftHangarSystem : JobComponentSystem
    {
        struct AircraftHangerSpawnJob : IJobForEachWithEntity<LocalToWorld, AircraftHangar, Team, Cooldown>
        {
            public EntityCommandBuffer CommandBuffer;

            public void Execute(
                Entity entity,
                int index,
                [ReadOnly] ref LocalToWorld localToWorld,
                [ReadOnly] ref AircraftHangar hangar,
                [ReadOnly] ref Team team,
                ref Cooldown cooldown
                )
            {
                if (!cooldown.IsReady())
                    return;

                cooldown.Timer = cooldown.Duration;

                var ship = CommandBuffer.Instantiate(hangar.Archetype);
                CommandBuffer.SetComponent(ship, new Translation { Value = localToWorld.Position - new float3(0f,-0.1f,0f) } );
                CommandBuffer.SetComponent(ship, new Rotation { Value = new quaternion(localToWorld.Value) });
                CommandBuffer.SetComponent(ship, team);
            }
        }

        WeaponEntityBufferSystem m_EntityCommandBufferSystem;

        protected override void OnCreate()
        {
            m_EntityCommandBufferSystem = World.GetOrCreateSystem<WeaponEntityBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var jobHandle = new AircraftHangerSpawnJob { CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer() }.ScheduleSingle(this, inputDeps);
            m_EntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);
            return jobHandle;
        }
    }
}