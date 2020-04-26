using Unity.Entities;
using Battle.Movement;

namespace Battle.Equipment
{
    /// <summary>
    /// Modifies a Parent's maximum speed as Engines are added/removed.
    /// </summary>
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(EquipmentUpdateGroup))]
    public class TurningEngineSystem : AggregateEquipmentSystem<TurningEngine>
    {
        protected override AggregationScenario Scenario => AggregationScenario.OnEnableAndDisable;
    }

    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(EquipmentUpdateGroup))]
    [UpdateAfter(typeof(TurningEngineSystem))]
    public class CalculateTurningSpeedSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .ForEach(
                (ref MaxTurnSpeed speed, in TurningEngine engine) => speed.RadiansPerSecond = engine.TurnSpeedRadians
                )
                .Schedule();
        }
    }
}