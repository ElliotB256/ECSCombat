using Battle.Combat.AttackSources;
using Battle.Combat.Calculations;
using Battle.Equipment;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Battle.Combat.AttackSources
{
    /// <summary>
    /// Fires all direct weapons that are armed and in range of their target.
    /// </summary>
    [
        UpdateInGroup(typeof(WeaponSystemsGroup))
        ]
    public class FireTargettedToolsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var worldTransforms = GetComponentDataFromEntity<LocalToWorld>(true);

            Entities
                .ForEach((ref TargettedTool tool) => tool.Firing = false)
                .Schedule();

            Entities
                .WithAll<Enabled>()
                .WithStoreEntityQueryInField(ref m_query)
                .WithReadOnly(worldTransforms)
                .ForEach(
                    (Entity attacker,
                    int entityInQueryIndex,
                    ref Cooldown cooldown,
                    ref TargettedTool tool,
                    in Target target,
                    in LocalToWorld worldTransform) =>
                {
                    if (target.Value == Entity.Null)
                        return;

                    if (!tool.Armed)
                        return;

                    if (!cooldown.IsReady())
                        return;

                    var delta = worldTransforms[target.Value].Position - worldTransform.Position;

                    // Cannot fire if out of weapon range
                    if (math.lengthsq(delta) > tool.Range * tool.Range)
                        return;

                    // Only fire when target is within weapon cone.
                    var projection = math.dot(math.normalize(delta), math.normalize(worldTransform.Forward));
                    if (math.cos(tool.Cone / 2f) > projection)
                        return;

                    tool.Firing = true;

                    // Reset the cooldown
                    cooldown.Timer = cooldown.Duration;
                }
                )
                .Schedule();
        }

        protected EntityQuery m_query;
    }
}