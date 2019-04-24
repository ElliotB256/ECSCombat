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
    public class EngineSystem : JobComponentSystem
    {
        protected EntityQuery m_engineParents;
        protected EntityQuery m_enginesToBeEnabled;
        protected EntityQuery m_enginesToBeDisabled;
        private EquipmentBufferSystem m_equipmentBuffer;

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            //EquipmentBufferSystem
            var AddJH = new UpdateParentSpeedJob {
                AddingComponent = true,
                EntityBuffer = m_equipmentBuffer.CreateCommandBuffer().ToConcurrent(),
                MaxSpeed = GetComponentDataFromEntity<MaxSpeed>()
            }.Schedule(m_enginesToBeEnabled, inputDependencies);
            m_equipmentBuffer.AddJobHandleForProducer(AddJH);
            return AddJH;
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

            //m_engineParents = GetEntityQuery(new EntityQueryDesc
            //{
            //    All = new[] { ComponentType.ReadWrite<MaxSpeed>(), ComponentType.ReadWrite<MaxTurnSpeed>() }
            //});

            m_equipmentBuffer = World.GetOrCreateSystem<EquipmentBufferSystem>();
        }

        // We want to get a list of entities to change and the amounts to change by.
        //
        // 1. Create NativeMultiHashMap<Entity,Data> of all delta equipment entities.
        // 2. Create NativeArray<Data>, aligned with HashMap<,> Entity Keys, which aggregates the deltas for each entity

        ///// <summary>
        ///// Adds or removes system state components.
        ///// </summary>
        //[BurstCompile]
        //struct AddRemoveSystemStateJob : IJobForEachWithEntity<Engine, Parent>
        //{
        //    public EntityCommandBuffer.Concurrent EntityBuffer;

        //    /// <summary>
        //    /// True if system state component should be added
        //    /// </summary>
        //    public bool AddSystemStateComponent;

        //    public void Execute(
        //        Entity e,
        //        int index,
        //        [ReadOnly] ref Engine engine,
        //        [ReadOnly] ref Parent parent
        //        )
        //    {
        //        if (AddSystemStateComponent)
        //            EntityBuffer.AddComponent(index, e, new EngineSystemState { });
        //        else
        //            EntityBuffer.RemoveComponent<EngineSystemState>(index, e);
        //    }
        //}

        [BurstCompile]
        struct UpdateParentSpeedJob : IJobForEachWithEntity<Engine, Parent>
        {
            public ComponentDataFromEntity<MaxSpeed> MaxSpeed;
            public EntityCommandBuffer.Concurrent EntityBuffer;

            /// <summary>
            /// True if components are being added.
            /// </summary>
            public bool AddingComponent;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref Engine engine,
                [ReadOnly] ref Parent parent
                )
            {
                float Value = 0f;
                if (AddingComponent)
                {
                    Value = engine.Thrust;
                    EntityBuffer.AddComponent(index, e, new EngineSystemState { });
                }
                else
                {
                    Value = -engine.Thrust;
                    EntityBuffer.RemoveComponent<EngineSystemState>(index, e);
                }

                if (MaxSpeed.Exists(parent.Value))
                {
                    MaxSpeed current = MaxSpeed[parent.Value];
                    current.Value += Value;
                    MaxSpeed[parent.Value] = current;
                }
            }
        }
    }
}