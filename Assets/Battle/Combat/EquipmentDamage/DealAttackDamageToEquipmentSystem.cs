using Battle.Equipment;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Battle.Combat
{
    /// <summary>
    /// Deals attack damage to equipment on a hit entity.
    /// </summary>
    [
        UpdateInGroup(typeof(AttackResultSystemsGroup))
        ]
    public class DealAttackDamageToEquipmentSystem : JobComponentSystem
    {
        EntityQuery AttackQuery;
        NativeArray<Entity> Attacks;

        protected override void OnCreateManager()
        {
            AttackQuery = GetEntityQuery(
                new EntityQueryDesc
                {
                    All = new[] { ComponentType.ReadOnly<Attack>(), ComponentType.ReadOnly<Target>(), ComponentType.ReadOnly<Damage>() }
                });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            Attacks = AttackQuery.ToEntityArray(Allocator.TempJob);
            return new UpdateJob()
            {
                Random = new Random((uint)UnityEngine.Random.Range(1, 10000)),
            Attacks = Attacks,
                AttackTargets = GetComponentDataFromEntity<Target>(true),
                AttackDamages = GetComponentDataFromEntity<Damage>(true),
                EquipmentLists = GetBufferFromEntity<EquipmentList>(true),
                CombatSize = GetComponentDataFromEntity<CombatSize>(true),
                EquipmentHealths = GetComponentDataFromEntity<Health>(false)                
            }.Schedule(inputDeps);
        }

        protected struct UpdateJob : IJob
        {
            public Random Random;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> Attacks;
            [ReadOnly] public ComponentDataFromEntity<Target> AttackTargets;
            [ReadOnly] public ComponentDataFromEntity<Damage> AttackDamages;
            [ReadOnly] public BufferFromEntity<EquipmentList> EquipmentLists;
            public ComponentDataFromEntity<Health> EquipmentHealths;
            [ReadOnly] public ComponentDataFromEntity<CombatSize> CombatSize;

            public void Execute()
            {
                foreach (Entity attack in Attacks)
                {
                    var target = AttackTargets[attack];

                    if (!EquipmentLists.Exists(target.Value))
                        continue;

                    var equipmentList = EquipmentLists[target.Value];
                    foreach (Entity equipment in equipmentList.AsNativeArray())
                    {
                        if (!EquipmentHealths.Exists(equipment))
                            continue;

                        // If both target and equipment have sizes, determine chance based on their area.
                        var baseChance = 0.5f;
                        if (CombatSize.Exists(equipment) && CombatSize.Exists(target.Value))
                        {
                            baseChance = math.pow(CombatSize[equipment].Value, 2f) / math.pow(CombatSize[target.Value].Value, 1f);
                        }

                        if (Random.NextFloat(0f,1f) > baseChance)
                           continue;

                        var health = EquipmentHealths[equipment];
                        health.Value -= AttackDamages[attack].Value;
                        EquipmentHealths[equipment] = health;
                    }
                }
            }
        }
    }
}