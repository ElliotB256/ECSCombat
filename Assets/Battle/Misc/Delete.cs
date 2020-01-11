using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// Marks an entity to be deleted at the end of the simulation frame.
    /// </summary>
    [Serializable]
    public struct Delete : IComponentData
    {
    }
}