using Battle.Combat;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Battle.Effects
{
    [UpdateInGroup(typeof(AttackResultSystemsGroup))]
    public class DeathExplosionEffectSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                (DeathExplosionEffect ExplosionEffect, ref Health health, ref LocalToWorld localToWorld) =>
            {
                if (!(health.Value < 0f))
                    return;

                var pos = localToWorld.Position;
                GameObject.Instantiate(ExplosionEffect.ParticleSystem, new Vector3(pos.x, pos.y, pos.z), Quaternion.identity);
            }
            );
        }
    }
}
