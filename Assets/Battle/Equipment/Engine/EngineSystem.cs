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
    [UpdateInGroup(typeof(EquipmentUpdateGroup))]
    public class EngineSystem : AggregateEquipmentSystem<Speed, Engine, EngineAggregator>
    {
        protected override AggregationScenario Scenario => AggregationScenario.OnEnableAndDisable;
    }

    public struct EngineAggregator : IAggregator<Speed, Engine>
    {
        public Speed Combine(Speed original, Engine component)
        {
            original.Value += component.Thrust;
            return original;
        }

        public Speed Remove(Speed original, Engine component)
        {
            original.Value -= component.Thrust;
            return original;
        }
    }
}