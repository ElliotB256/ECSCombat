using Battle.Effects;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Battle.Combat
{
    [
        UpdateInGroup(typeof(AttackResultSystemsGroup)),
        UpdateBefore(typeof(DealAttackDamageSystem))
        ]
    public class ShieldsAbsorbDamageSystem : SystemBase
    {
        EntityQuery Attacks;
        PostAttackEntityBuffer Buffer;

        protected override void OnCreate()
        {
            Buffer = World.GetOrCreateSystem<PostAttackEntityBuffer>();
        }

        protected override void OnUpdate()
        {
            var buffer = Buffer.CreateCommandBuffer().AsParallelWriter();
            var shields = GetComponentDataFromEntity<Shield>(false);
            var worldTransforms = GetComponentDataFromEntity<LocalToWorld>(true);
            
            // Enumerate over all attacks.
            Entities
                .WithNone<ShieldBypass>()
                .WithAll<Instigator>()
                .WithReadOnly(worldTransforms)
                .WithStoreEntityQueryInField(ref Attacks)
                .ForEach(
                (Entity entity,
                int entityInQueryIndex,
                ref Attack attack,
                ref Damage damage,
                ref SourceLocation sourceLocation,
                ref HitLocation hitLocation,
                ref Target target) =>
            {
                if (!shields.HasComponent(target.Value) || !worldTransforms.HasComponent(target.Value))
                    return;
                var shield = shields[target.Value];

                // depleted shields do not block attacks.
                if (shield.Health <= 0f)
                    return;

                // if attack comes from within shield there is no shield protection.
                var targetPosition = worldTransforms[target.Value];
                var delta = targetPosition.Position - sourceLocation.Position;
                if (math.lengthsq(delta) < math.pow(shield.Radius, 2.0f))
                    return;

                // Shield reduces incoming damage.
                float absorbed = math.min(shield.Health, damage.Value);
                shield.Health = shield.Health - absorbed;
                damage.Value = damage.Value - absorbed;
                hitLocation.Position = targetPosition.Position + shield.Radius * -math.normalize(delta);

                bool blocked = absorbed > 0f;

                shields[target.Value] = shield;

                //generate aesthetic effect.
                if (blocked)
                {
                    var effect = buffer.CreateEntity(entityInQueryIndex);
                    buffer.AddComponent(entityInQueryIndex, effect, new ShieldHitEffect { HitDirection = -math.normalize(delta) });
                    buffer.AddComponent(entityInQueryIndex, effect, shield);
                    buffer.AddComponent(entityInQueryIndex, effect, targetPosition);
                    buffer.AddComponent(entityInQueryIndex, effect, new Delete());
                }
            }).Schedule();

            Buffer.AddJobHandleForProducer(Dependency);
        }
    }
}
