using System;
using Unity.Entities;

namespace Battle.Equipment
{
    [Serializable]
    public struct ShieldEquipment : IComponentData
    {
        public float HealthFractionBonus;
    }
}