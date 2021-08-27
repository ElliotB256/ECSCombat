using Battle.Movement;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;

namespace Battle.Equipment
{
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(EquipmentUpdateGroup))]
    public class EngineSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var thrustData = GetComponentDataFromEntity<Thrust>( isReadOnly:false );

            Entities
                .WithAll<Enabling>()
                .ForEach( ( in Engine engine , in Parent parent ) =>
                {
                    Thrust thrust = thrustData[parent.Value];
                    thrust.Forward += engine.ForwardThrust;
                    thrust.Turning += engine.TurningThrust;
                    thrustData[parent.Value] = thrust;
                } )
                .WithBurst()
                .Schedule();

            Entities
                .WithAll<Disabling>()
                .ForEach( ( in Engine engine , in Parent parent ) =>
                {
                    Thrust thrust = thrustData[parent.Value];
                    thrust.Forward -= engine.ForwardThrust;
                    thrust.Turning -= engine.TurningThrust;
                    thrustData[parent.Value] = thrust;
                } )
                .WithBurst()
                .Schedule();
        }
    }
}