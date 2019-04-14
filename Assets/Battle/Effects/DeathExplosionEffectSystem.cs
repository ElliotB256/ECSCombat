using Battle.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Battle.Effects
{
    [UpdateAfter(typeof(DealAttackDamageSystem)),UpdateBefore(typeof(KillEntitiesWithNoHealthSystem))]
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
