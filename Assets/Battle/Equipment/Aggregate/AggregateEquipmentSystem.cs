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
    public abstract class AggregateEquipmentSystem<TParent,TEquipment,TAggregator> : JobComponentSystem
        where TEquipment : struct, IComponentData
        where TParent    : struct, IComponentData
        where TAggregator : struct, IAggregator<TParent, TEquipment>
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
            ComponentType AddCT;
            ComponentType RemoveCT;
            switch (Scenario)
            {
                case AggregationScenario.OnEnableAndDisable:
                    AddCT = ComponentType.ReadOnly<Enabling>();
                    RemoveCT = ComponentType.ReadOnly<Disabling>();
                    break;
                case AggregationScenario.OnEquipAndDequip:
                    AddCT = ComponentType.ReadOnly<Equipping>();
                    RemoveCT = ComponentType.ReadOnly<Dequipping>();
                    break;
                default:
                    throw new System.Exception("Unhandled AggregationBehaviour");
            }

            ComponentsToBeEnabled = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<TEquipment>(), ComponentType.ReadOnly<Parent>(), AddCT },
            });

            ComponentsToBeDisabled = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<TEquipment>(), ComponentType.ReadOnly<Parent>(), RemoveCT },
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

            var updateJH = new AggregateJob()
            {
                Aggregator = GetAggregator(),
                AddedEquipmentMap = AddedEquipment,
                RemovedEquipmentMap = RemovedEquipment,
                ParentComponents = GetComponentDataFromEntity<TParent>(false)
            }.Schedule(combinedJH);
            
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

        public virtual TAggregator GetAggregator()
        {
            return new TAggregator();
        }

        //[BurstCompile]
        struct AggregateJob : IJob
        {
            public TAggregator Aggregator;
            public ComponentDataFromEntity<TParent> ParentComponents;
            [ReadOnly] public NativeMultiHashMap<Entity, TEquipment> AddedEquipmentMap;
            [ReadOnly] public NativeMultiHashMap<Entity, TEquipment> RemovedEquipmentMap;

            public void Execute()
            {
                var parents = AddedEquipmentMap.GetKeyArray(Allocator.Temp);
                var addedEquipments = AddedEquipmentMap.GetValueArray(Allocator.Temp);
                for (int i = 0; i < parents.Length; i++)
                {
                    var parent = parents[i];
                    if (!ParentComponents.Exists(parent))
                        continue;
                    ParentComponents[parent] = Aggregator.Combine(ParentComponents[parent], addedEquipments[i]);
                }
                addedEquipments.Dispose();
                parents.Dispose();

                parents = RemovedEquipmentMap.GetKeyArray(Allocator.Temp);
                var removedEquipments = RemovedEquipmentMap.GetValueArray(Allocator.Temp);
                for (int i = 0; i < parents.Length; i++)
                {
                    var parent = parents[i];
                    if (!ParentComponents.Exists(parent))
                        continue;
                    ParentComponents[parent] = Aggregator.Remove(ParentComponents[parent], removedEquipments[i]);
                }
                removedEquipments.Dispose();
                parents.Dispose();
            }
        }
    }

    /// <summary>
    /// Modifies one component type with another.
    /// </summary>
    public interface IAggregator<TParent,TComp>
    {
        TParent Combine(TParent original, TComp component);
        TParent Remove(TParent original, TComp component);
    }
}