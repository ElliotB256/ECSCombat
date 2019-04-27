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
    [UpdateInGroup(typeof(EquipmentUpdateGroup))]
    public class EngineSystem : AggregateEquipmentSystem<Engine>
    {
        protected override JobHandle CreateProcessJobHandle(JobHandle inputDependencies)
        {
            var job = new UpdateMaxSpeeds()
            {
                AddedEquipmentMap = AddedEquipment,
                RemovedEquipmentMap = RemovedEquipment,
                MaxSpeed = GetComponentDataFromEntity<Speed>()
            };
            var jobHandle = job.Schedule(inputDependencies);
            return jobHandle;
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