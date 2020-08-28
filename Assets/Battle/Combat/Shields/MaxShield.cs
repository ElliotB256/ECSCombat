using System;
using Unity.Entities;

namespace Battle.Combat
{
    [Serializable]
    public struct MaxShield : IComponentData
    {
        public float Value;
    }
}