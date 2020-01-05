using Battle.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace Battle.Equipment
{
    /// <summary>
    /// Adds parent team to the equipment as it is being equiped.
    /// 
    /// Very similar to EquipmentTargetsParentTarget
    /// </summary>
    [
        UpdateInGroup(typeof(EquipmentUpdateGroup))
    ]
    public class SetEquipmentTeamOnEquipSystem : JobComponentSystem
    {
        public EntityQuery Query;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var targetArray = new NativeArray<Team>(Query.CalculateEntityCount(), Allocator.TempJob);
            var getTeamsJH = new GetParentTeams
            {
                ParentTeams = targetArray,
                Teams = GetComponentDataFromEntity<Team>(true)
            }.Schedule(Query, inputDeps);
            var setTeamsJH = new SetTeams { ParentTeams = targetArray }.Schedule(Query, getTeamsJH);
            return setTeamsJH;
        }

        protected override void OnCreateManager()
        {
            Query = GetEntityQuery(
                new EntityQueryDesc
                {
                    All = new[] {
                        ComponentType.ReadOnly<Equipment>(),
                        ComponentType.ReadOnly<Parent>(),
                        ComponentType.ReadWrite<Team>(),
                        ComponentType.ReadOnly<Equipping>()
                    }
                });
        }


        /// <summary>
        /// Essentially same as for EquipmentTargetsParentTarget - perhaps some miscellaneous jobs generic library would be useful?
        /// </summary>
        [BurstCompile]
        [RequireComponentTag(typeof(Equipment))]
        struct GetParentTeams : IJobForEachWithEntity<Parent, Team>
        {
            [ReadOnly] public ComponentDataFromEntity<Team> Teams;
            [WriteOnly] public NativeArray<Team> ParentTeams;

            public void Execute(
                Entity attack,
                int index,
                [ReadOnly] ref Parent parent,
                [ReadOnly] ref Team target
                )
            {
                if (!Teams.Exists(parent.Value))
                {
                    Debug.LogWarning("Could not find parent team for attached equipment.");
                    ParentTeams[index] = new Team { ID = 255 };
                }
                else
                    ParentTeams[index] = Teams[parent.Value];
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Equipment))]
        struct SetTeams : IJobForEachWithEntity<Team>
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Team> ParentTeams;

            public void Execute(
                Entity attack,
                int index,
                [WriteOnly] ref Team team
                )
            {
                team = ParentTeams[index];
            }
        }
    }
}
