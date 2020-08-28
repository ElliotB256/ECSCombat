using System;
using Unity.Entities;

namespace Battle.Combat
{
    [Serializable]
    public struct Heal : IComponentData
    {
        public float Value;
    }
}