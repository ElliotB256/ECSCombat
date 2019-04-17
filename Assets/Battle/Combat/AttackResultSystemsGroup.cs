using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// A group for systems which respond to attacks already made.
    /// </summary>
    [UpdateAfter(typeof(AttackSystemsGroup))]
    public class AttackResultSystemsGroup : ComponentSystemGroup
    {
    }
}
