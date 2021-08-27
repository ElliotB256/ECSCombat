using Battle.Equipment;
using Unity.Entities;

namespace Battle.Movement
{
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(MovementUpdateSystemsGroup))]
    [UpdateBefore(typeof(UpdateTranslationSystem))]
    [UpdateBefore(typeof(UpdateRotationSystem))]
    public class CalculateSpeedSystem : SystemBase
    {
        protected override void OnUpdate ()
        {
            Entities
                .WithChangeFilter<Thrust,Mass>()
                .ForEach( ( ref MaxSpeed maxSpeed , ref MaxTurnSpeed turn , in Thrust thrust , in Mass mass ) =>
                {
                    maxSpeed.Value = thrust.Forward / mass.Value;
                    turn.RadiansPerSecond = thrust.Turning / mass.Value;
                } )
                .WithBurst()
                .ScheduleParallel();

            // gotta go fast
            Entities
                .ForEach( ( ref Speed speed , in MaxSpeed maxSpeed ) => speed.Value = maxSpeed.Value )
                .WithBurst()
                .ScheduleParallel();
        }
    }
}