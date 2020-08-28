using System;
using Unity.Entities;

namespace Battle.Equipment
{
    /// <summary>
    /// Marks an entity as engine equipment.
    /// </summary>
    [Serializable]
    public struct Engine : IComponentData
    {
        public float ForwardThrust;
        public float TurningThrust;
    }
}