using Unity.Entities;

namespace Battle.Spawner
{
    [UpdateInGroup(typeof(SpawnSystemGroup))]
    public class SpawnSystemEntityBuffer : EntityCommandBufferSystem
    {
    }
}
