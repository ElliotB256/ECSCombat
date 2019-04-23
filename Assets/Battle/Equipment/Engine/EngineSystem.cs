using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Battle.Combat;
using Battle.Movement;

namespace Battle.Equipment
{
    /// <summary>
    /// Modifies a Parent's maximum speed as Engines are added/removed.
    /// </summary>
    [AlwaysUpdateSystem]
    public class EngineSystem : JobComponentSystem
    {
        protected EntityQuery m_engineParents;
        protected EntityQuery m_enginesToBeEnabled;
        protected EntityQuery m_enginesToBeDisabled;

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            return inputDependencies;
        }

        protected override void OnCreate()
        {
            // We create queries to select all engines that must be enabled, and all engines that must be disabled.
            m_enginesToBeEnabled = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<Engine>(), ComponentType.ReadOnly<Parent>() },
                None = new[] { ComponentType.ReadWrite<EngineSystemState>() }
            });

            m_enginesToBeDisabled = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<EngineSystemState>(), ComponentType.ReadOnly<Parent>() },
                None = new[] { ComponentType.ReadWrite<Engine>() }
            });

            m_engineParents = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadWrite<MaxSpeed>(), ComponentType.ReadWrite<MaxTurnSpeed>() }
            });
        }
    }
}