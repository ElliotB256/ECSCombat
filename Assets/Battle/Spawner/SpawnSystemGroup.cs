using Battle.Movement;
using Unity.Entities;
using Unity.Transforms;

namespace Battle.Spawner
{
    [UpdateAfter(typeof(MovementUpdateSystemsGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public class SpawnSystemGroup : ComponentSystemGroup
    {
    }
}
