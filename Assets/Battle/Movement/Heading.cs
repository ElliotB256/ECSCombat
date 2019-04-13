using System;
using Unity.Entities;

namespace Battle.Movement
{
    /// <summary>
    /// Direction/facing of an entity.
    /// This is a readonly component, used as a world-space representation of an object's rotation.
    /// Do not modify it.
    /// </summary>
    [Serializable]
    public struct Heading : IComponentData
    {
        public float Value;
    }
}