using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// Buffer system used after attack results are processed.
    /// </summary>
    [UpdateAfter(typeof(AttackResultSystemsGroup)), UpdateAfter(typeof(DestroyAttacksSystem))]
    public class PostAttackEntityBuffer : EntityCommandBufferSystem
    {
    }
}
