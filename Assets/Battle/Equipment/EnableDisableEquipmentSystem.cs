using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.Equipment
{
    /// <summary>
    /// Enable/Disable equipment
    /// </summary>
    [
        UpdateAfter(typeof(EquipmentUpdateGroup)),
        UpdateBefore(typeof(EquipmentBufferSystem))
        ]
    public class EnableDisableEquipmentSystem : JobComponentSystem
    {
        protected EquipmentBufferSystem EquipmentBuffer;
        protected EntityQuery EnablingQuery;
        protected EntityQuery DisablingQuery;

        protected override void OnCreate()
        {
            EquipmentBuffer = World.GetOrCreateSystem<EquipmentBufferSystem>();
            EnablingQuery = GetEntityQuery(new EntityQueryDesc { 
                All = new[] {
                    ComponentType.ReadOnly<Enabling>()
                }});
            DisablingQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<Disabling>()
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var buffer = EquipmentBuffer.CreateCommandBuffer();
            buffer.RemoveComponent<Enabled>(DisablingQuery);
            buffer.RemoveComponent<Disabling>(DisablingQuery);
            buffer.AddComponent<Enabled>(EnablingQuery);
            buffer.RemoveComponent<Enabling>(EnablingQuery);

            return inputDependencies;
        }
    }
}