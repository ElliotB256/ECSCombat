using Unity.Entities;

namespace Battle.Equipment
{
    /// <summary>
    /// Removes the Equipping/Dequipping flag components
    /// </summary>
    [
        UpdateAfter(typeof(EquipmentUpdateGroup)),
        UpdateBefore(typeof(EquipmentBufferSystem))
        ]
    public class RemoveEquippingDequippingSystem : SystemBase
    {
        private EntityQuery EquippingQuery;
        private EntityQuery DequippingQuery;

        protected override void OnCreate()
        {
            EquippingQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Equipping>());
            DequippingQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Dequipping>());
        }

        protected override void OnUpdate()
        {
            EntityManager.RemoveComponent<Equipping>(EquippingQuery);
            EntityManager.RemoveComponent<Dequipping>(DequippingQuery);
        }
    }
}