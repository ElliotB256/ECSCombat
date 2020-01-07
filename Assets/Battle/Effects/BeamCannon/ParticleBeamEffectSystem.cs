using Battle.Combat;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Battle.Effects
{
    /// <summary>
    /// Spawns particle beam effects.
    /// </summary>
    [UpdateInGroup(typeof(AttackResultSystemsGroup))]
    public class ParticleBeamEffectSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var positions = GetComponentDataFromEntity<LocalToWorld>(true);

            Entities.ForEach(
                (ParticleBeamEffect effect, ref Attack attack, ref Instigator instigator, ref Target target) =>
                {
                    Vector3 start = positions[instigator.Value].Position;
                    //start = start - (0.01f * Vector3.up);
                    Vector3 end = positions[target.Value].Position;

                    // if attack missed, move the end position randomly.
                    if (attack.Result == Attack.eResult.Miss)
                    {
                        var shift = UnityEngine.Random.insideUnitSphere;
                        shift.y = 0f;
                        end = end + 6f * shift;
                    }

                    Vector3 delta = end - start;
                    float length = delta.magnitude;

                    var go = GameObject.Instantiate(effect.ParticleSystem, start, Quaternion.identity);
                    go.transform.right = delta;

                    // Tune number and particle and length depending on target destination.
                    int number = (int)(20 * length);
                    var system = go.GetComponent<ParticleSystem>();
                    var burst = system.emission.GetBurst(0);
                    burst.count = number;
                    system.emission.SetBurst(0, burst);
                    var shape = system.shape;
                    shape.scale = new Vector3(length / 2f, 0f, 0f);
                    shape.position = new Vector3(length / 2f, 0f, 0f);
                }
            );
        }
    }
}
