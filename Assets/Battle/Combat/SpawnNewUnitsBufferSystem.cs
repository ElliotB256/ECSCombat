using Unity.Entities;
using Unity.Transforms;

namespace Battle.Combat
{
    /// <summary>
    /// New units are spawned at the end of the frame.
    /// 
    /// This ensures new entities are given a valid LocalToWorld before they are included in any find target or attack searches.
    /// </summary>
    [
        UpdateAfter(typeof(CleanUpAttacksSystem)),
        UpdateBefore(typeof(TransformSystemGroup))
        ]
    public class SpawnNewUnitsBufferSystem : EntityCommandBufferSystem
    {
    }
}
