using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Battle.Movement;

namespace Battle.Equipment
{
    /// <summary>
    /// Modifies a Parent's maximum speed as Engines are added/removed.
    /// </summary>
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(EquipmentUpdateGroup))]
    public class EngineSystem : AggregateEquipmentSystem<Engine>
    {
        protected override AggregationScenario Scenario => AggregationScenario.OnEnableAndDisable;
    }

    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(EquipmentUpdateGroup))]
    [UpdateAfter(typeof(EngineSystem))]
    public class CalculateSpeedSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .ForEach(
                (ref Speed speed, in Engine engine) => speed.Value = engine.Thrust
                )
                .Schedule();
        }
    }
}