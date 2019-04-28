using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.Equipment
{
    /// <summary>
    /// Modifies attributes of a parent component as equipment is enabled/disabled
    /// </summary>
    [AlwaysUpdateSystem]
    public abstract class AggregateEquipmentSystem<TEquipment> : JobComponentSystem
        where TEquipment : struct, IComponentData
    {
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

        /// <summary>
        /// Map that indexes the 'to enable' components by the entitiy that they affect.
        /// </summary>
        protected NativeMultiHashMap<Entity, TEquipment> AddedEquipment;

        /// <summary>
        /// Map that indexes the 'to remove' components by the entity that they affect.
        /// </summary>
        protected NativeMultiHashMap<Entity, TEquipment> RemovedEquipment;

        /// <summary>
        /// Boolean that stores if system has run previously.
        /// </summary>
        bool hasRunBefore = false;

        protected override void OnCreate()
        {
            ComponentsToBeEnabled = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<TEquipment>(), ComponentType.ReadOnly<Parent>(), ComponentType.ReadOnly<Enabling>() },
            });

            ComponentsToBeDisabled = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<TEquipment>(), ComponentType.ReadOnly<Parent>(), ComponentType.ReadOnly<Disabling>() },
            });

            EquipmentBuffer = World.GetOrCreateSystem<EquipmentBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var removedCount = ComponentsToBeDisabled.CalculateLength();
            var addedCount = ComponentsToBeEnabled.CalculateLength();
            if (hasRunBefore)
            {
                RemovedEquipment.Dispose();
                AddedEquipment.Dispose();
            }

            AddedEquipment = new NativeMultiHashMap<Entity, TEquipment>(addedCount, Allocator.TempJob);
            RemovedEquipment = new NativeMultiHashMap<Entity, TEquipment>(removedCount, Allocator.TempJob);
            hasRunBefore = true;

            var sortAddedJH = new AddToMap
            {
                EquipmentMap = AddedEquipment.ToConcurrent(),
            }.Schedule(ComponentsToBeEnabled, inputDependencies);
            var sortRemovedJH = new AddToMap
            {
                EquipmentMap = RemovedEquipment.ToConcurrent(),
            }.Schedule(ComponentsToBeDisabled, inputDependencies);
            var combinedJH = JobHandle.CombineDependencies(sortAddedJH, sortRemovedJH);

            var updateJH = CreateProcessJobHandle(combinedJH);
            
            return updateJH;
        }

        protected override void OnStopRunning()
        {
            if (hasRunBefore)
            {
                RemovedEquipment.Dispose();
                AddedEquipment.Dispose();
            }
        }

        /// <summary>
        /// Maps TEquipment components to the parent of their entities.
        /// </summary>
        [BurstCompile]
        struct AddToMap : IJobForEachWithEntity<TEquipment, Parent>
        {
            public NativeMultiHashMap<Entity, TEquipment>.Concurrent EquipmentMap;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref TEquipment engine,
                [ReadOnly] ref Parent parent
                )
            {
                EquipmentMap.Add(parent.Value, engine);
            }
        }

        protected abstract JobHandle CreateProcessJobHandle(JobHandle inputDependencies);
    }
}