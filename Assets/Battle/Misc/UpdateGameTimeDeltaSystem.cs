using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Battle.Combat
{
    [UpdateBefore(typeof(SimulationSystemGroup))]
    public class UpdateGameTimeDeltaSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var faster = Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus);
            var slower = Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus);
            var pause = Input.GetKeyDown(KeyCode.Space);

            float dT = Time.DeltaTime;
            Entities
                .ForEach(
                (ref GameTimeDelta delta) =>
                {
                    if (faster)
                        delta.RateFactor = math.min(2f, delta.RateFactor + 0.5f);
                    if (slower)
                        delta.RateFactor = math.max(0.5f, delta.RateFactor - 0.5f);
                    if (pause)
                        delta.Paused = !delta.Paused;

                    delta.dT = dT * delta.RateFactor * (delta.Paused ? 0f : 1f);
                })
                .Schedule();

            var gameTime = GetSingleton<GameTimeDelta>();
            Entities
                .ForEach(
                (ref GameTimeMaterialProperty material) => {
                    material.Value += gameTime.dT;
                }
                ).Schedule();
        }
    }
}