using Battle.AI;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Battle.Combat
{
    /// <summary>
    /// Spawns ships from the aircraft hangar.
    /// </summary>
    [
        UpdateInGroup(typeof(WeaponSystemsGroup))
        ]
    public class AircraftHangarSystem : SystemBase
    {
        WeaponEntityBufferSystem m_EntityCommandBufferSystem;

        protected override void OnCreate()
        {
            m_EntityCommandBufferSystem = World.GetOrCreateSystem<WeaponEntityBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var buffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();

            Entities
                .ForEach(
                (
                Entity entity,
                int entityInQueryIndex,
                ref LocalToWorld localToWorld,
                ref AircraftHangar hangar,
                ref Team team,
                ref Cooldown cooldown
                ) =>
                {
                    if (!cooldown.IsReady())
                        return;

                    cooldown.Timer = cooldown.Duration;

                    var ship = buffer.Instantiate(entityInQueryIndex, hangar.Archetype);
                    buffer.SetComponent(entityInQueryIndex, ship, new Translation { Value = localToWorld.Position - new float3(0f, -0.1f, 0f) });
                    buffer.SetComponent(entityInQueryIndex, ship, new Rotation { Value = new quaternion(localToWorld.Value) });
                    buffer.SetComponent(entityInQueryIndex, ship, team);
                    buffer.AddComponent(entityInQueryIndex, ship, new Escort { Target = entity });
                })
                .Schedule();

            m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}