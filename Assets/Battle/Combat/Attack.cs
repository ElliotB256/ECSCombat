using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// An attack made by one entity against another.
    /// </summary>
    [Serializable]
    public struct Attack : IComponentData
    {
        byte dummy;
    }

    // Ideas - ShieldSystem would just redirect an attack intended for the owning entity into the shield entity.
}