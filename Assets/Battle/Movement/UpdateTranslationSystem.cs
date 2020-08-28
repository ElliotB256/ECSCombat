using Battle.Combat;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Battle.Movement
{
    /// <summary>
    /// Updates the Translation of all entities with speed.
    /// </summary>
    [UpdateInGroup(typeof(MovementUpdateSystemsGroup))]
    public class UpdateTranslationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dT = GetSingleton<GameTimeDelta>().dT;

            Entities.ForEach(
                (ref Translation translation, in Rotation rot, in Speed speed) =>
                {
                    float3 displacement = dT * speed.Value * math.forward(rot.Value);
                    translation.Value = translation.Value + displacement;
                }
                )
                .Schedule();
        }
    }
}