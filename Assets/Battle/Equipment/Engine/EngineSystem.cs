using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Battle.Combat;
using Battle.Movement;

namespace Battle.Equipment
{
    /// <summary>
    /// Modifies a Parent's maximum speed as Engines are added/removed.
    /// </summary>
    [AlwaysUpdateSystem]
    public class EngineSystem : JobComponentSystem
    {
        protected EntityQuery m_engineParents;
        protected EntityQuery m_enginesToBeEnabled;
        protected EntityQuery m_enginesToBeDisabled;
        private EquipmentBufferSystem m_equipmentBuffer;
        bool hasRunBefore = false;
        private NativeMultiHashMap<Entity, Engine> AddedHashMap;
        private NativeMultiHashMap<Entity, Engine> RemovedHashMap;

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var removedCount = m_enginesToBeDisabled.CalculateLength();
            var addedCount = m_enginesToBeEnabled.CalculateLength();
            if (hasRunBefore)
            {
                RemovedHashMap.Dispose();
                AddedHashMap.Dispose();
            }

            AddedHashMap = new NativeMultiHashMap<Entity, Engine>(addedCount, Allocator.TempJob);
            RemovedHashMap = new NativeMultiHashMap<Entity, Engine>(removedCount, Allocator.TempJob);
            hasRunBefore = true;

            var sortAddedJH = new AddSystemStateJob
            {
                EquipmentMap = AddedHashMap.ToConcurrent(),
                EntityBuffer = m_equipmentBuffer.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(m_enginesToBeEnabled, inputDependencies);
            var sortRemovedJH = new RemoveSystemStateJob
            {
                EquipmentMap = RemovedHashMap.ToConcurrent(),
                EntityBuffer = m_equipmentBuffer.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(m_enginesToBeDisabled, inputDependencies);
            m_equipmentBuffer.AddJobHandleForProducer(sortAddedJH);
            m_equipmentBuffer.AddJobHandleForProducer(sortRemovedJH);
            var combinedJH = JobHandle.CombineDependencies(sortAddedJH, sortRemovedJH);

            var updateJH = new UpdateMaxSpeeds
            {
                AddedEquipmentMap = AddedHashMap,
                RemovedEquipmentMap = RemovedHashMap,
                MaxSpeed = GetComponentDataFromEntity<Speed>()
            }.Schedule(combinedJH);
            
            return updateJH;
        }

        protected override void OnCreate()
        {
            // We create queries to select all engines that must be enabled, and all engines that must be disabled.
            m_enginesToBeEnabled = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<Engine>(), ComponentType.ReadOnly<Parent>() },
                None = new[] { ComponentType.ReadWrite<EngineSystemState>() }
            });

            m_enginesToBeDisabled = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<EngineSystemState>(), ComponentType.ReadOnly<Parent>() },
                None = new[] { ComponentType.ReadWrite<Engine>() }
            });

            m_equipmentBuffer = World.GetOrCreateSystem<EquipmentBufferSystem>();
        }

        protected override void OnStopRunning()
        {
            if (!hasRunBefore)
            {
                RemovedHashMap.Dispose();
                AddedHashMap.Dispose();
            }
        }

        // We want to get a list of entities to change and the amounts to change by.
        //
        // 1. Create NativeMultiHashMap<Entity,Data> of all delta equipment entities.
        // 2. Copy NativeArray<Data> for the parents
        // 3. Update NativeArray<Data> for the parents using the HashMap -> Copying wasteful?
        // 4. Set parent Data using IJobForEachWithEntity

        /// <summary>
        /// Adds or removes system state components.
        /// </summary>
        //[BurstCompile]
        struct AddSystemStateJob : IJobForEachWithEntity<Engine, Parent>
        {
            public EntityCommandBuffer.Concurrent EntityBuffer;
            public NativeMultiHashMap<Entity, Engine>.Concurrent EquipmentMap;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref Engine engine,
                [ReadOnly] ref Parent parent
                )
            {
                EquipmentMap.Add(parent.Value, engine);
                EntityBuffer.AddComponent(index, e, new EngineSystemState { Thrust = engine.Thrust });
            }
        }

        /// <summary>
        /// Adds or removes system state components.
        /// </summary>
        //[BurstCompile]
        struct RemoveSystemStateJob : IJobForEachWithEntity<EngineSystemState, Parent>
        {
            public EntityCommandBuffer.Concurrent EntityBuffer;
            public NativeMultiHashMap<Entity, Engine>.Concurrent EquipmentMap;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref EngineSystemState engine,
                [ReadOnly] ref Parent parent
                )
            {
                EquipmentMap.Add(parent.Value, new Engine { Thrust = engine.Thrust });
                EntityBuffer.RemoveComponent<EngineSystemState>(index, e);
            }
        }

        //[BurstCompile]
        struct UpdateMaxSpeeds : IJob
        {
            public ComponentDataFromEntity<Speed> MaxSpeed;
            [ReadOnly] public NativeMultiHashMap<Entity, Engine> AddedEquipmentMap;
            [ReadOnly] public NativeMultiHashMap<Entity, Engine> RemovedEquipmentMap;

            public void Execute()
            {
                var addedToEntities = AddedEquipmentMap.GetKeyArray(Allocator.Temp);
                for (int i=0; i<addedToEntities.Length; i++)
                {
                    var parent = addedToEntities[i];
                    if (!MaxSpeed.Exists(parent))
                        continue;

                    Speed current = MaxSpeed[parent];
                    AddedEquipmentMap.TryGetFirstValue(parent, out Engine engine, out var iterator);
                    do
                    {
                        current.Value += engine.Thrust;
                    } while (AddedEquipmentMap.TryGetNextValue(out engine, ref iterator));

                    MaxSpeed[parent] = current;
                }
                addedToEntities.Dispose();

                var removedFromEntities = RemovedEquipmentMap.GetKeyArray(Allocator.Temp);
                for (int i = 0; i < removedFromEntities.Length; i++)
                {
                    var parent = addedToEntities[i];
                    if (!MaxSpeed.Exists(parent))
                        continue;

                    Speed current = MaxSpeed[parent];
                    RemovedEquipmentMap.TryGetFirstValue(parent, out Engine engine, out var iterator);
                    do
                    {
                        current.Value -= engine.Thrust;
                    } while (RemovedEquipmentMap.TryGetNextValue(out engine, ref iterator));

                    MaxSpeed[parent] = current;
                }
                removedFromEntities.Dispose();
            }
        }
    }
}