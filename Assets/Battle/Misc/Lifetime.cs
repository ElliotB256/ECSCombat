using System;
using Unity.Entities;
using Unity.Rendering;

namespace Battle.Combat
{
    /// <summary>
    /// Represents an entity with a limited lifetime.
    /// The entity is marked for deletion when the lifetime expires.
    /// </summary>
    [Serializable]
    [MaterialProperty("_Lifetime", MaterialPropertyFormat.Float)]
    public struct Lifetime : IComponentData
    {
        public float Value;
    }
}