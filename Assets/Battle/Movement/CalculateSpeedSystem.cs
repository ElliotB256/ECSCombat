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
        protected override void OnUpdate()
        {
            Entities
                .ForEach(
                (ref MaxSpeed maxSpeed, in Thrust thrust, in Mass mass) =>
                {
                    maxSpeed.Value = thrust.Forward / mass.Value;
                }
                ).ScheduleParallel();

            Entities
                .ForEach(
                (ref MaxTurnSpeed turn, in Thrust thrust, in Mass mass) =>
                {
                    turn.RadiansPerSecond = thrust.Turning / mass.Value;
                }
                ).ScheduleParallel();

            // gotta go fast
            Entities
                .ForEach((ref Speed speed, in MaxSpeed maxSpeed) => speed.Value = maxSpeed.Value)
                .ScheduleParallel();
        }
    }
}