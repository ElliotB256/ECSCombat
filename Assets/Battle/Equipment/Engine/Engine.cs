using System;
using Unity.Entities;

namespace Battle.Equipment
{
    /// <summary>
    /// Marks an entity as engine equipment.
    /// </summary>
    [Serializable]
    public struct Engine : IComponentData, ICombineable<Engine>
    {
        /// <summary>
        /// Amount of thrust produced by this engine
        /// </summary>
        public float Thrust;

        public void Combine(Engine other)
        {
            Thrust += other.Thrust;
        }

        public void Decombine(Engine other)
        {
            Thrust -= other.Thrust;
        }
    }
}