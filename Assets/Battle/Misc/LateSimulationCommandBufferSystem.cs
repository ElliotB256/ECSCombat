using Unity.Entities;

namespace Battle.Combat
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class LateSimulationCommandBufferSystem : EntityCommandBufferSystem
    {
    }
}