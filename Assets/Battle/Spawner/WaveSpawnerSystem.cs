using Battle.Combat;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = UnityEngine.Random;

namespace Battle.Spawner
{
    [UpdateInGroup(typeof(SpawnSystemGroup))]
    public class WaveSpawnerSystem : SystemBase
    {
        EntityCommandBufferSystem BufferSystem;

        protected override void OnCreate()
        {
            BufferSystem = World.GetOrCreateSystem<SpawnSystemEntityBuffer>();
        }

        protected override void OnUpdate()
        {
            var buffer = BufferSystem.CreateCommandBuffer();

            float dt = Time.DeltaTime;
            Entities.WithNone<SpawnWaveComponent>().ForEach(
                (
                    ref WaveSpawner spawner,
                    in DynamicBuffer<SpawnWave> waves,
                    in Translation translation,
                    in Team team
                ) =>
                {
                    spawner.CurrentTime -= dt;

                    if (spawner.CurrentTime > 0f)
                        return;

                    //Spawn a random wave
                    spawner.CurrentTime = spawner.TimeBetweenWaves;

                    var waveToSpawn = waves[Random.Range(0, waves.Length)].Wave;
                    var spawnedWave = buffer.Instantiate(waveToSpawn);
                    buffer.SetComponent(spawnedWave, translation);
                    buffer.AddComponent(spawnedWave, team);
                    buffer.AddComponent(spawnedWave, spawner);
                }
                )
                .Run();

            Entities.ForEach(
            (
                Entity wave,
                in WaveSpawner spawner,
                in DynamicBuffer<SpawnWaveComponent> waveComponents,
                in Translation translation,
                in Team team
            ) =>
            {
                // Spawn entities from wave
                for (int ci = 0; ci < waveComponents.Length; ci++)
                {
                    SpawnWaveComponent c = waveComponents[ci];
                    for (int i= 0; i < c.Number; i++)
                    {
                        var spawnedEntity = buffer.Instantiate(c.Template);
                        buffer.SetComponent(spawnedEntity,
                            new Translation {
                                Value = translation.Value
                                + new float3(
                                     Random.Range(-spawner.SpawnerBounds.x, spawner.SpawnerBounds.x) / 2f,
                                     0f,
                                     Random.Range(-spawner.SpawnerBounds.y, spawner.SpawnerBounds.y) / 2f
                                     )
                            });
                        buffer.SetComponent(spawnedEntity, team);
                    }
                }

                // Finally, delete the spawned wave.
                buffer.DestroyEntity(wave);
            }
            )
            .Run();
        }
    }
}
