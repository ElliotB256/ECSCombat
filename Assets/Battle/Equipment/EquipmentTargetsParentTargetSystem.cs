using Battle.Combat;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.Equipment
{
    /// <summary>
    /// Sets the target of all equipment to the target of the parent entity. Eg for weapons, etc.
    /// </summary>
    [
        UpdateInGroup(typeof(EquipmentUpdateGroup))
    ]
    public class EquipmentTargetsParentTargetSystem : SystemBase
    {
        public EntityQuery Query;

        protected override void OnUpdate()
        {
            var targets = GetComponentDataFromEntity<Target>();
            Entities
                .WithAll<Equipment, Target>()
                .ForEach(
                (
                    Entity e,
                    in Parent parent
                ) => {

                    if (!targets.HasComponent(e) || !targets.HasComponent(parent.Value))
                        return;

                    targets[e] = targets[parent.Value];
                }
                )
                .Schedule();
        }
    }
}
