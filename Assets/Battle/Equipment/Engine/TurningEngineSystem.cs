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
    public class TurningEngineSystem : AggregateEquipmentSystem<MaxTurnSpeed, TurningEngine, TurningEngineAggregator>
    {
        protected override AggregationScenario Scenario => AggregationScenario.OnEnableAndDisable;
    }

    public struct TurningEngineAggregator : IAggregator<MaxTurnSpeed, TurningEngine>
    {
        public MaxTurnSpeed Combine(MaxTurnSpeed original, TurningEngine component)
        {
            original.RadiansPerSecond += component.TurnSpeedRadians;
            return original;
        }

        public MaxTurnSpeed Remove(MaxTurnSpeed original, TurningEngine component)
        {
            original.RadiansPerSecond -= component.TurnSpeedRadians;
            return original;
        }
    }
}