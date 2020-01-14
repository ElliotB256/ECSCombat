using Battle.Effects;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Battle.Combat
{
    /// <summary>
    /// A shield will block an attack if:
    ///  1. It has non-zero health
    ///  2. The attack doesn't come from within the shield radius
    ///  
    /// If an attack is blocked:
    ///  1. The shield tries to absorb as much damage as it can from the attack.
    ///  
    /// This is done in three phases.
    ///  1. A `CollectShieldHitsJob` runs. This enumerates through the attacks, find
    ///     which ones may be blocked, and stores them in a HashMap keyed by the entity target.
    ///  2. The `ShieldAbsorbDamageJob`. For each Shield entity, enumerates through the hashmap
    ///     entries and mitigates damage until the shield is depleted. The modified attack damages
    ///     are collected in a hashmap indexed by the attack entity.
    ///  3. The `ModifyShieldedAttacksJob`. This updates the damage of each attack using the modified
    ///     values after shield absorption. Shielded attacks generate aesthetic entities to illustrate
    ///     that the hit has occured.
    ///     
    /// This pattern allows all loops to be performed async.
    /// </summary>
    [
        UpdateInGroup(typeof(AttackResultSystemsGroup)),
        UpdateBefore(typeof(DealAttackDamageSystem))
        ]
    public class ShieldsAbsorbDamageSystem : JobComponentSystem
    {
        EntityQuery Attacks;
        PostAttackEntityBuffer Buffer;

        protected override void OnCreateManager()
        {
            Attacks = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<Attack>(),
                    ComponentType.ReadOnly<Damage>(),
                    ComponentType.ReadOnly<Target>(),
                    ComponentType.ReadOnly<Instigator>(),
                    ComponentType.ReadOnly<SourceLocation>(),
                    ComponentType.ReadWrite<HitLocation>(),
                },
                None = new [] {
                    ComponentType.ReadOnly<ShieldBypass>()
                }
            });
            Buffer = World.GetOrCreateSystem<PostAttackEntityBuffer>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            int numberOfAttacks = Attacks.CalculateEntityCount();
            var ShieldHits = new NativeMultiHashMap<Entity, ShieldHit>(numberOfAttacks, Allocator.TempJob);
            var CollectShieldHits = new CollectShieldHitsJob
            {
                WorldTransforms = GetComponentDataFromEntity<LocalToWorld>(true),
                Shields = GetComponentDataFromEntity<Shield>(true),
                ShieldHits = ShieldHits.AsParallelWriter()
            }.Schedule(Attacks, inputDependencies);

            var Results = new NativeHashMap<Entity, ShieldHitResult>(numberOfAttacks, Allocator.TempJob);
            var ShieldAbsorbDamage = new ShieldAbsorbDamageJob
            {
                Results = Results.AsParallelWriter(),
                ShieldHits = ShieldHits,
                Buffer = Buffer.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, CollectShieldHits);
            Buffer.AddJobHandleForProducer(ShieldAbsorbDamage);

            var ModifyShieldedAttacks = new ModifyShieldedAttacksJob
            {
                Results = Results
            }.Schedule(Attacks, ShieldAbsorbDamage);

            ShieldHits.Dispose(ModifyShieldedAttacks);
            Results.Dispose(ModifyShieldedAttacks);

            return ModifyShieldedAttacks;
        }
        
        [BurstCompile]
        struct CollectShieldHitsJob : IJobForEachWithEntity<Attack, Damage, SourceLocation, Target>
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> WorldTransforms;
            [ReadOnly] public ComponentDataFromEntity<Shield> Shields;
            public NativeMultiHashMap<Entity, ShieldHit>.ParallelWriter ShieldHits;

            public void Execute(
                Entity entity,
                int index,
                ref Attack attack,
                ref Damage damage,
                ref SourceLocation sourceLoc,
                ref Target target)
            {
                if (!Shields.HasComponent(target.Value) || !WorldTransforms.HasComponent(target.Value))
                    return;
                var shield = Shields[target.Value];

                // depleted shields do not block attacks.
                if (shield.Health <= 0f)
                    return;

                var targetPosition = WorldTransforms[target.Value];

                // attack comes from within shield - no shield protection.
                var delta = targetPosition.Position - sourceLoc.Position;
                if (math.lengthsq(delta) < math.pow(shield.Radius, 2.0f))
                    return;

                ShieldHit hit = new ShieldHit
                {
                    Attack = entity,
                    Damage = damage,
                    FromDirection = -math.normalize(delta)
                };
                ShieldHits.Add(target.Value, hit);
            }
        }

        [BurstCompile]
        struct ShieldAbsorbDamageJob : IJobForEachWithEntity<Shield, LocalToWorld>
        {
            [ReadOnly] public NativeMultiHashMap<Entity, ShieldHit> ShieldHits;
            [WriteOnly] public NativeHashMap<Entity, ShieldHitResult>.ParallelWriter Results;
            public EntityCommandBuffer.Concurrent Buffer;

            public void Execute(Entity e, int i, ref Shield shield, ref LocalToWorld localToWorld)
            {
                if (!ShieldHits.TryGetFirstValue(e, out var shieldHit, out var iter))
                    return;
                do
                {
                    if (shield.Health < 0f)
                        continue;
                    float absorbed = math.min(shield.Health, shieldHit.Damage.Value);
                    shield.Health = shield.Health - absorbed;
                    Damage damage = shieldHit.Damage;
                    damage.Value = damage.Value - absorbed;

                    bool blocked = absorbed > 0f;

                    //generate aesthetic effect.
                    if (blocked)
                    {
                        var effect = Buffer.CreateEntity(i);
                        Buffer.AddComponent(i, effect, new ShieldHitEffect { HitDirection = shieldHit.FromDirection });
                        Buffer.AddComponent(i, effect, shield);
                        Buffer.AddComponent(i, effect, localToWorld);
                        Buffer.AddComponent(i, effect, new Delete());
                    }

                    ShieldHitResult result = new ShieldHitResult
                    {
                        Damage = damage,
                        Blocked = blocked,
                        HitLocation = new HitLocation { Position = localToWorld.Position + shield.Radius * shieldHit.FromDirection }
                    };
                    Results.TryAdd(shieldHit.Attack, result);
                } while (ShieldHits.TryGetNextValue(out shieldHit, ref iter));
            }
        }

        [BurstCompile]
        struct ModifyShieldedAttacksJob : IJobForEachWithEntity<Damage, HitLocation>
        {
            [ReadOnly] public NativeHashMap<Entity, ShieldHitResult> Results;

            public void Execute(Entity e, int index, ref Damage damage, ref HitLocation hitLoc)
            {
                if (!Results.ContainsKey(e))
                    return;
                if (!Results[e].Blocked)
                    return;
                damage = Results[e].Damage;
                hitLoc = Results[e].HitLocation;
            }
        }

        struct ShieldHit
        {
            public Entity Attack;
            public Damage Damage;
            public float3 FromDirection;
        }

        struct ShieldHitResult
        {
            public Damage Damage;
            public HitLocation HitLocation;
            public bool Blocked;
        }
    }
}
