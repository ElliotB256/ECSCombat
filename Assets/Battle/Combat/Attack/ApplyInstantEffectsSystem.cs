using Battle.Combat.AttackSources;
using Battle.Combat.Calculations;
using Battle.Equipment;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Battle.Combat.AttackSources
{
    /// <summary>
    /// Applies all instant effects.
    /// </summary>
    [
        UpdateInGroup(typeof(WeaponSystemsGroup)),
        UpdateAfter(typeof(FireTargettedToolsSystem))
        ]
    public class ApplyInstantEffectsSystem : SystemBase
    {
        protected WeaponEntityBufferSystem m_entityBufferSystem;

        protected override void OnCreate()
        {
            m_entityBufferSystem = World.GetOrCreateSystem<WeaponEntityBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var buffer = m_entityBufferSystem.CreateCommandBuffer().ToConcurrent();
            var transforms = GetComponentDataFromEntity<LocalToWorld>(true);

            Entities.ForEach(
                (
                    Entity attacker,
                    int entityInQueryIndex,
                    in Target target,
                    in LocalToWorld localToWorld,
                    in TargettedTool tool,
                    in InstantEffect effect
                ) =>
                {
                    if (!tool.Firing)
                        return;

                    if (target.Value == Entity.Null)
                        return;

                    // Create the effect
                    Entity attack = buffer.Instantiate(entityInQueryIndex, effect.AttackTemplate);
                    buffer.AddComponent(entityInQueryIndex, attack, Attack.New(effect.Accuracy));
                    buffer.AddComponent(entityInQueryIndex, attack, target);
                    buffer.AddComponent(entityInQueryIndex, attack, new Instigator() { Value = attacker });
                    buffer.AddComponent(entityInQueryIndex, attack, new EffectSourceLocation { Value = localToWorld.Position });
                    buffer.AddComponent(entityInQueryIndex, attack, new Effectiveness { Value = 1f });
                    buffer.AddComponent(entityInQueryIndex, attack, new SourceLocation { Position = localToWorld.Position });
                    if (transforms.HasComponent(target.Value))
                        buffer.AddComponent(entityInQueryIndex, attack, new HitLocation { Position = transforms[target.Value].Position });
                }
                )
                .WithReadOnly(transforms)
                .Schedule();

            m_entityBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}