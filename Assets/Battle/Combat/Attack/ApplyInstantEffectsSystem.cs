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
    public class ApplyInstantEffectsSystem : JobComponentSystem
    {
        protected WeaponEntityBufferSystem m_entityBufferSystem;

        [BurstCompile]
        struct ApplyInstantEffectsJob : IJobForEachWithEntity<Target, LocalToWorld, TargettedTool, InstantEffect>
        {
            public EntityCommandBuffer.Concurrent buffer;
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Transforms;

            public void Execute(
                Entity attacker,
                int index,
                [ReadOnly] ref Target target,
                [ReadOnly] ref LocalToWorld worldTransform,
                [ReadOnly] ref TargettedTool tool,
                [ReadOnly] ref InstantEffect effect
                )
            {
                if (!tool.Firing)
                    return;

                if (target.Value == Entity.Null)
                    return;

                // Create the effect
                Entity attack = buffer.Instantiate(index, effect.AttackTemplate);
                buffer.AddComponent(index, attack, Attack.New(effect.Accuracy));
                buffer.AddComponent(index, attack, target);
                buffer.AddComponent(index, attack, new Instigator() { Value = attacker });
                buffer.AddComponent(index, attack, new EffectSourceLocation { Value = worldTransform.Position });
                buffer.AddComponent(index, attack, new Effectiveness { Value = 1f });
                buffer.AddComponent(index, attack, new SourceLocation { Position = worldTransform.Position });
                if (Transforms.HasComponent(target.Value))
                    buffer.AddComponent(index, attack, new HitLocation { Position = Transforms[target.Value].Position });
            }
        }

        protected override void OnCreateManager()
        {
            m_entityBufferSystem = World.GetOrCreateSystem<WeaponEntityBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var applyEffectsJH = new ApplyInstantEffectsJob()
            {
                buffer = m_entityBufferSystem.CreateCommandBuffer().ToConcurrent(),
                Transforms = GetComponentDataFromEntity<LocalToWorld>(true)
            }.Schedule(this, inputDependencies);
            m_entityBufferSystem.AddJobHandleForProducer(applyEffectsJH);
            return applyEffectsJH;
        }
    }
}