using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// A group for systems which modify the state of attacks.
    /// </summary>
    [UpdateAfter(typeof(WeaponEntityBufferSystem))]
    public class AttackSystemsGroup : ComponentSystemGroup
    {
    }
}
