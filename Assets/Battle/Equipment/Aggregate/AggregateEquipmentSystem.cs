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

            var addedToEntities = new NativeArray<Entity>(addedCount, Allocator.TempJob);
            var addedEquipment = new NativeArray<TEquipment>(addedCount, Allocator.TempJob);
            var removedFromEntities = new NativeArray<Entity>(removedCount, Allocator.TempJob);
            var removedEquipment = new NativeArray<TEquipment>(removedCount, Allocator.TempJob);

            var addMapJobHandle = new AddToMapJob
            {
                Equipment = GetArchetypeChunkComponentType<TEquipment>(true),
                Parent = GetArchetypeChunkComponentType<Parent>(true),
                Entities = addedToEntities,
                ChangedEquipment = addedEquipment
            }.Schedule(ComponentsToBeEnabled, Dependency);

            var removeMapJobHandle = new AddToMapJob
            {
                Equipment = GetArchetypeChunkComponentType<TEquipment>(true),
                Parent = GetArchetypeChunkComponentType<Parent>(true),
                Entities = removedFromEntities,
                ChangedEquipment = removedEquipment
            }.Schedule(ComponentsToBeDisabled, Dependency);

            var barrier = JobHandle.CombineDependencies(addMapJobHandle, removeMapJobHandle);

            var addJH = new AggregateJob()
            {
                Entities = addedToEntities,
                ChangedEquipment = addedEquipment,
                EquipmentComponents = GetComponentDataFromEntity<TEquipment>(false),
                Added = true
            }.Schedule(barrier);

            var removeJH = new AggregateJob()
            {
                Entities = removedFromEntities,
                ChangedEquipment = removedEquipment,
                EquipmentComponents = GetComponentDataFromEntity<TEquipment>(false),
                Added = false
            }.Schedule(addJH);

            Dependency = removeJH;

            addedEquipment.Dispose(removeJH);
            addedToEntities.Dispose(removeJH);
            removedEquipment.Dispose(removeJH);
            removedFromEntities.Dispose(removeJH);
        }

        [BurstCompile]
        struct AddToMapJob : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkComponentType<TEquipment> Equipment;
            [ReadOnly] public ArchetypeChunkComponentType<Parent> Parent;
            [NativeDisableParallelForRestriction] public NativeArray<Entity> Entities;
            [NativeDisableParallelForRestriction] public NativeArray<TEquipment> ChangedEquipment;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var equipments = chunk.GetNativeArray(Equipment);
                var parents = chunk.GetNativeArray(Parent);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Entities[firstEntityIndex+i] = parents[i].Value;
                    ChangedEquipment[firstEntityIndex+i] = equipments[i];
                }
            }
        }

        //[BurstCompile]
        struct AggregateJob : IJob
        {
            public ComponentDataFromEntity<TEquipment> EquipmentComponents;
            [ReadOnly] public NativeArray<Entity> Entities;
            [ReadOnly] public NativeArray<TEquipment> ChangedEquipment;
            public bool Added;

            public void Execute()
            {
                for (int i = 0; i < Entities.Length; i++)
                {
                    var parent = Entities[i];
                    if (!EquipmentComponents.Exists(parent))
                        continue;
                    var parentComponent = EquipmentComponents[parent];
                    if (Added)
                        parentComponent.Combine(ChangedEquipment[i]);
                    else
                        parentComponent.Decombine(ChangedEquipment[i]);
                    EquipmentComponents[parent] = parentComponent;
                }
            }
        }
    }

    public interface ICombineable<T> { void Combine(T other); void Decombine(T other); }
}