using System;
using Unity.Entities;

namespace Battle.Equipment
{
    /// <summary>
    /// Marks an entity as engine equipment.
    /// </summary>
    [Serializable]
    public struct Engine : IComponentData, IAggregateEquipment
    {
        /// <summary>
        /// Amount of thrust produced by this engine
        /// </summary>
        public float Thrust;
    }
}