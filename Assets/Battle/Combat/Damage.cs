using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// Damage associated with an entity.
    /// For example, for a weapon this may be the damage of the weapon.
    /// For an attack, this is the damage of the attack.
    /// </summary>
    [Serializable]
    public struct Damage : IComponentData
    {
        public float Value;
    }
}