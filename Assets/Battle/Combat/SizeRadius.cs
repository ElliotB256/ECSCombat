using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// Size of an entity expressed as a radius.
    /// </summary>
    [Serializable]
    public struct SizeRadius : IComponentData
    {
        public float Value;
    }
}