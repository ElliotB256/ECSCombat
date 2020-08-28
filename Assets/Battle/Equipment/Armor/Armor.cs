using System;
using Unity.Entities;

namespace Battle.Equipment
{
    [Serializable]
    public struct Armor : IComponentData
    {
        public float HealthFractionBonus;
    }
}