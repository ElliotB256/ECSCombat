using System;
using Unity.Entities;

namespace Battle.Equipment
{
    [Serializable]
    public struct EquipmentMass : IComponentData
    {
        /// <summary>
        /// The fractional increase in an entity's Mass by this equipment.
        /// </summary>
        public float MassFractionalIncrease;
    }
}