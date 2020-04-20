using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Battle.Combat
{
    /// <summary>
    /// Deletes all entities with Destroy component
    /// </summary>
    [
        UpdateInGroup(typeof(LateSimulationSystemGroup))
        ]
    public class DeleteEntitiesSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem BufferSystem;
        private EntityQuery DeleteEntities;

        protected override void OnCreate()
        {
            BufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            DeleteEntities = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Delete>());
        }

        protected override void OnUpdate()
        {
            var buff = BufferSystem.CreateCommandBuffer();
            buff.DestroyEntity(DeleteEntities);
            BufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}