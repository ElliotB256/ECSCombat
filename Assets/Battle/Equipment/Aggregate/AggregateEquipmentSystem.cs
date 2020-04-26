using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.Equipment
{
    /// <summary>
    /// Adds/subtracts values from equipped/dequipped items to their parent.
    /// </summary>
    public abstract class AggregateEquipmentSystem<TEquipment> : SystemBase
        where TEquipment : struct, IComponentData, ICombineable<TEquipment>
    {
        protected enum AggregationScenario
        {
            OnEquipAndDequip,
            OnEnableAndDisable,
        }

        /// <summary>
        /// Scenario system uses for adding/removing components.
        /// </summary>
        /// <returns></returns>
        protected abstract AggregationScenario Scenario { get; }

        /// <summary>
        /// Query that selects entities/components to be enabled.
        /// </summary>
        protected EntityQuery ComponentsToBeEnabled;

        /// <summary>
        /// Query that selects entities/components to be disabled.
        /// </summary>
        protected EntityQuery ComponentsToBeDisabled;

        /// <summary>
        /// Buffer that enacts changes due to the aggregate equipment system.
        /// </summary>
        protected EquipmentBufferSystem EquipmentBuffer;

        protected override void OnCreate()
        {
            ComponentsToBeEnabled = EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<Enabling>(),
                ComponentType.ReadOnly<TEquipment>(),
                ComponentType.ReadOnly<Parent>()
                );

            ComponentsToBeDisabled = EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<Disabling>(),
                ComponentType.ReadOnly<TEquipment>(),
                ComponentType.ReadOnly<Parent>()
                );

            EquipmentBuffer = World.GetOrCreateSystem<EquipmentBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var addedCount = ComponentsToBeEnabled.CalculateEntityCount();
            var removedCount = ComponentsToBeDisabled.CalculateEntityCount();

            var addedEquipment = new NativeMultiHashMap<Entity, TEquipment>(addedCount, Allocator.TempJob);
            var removedEquipment = new NativeMultiHashMap<Entity, TEquipment>(removedCount, Allocator.TempJob);

            var addMapJobHandle = new AddToMapJob
            {
                Equipment = GetArchetypeChunkComponentType<TEquipment>(true),
                Parent = GetArchetypeChunkComponentType<Parent>(true),
                EquipmentMap = addedEquipment.AsParallelWriter()
            }.Schedule(ComponentsToBeEnabled, Dependency);

            var removeMapJobHandle = new AddToMapJob
            {
                Equipment = GetArchetypeChunkComponentType<TEquipment>(true),
                Parent = GetArchetypeChunkComponentType<Parent>(true),
                EquipmentMap = removedEquipment.AsParallelWriter()
            }.Schedule(ComponentsToBeDisabled, Dependency);

            var barrier = JobHandle.CombineDependencies(addMapJobHandle, removeMapJobHandle);

            //var updateJH = new AggregateJob()
            //{
            //    AddedEquipmentMap = addedEquipment,
            //    RemovedEquipmentMap = removedEquipment,
            //    ParentComponents = GetComponentDataFromEntity<TEquipment>(false)
            //}.Schedule(barrier);
            var updateJH = barrier;

            Dependency = updateJH;

            addedEquipment.Dispose(updateJH);
            removedEquipment.Dispose(updateJH);
        }

        [BurstCompile]
        struct AddToMapJob : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkComponentType<TEquipment> Equipment;
            [ReadOnly] public ArchetypeChunkComponentType<Parent> Parent;
            public NativeMultiHashMap<Entity, TEquipment>.ParallelWriter EquipmentMap;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var equipments = chunk.GetNativeArray(Equipment);
                var parent = chunk.GetNativeArray(Parent);

                for (int i = 0; i < chunk.Count; i++)
                {
                    EquipmentMap.Add(parent[i].Value, equipments[i]);
                }
            }
        }

        ////[BurstCompile]
        //struct AggregateJob : IJob
        //{
        //    public ComponentDataFromEntity<TEquipment> ParentComponents;
        //    [ReadOnly] public NativeMultiHashMap<Entity, TEquipment> AddedEquipmentMap;
        //    [ReadOnly] public NativeMultiHashMap<Entity, TEquipment> RemovedEquipmentMap;

        //    public void Execute()
        //    {
        //        var parents = AddedEquipmentMap.GetKeyArray(Allocator.Temp);
        //        var addedEquipments = AddedEquipmentMap.GetValueArray(Allocator.Temp);

        //        for (int i = 0; i < parents.Length; i++)
        //        {
        //            var parent = parents[i];
        //            if (!ParentComponents.Exists(parent))
        //                continue;
        //            var parentComponent = ParentComponents[parent];
        //            parentComponent.Combine(addedEquipments[i]);
        //            ParentComponents[parent] = parentComponent;
        //        }
        //        addedEquipments.Dispose();
        //        parents.Dispose();

        //        parents = RemovedEquipmentMap.GetKeyArray(Allocator.Temp);
        //        var removedEquipments = RemovedEquipmentMap.GetValueArray(Allocator.Temp);
        //        for (int i = 0; i < parents.Length; i++)
        //        {
        //            var parent = parents[i];
        //            if (!ParentComponents.Exists(parent))
        //                continue;
        //            var parentComponent = ParentComponents[parent];
        //            parentComponent.Decombine(removedEquipments[i]);
        //            ParentComponents[parent] = parentComponent;
        //        }
        //        removedEquipments.Dispose();
        //        parents.Dispose();
        //    }
        //}
    }

    public interface ICombineable<T> { void Combine(T other); void Decombine(T other); }
}