using Battle.Combat;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Battle.Movement
{
    /// <summary>
    /// Updates the rotation of all entities with TurnSpeed
    /// </summary>
    [UpdateInGroup(typeof(MovementUpdateSystemsGroup))]
    public class UpdateRotationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dT = GetSingleton<GameTimeDelta>().dT;
            Entities
                .ForEach(
                (ref Rotation rot, in TurnSpeed turnSpeed) =>
                {
                    rot.Value = math.mul(math.normalize(rot.Value), quaternion.AxisAngle(math.up(), turnSpeed.RadiansPerSecond * dT));
                }
                )
                .ScheduleParallel();
        }
    }
}