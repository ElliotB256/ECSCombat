using Battle.Movement;
using Unity.Entities;
using Unity.Transforms;

namespace Battle.Equipment
{
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(EquipmentUpdateGroup))]
    public class EngineSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<Enabling>()
                .ForEach(
                (in Engine engine, in Parent parent) =>
                {
                    var thrust = GetComponent<Thrust>(parent.Value);
                    thrust.Forward += engine.ForwardThrust;
                    thrust.Turning += engine.TurningThrust;
                    SetComponent(parent.Value, thrust);
                }
                ).Schedule();

            Entities
                .WithAll<Disabling>()
                .ForEach(
                (in Engine engine, in Parent parent) =>
                {
                    var thrust = GetComponent<Thrust>(parent.Value);
                    thrust.Forward -= engine.ForwardThrust;
                    thrust.Turning -= engine.TurningThrust;
                    SetComponent(parent.Value, thrust);
                }
                ).Schedule();
        }
    }
}