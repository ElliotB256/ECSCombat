using Battle.Combat;
using Unity.Entities;

namespace Battle.Movement
{
    /// <summary>
    /// A group for systems that update the position of entities.
    /// </summary>
    [UpdateAfter(typeof(PostAttackEntityBuffer))]
    public class MovementUpdateSystemsGroup : ComponentSystemGroup
    {
    }
}
