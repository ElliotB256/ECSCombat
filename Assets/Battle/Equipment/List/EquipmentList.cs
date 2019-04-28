using Unity.Entities;

namespace Battle.Equipment
{
    /// <summary>
    /// A list of all equipment attached to an entity.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct EquipmentList : IBufferElementData
    {
        public static implicit operator Entity(EquipmentList e) { return e.Equipment; }
        public static implicit operator EquipmentList(Entity e) { return new EquipmentList { Equipment = e }; }

        public Entity Equipment;
    }
}
