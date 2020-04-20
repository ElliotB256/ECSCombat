using Battle.Combat;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.Equipment
{
    /// <summary>
    /// Sets the target of all equipment to follow that of the parent. Eg for weapons, etc.
    /// 
    /// Foreach over all entities with parent+equipment+Target. Set target equal to parent target.
    /// 
    /// TODO: In future, only required when parent changes target.
    /// </summary>
    [
        UpdateInGroup(typeof(EquipmentUpdateGroup))
    ]
    public class EquipmentTargetsParentTargetSystem : JobComponentSystem
    {
        public EntityQuery Query;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var targetArray = new NativeArray<Entity>(Query.CalculateEntityCount(), Allocator.TempJob);
            var getTargetsJH = new GetParentTargets {
                ParentTargets = targetArray,
                Targets = GetComponentDataFromEntity<Target>(true)
            }.Schedule(Query, inputDeps);
            var setTargetsJH = new SetTargets { ParentTargets = targetArray }.Schedule(Query, getTargetsJH);
            return setTargetsJH;
        }

        protected override void OnCreate()
        {
            Query = GetEntityQuery(
                new EntityQueryDesc
                {
                    All = new[] { ComponentType.ReadOnly<Equipment>(), ComponentType.ReadOnly<Parent>(), ComponentType.ReadWrite<Target>() }
                });
        }


        [BurstCompile]
        [RequireComponentTag(typeof(Equipment))]
        struct GetParentTargets : IJobForEachWithEntity<Parent,Target>
        {
            [ReadOnly] public ComponentDataFromEntity<Target> Targets;
            [WriteOnly] public NativeArray<Entity> ParentTargets;

            public void Execute(
                Entity attack,
                int index,
                [ReadOnly] ref Parent parent,
                [ReadOnly] ref Target target
                )
            {
                if (!Targets.Exists(parent.Value))
                    ParentTargets[index] = Entity.Null;
                else
                    ParentTargets[index] = Targets[parent.Value].Value;
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Equipment))]
        struct SetTargets : IJobForEachWithEntity<Target>
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> ParentTargets;

            public void Execute(
                Entity attack,
                int index,
                [WriteOnly] ref Target target
                )
            {
                target = new Target { Value = ParentTargets[index] };
            }
        }

    }
}
