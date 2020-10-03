using Unity.Entities;
using Unity.Jobs;

namespace Battle.Equipment
{
    /// <summary>
    /// Enable/Disable equipment
    /// </summary>
    [
        UpdateAfter(typeof(EquipmentUpdateGroup)),
        UpdateBefore(typeof(EquipmentBufferSystem))
        ]
    public class EnableDisableEquipmentSystem : SystemBase
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

        protected override void OnUpdate()
        {
            var buffer = EquipmentBuffer.CreateCommandBuffer();
            buffer.RemoveComponent<Enabled>(DisablingQuery);
            buffer.RemoveComponent<Disabling>(DisablingQuery);
            buffer.AddComponent<Enabled>(EnablingQuery);
            buffer.RemoveComponent<Enabling>(EnablingQuery);
            EquipmentBuffer.AddJobHandleForProducer(Dependency);
        }
    }
}