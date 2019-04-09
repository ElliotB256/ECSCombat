using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Battle.Combat;
using UnityEngine;

namespace Battle.AI
{
    /// <summary>
    /// Searches for target entities.
    /// 'Pickers' are entities that are currently choosing targets.
    /// 'Targets' are all entities that are acceptable to be chosen as targets.
    /// </summary>
    public class SearchForTargetSystem : JobComponentSystem
    {
        bool hasRunBefore = false;

        private EntityQuery m_targetQuery;
        private EntityQuery m_pickerQuery;

        private NativeArray<float3> m_targetPositions;
        private NativeArray<float3> m_pickerPositions;
        private NativeArray<byte> m_pickerStates;
        private NativeArray<byte> m_pickerTeams;
        private NativeArray<byte> m_targetTeams;
        private NativeArray<Entity> m_targetIds;

        private NativeMultiHashMap<int, int> m_targetBins;
        private NativeMultiHashMap<int, int> m_pickerBins;

        /// <summary>
        /// Cell size for sorting via hash map
        /// </summary>
        public float UnitCellSize = 10.0f;

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            // if not first time, dispose of memory allocated on previous iteration.
            if (hasRunBefore)
            {
                DisposeNatives();
            }

            // Allocating native arrays used by this job.
            int targetNum = m_targetQuery.CalculateLength();
            int pickerNum = m_pickerQuery.CalculateLength();
            m_targetPositions = new NativeArray<float3>(targetNum, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            m_pickerPositions = new NativeArray<float3>(pickerNum, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            m_pickerStates = new NativeArray<byte>(pickerNum, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            m_targetTeams = new NativeArray<byte>(targetNum, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            m_pickerTeams = new NativeArray<byte>(pickerNum, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            //NativeMultiHashMaps are used to sort the entities into space.
            //  The basic principle is as follows. The position of each entity is hashed into a integer, which thus encodes
            //  all coordinates in a single number. Two nearby entities hash to the same number, and so get put into the
            //  same bin. We can then look into each bin to figure out which entities are near each other.
            m_targetBins = new NativeMultiHashMap<int, int>(targetNum, Allocator.TempJob);
            m_pickerBins = new NativeMultiHashMap<int, int>(pickerNum, Allocator.TempJob);


            // Populate the allocated arrays with position information.
            //   Setup the jobs, then schedule for execution.
            var copyTargetPosition = new CopyPosition() { positions = m_targetPositions };
            var copyPickerPosition = new CopyPosition() { positions = m_pickerPositions };
            var copyTargetPosJob = copyTargetPosition.Schedule(m_targetQuery, inputDependencies);
            var copyPickerPosJob = copyPickerPosition.Schedule(m_pickerQuery, inputDependencies);

            JobHandle copyTargetEntityJob;
            var copyPickerStatesJob = new CopyFighterState() { states = m_pickerStates }.Schedule(m_pickerQuery, inputDependencies);
            var copyPickerTeamsJob = new CopyTeam() { teamIds = m_pickerTeams }.Schedule(m_pickerQuery, inputDependencies);
            var copyTargetTeamsJob = new CopyTeam() { teamIds = m_targetTeams }.Schedule(m_targetQuery, inputDependencies);
            m_targetIds = m_targetQuery.ToEntityArray(Allocator.TempJob, out copyTargetEntityJob);
            var miscCopies = JobHandle.CombineDependencies(copyTargetEntityJob,
                JobHandle.CombineDependencies(copyPickerStatesJob, copyPickerTeamsJob, copyTargetTeamsJob));


            // Once positions are copied over, we sort the positions into a hashmap.
            var hashTargetPosition = new HashPositions() { cellRadius = UnitCellSize, hashMap = m_targetBins.ToConcurrent() };
            var hashPickerPosition = new HashPositions() { cellRadius = UnitCellSize, hashMap = m_pickerBins.ToConcurrent() };
            var hashTargetJob = hashTargetPosition.Schedule(m_targetQuery, copyTargetPosJob);
            var hashPickerJob = hashPickerPosition.Schedule(m_pickerQuery, copyPickerPosJob);

            var hashBarrier = JobHandle.CombineDependencies(hashTargetJob, hashPickerJob, miscCopies);


            // Having sorted entities into buckets by spatial pos, we now look in each bucket.
            // We enumerate through picker entities, and look in the buckets associated with them.
            var findTargets = new IdentifyBestTarget()
            {
                targetPositions = m_targetPositions,
                pickerPositions = m_pickerPositions,
                pickerTeams = m_pickerTeams,
                targetTeams = m_targetTeams,
                pickerState = m_pickerStates,
                targetMap = m_targetBins,
                targetIds = m_targetIds,
                cellRadius = UnitCellSize
            };
            var findTargetsJH = findTargets.Schedule(m_pickerQuery, hashBarrier);

            hasRunBefore = true;

            return findTargetsJH;
        }

        public void DisposeNatives()
        {
            m_targetBins.Dispose();
            m_pickerBins.Dispose();
            m_targetPositions.Dispose();
            m_targetTeams.Dispose();
            m_pickerPositions.Dispose();
            m_pickerStates.Dispose();
            m_pickerTeams.Dispose();
            m_targetIds.Dispose();
        }

        protected override void OnStopRunning()
        {
            // if memory exists, dispose of memory allocated on previous iteration.
            if (hasRunBefore)
            {
                DisposeNatives();
            }
        }

        protected override void OnCreate()
        {
            m_targetQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<Team>()
                }
            });

            m_pickerQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<FighterAIState>(),
                    ComponentType.ReadOnly<Team>(),
                    ComponentType.ReadWrite<Target>()
                }
            });
        }

        #region Copying data to/from NativeArray

        [BurstCompile]
        struct CopyPosition : IJobForEachWithEntity<Translation>
        {
            public NativeArray<float3> positions;

            public void Execute(Entity entity, int index, [ReadOnly]ref Translation pos)
            {
                positions[index] = pos.Value;
            }
        }

        [BurstCompile]
        struct CopyFighterState : IJobForEachWithEntity<FighterAIState>
        {
            public NativeArray<byte> states;

            public void Execute(Entity entity, int index, [ReadOnly]ref FighterAIState state)
            {
                states[index] = (byte)state.State;
            }
        }

        [BurstCompile]
        struct CopyTeam : IJobForEachWithEntity<Team>
        {
            public NativeArray<byte> teamIds;

            public void Execute(Entity entity, int index, [ReadOnly]ref Team team)
            {
                teamIds[index] = team.ID;
            }
        }

        #endregion

        /// <summary>
        /// Sorts positions into a hashmap.
        /// </summary>
        [BurstCompile]
        struct HashPositions : IJobForEachWithEntity<Translation>
        {
            public NativeMultiHashMap<int, int>.Concurrent hashMap;
            public float cellRadius;

            public void Execute(Entity entity, int index, [ReadOnly] ref Translation pos)
            {
                var hash = (int)math.hash(new int3(math.floor(pos.Value / cellRadius)));
                hashMap.Add(hash, index);
            }
        }

        /// <summary>
        /// Identifies the best target for each picker.
        /// </summary>
        struct IdentifyBestTarget : IJobForEachWithEntity<Target> // was IJobParallelFor.
        {
            public float cellRadius;

            [ReadOnly] public NativeArray<float3> targetPositions;
            [ReadOnly] public NativeArray<float3> pickerPositions;
            [ReadOnly] public NativeArray<byte> pickerTeams;
            [ReadOnly] public NativeArray<byte> targetTeams;
            [ReadOnly] public NativeArray<byte> pickerState;
            [ReadOnly] public NativeMultiHashMap<int, int> targetMap;
            [ReadOnly] public NativeArray<Entity> targetIds;
            

            public void Execute(Entity picker, int pickerIndex, ref Target pickerTarget)
            {
                // Only select targets for pickers with Idle state.
                if (pickerState[pickerIndex] != (byte)FighterAIState.eState.Idle)
                    return;

                // First, compute hash of this picker.
                float3 pickerPos = pickerPositions[pickerIndex];
                var hash = (int)math.hash(new int3(math.floor(pickerPos / cellRadius)));

                // Iterate over the hash map of positions. For each associated entity, determine if it is a good target.
                bool found = false;
                NativeMultiHashMapIterator<int> iterator;
                int targetIndex;
                if (!targetMap.TryGetFirstValue(hash, out targetIndex, out iterator))
                    return;

                float score = float.PositiveInfinity;
                Entity target = Entity.Null;
                TestTarget(pickerIndex, targetIndex, ref score, ref found, ref target);

                while (targetMap.TryGetNextValue(out targetIndex, ref iterator))
                    TestTarget(pickerIndex, targetIndex, ref score, ref found, ref target);

                // If we found a target, write it.
                if (found)
                {
                    pickerTarget.Value = target;
                }
            }

            /// <summary>
            /// Test a target entity to see if it is a favorable target
            /// </summary>
            /// <param name="targetIndex">index of entity to test</param>
            /// <param name="score">the current score, to beat</param>
            /// <param name=""></param>
            /// <param name="currentTarget">the current target entity</param>
            public void TestTarget(int pickerIndex, int targetIndex, ref float score, ref bool found, ref Entity currentTarget)
            {
                // Cannot target if on the same team.
                if (pickerTeams[pickerIndex] == targetTeams[targetIndex])
                    return;

                found = true;

                var distance = targetPositions[targetIndex] - pickerPositions[pickerIndex];
                var newScore = math.lengthsq(distance);
                if (newScore < score)
                {
                    score = newScore;
                    currentTarget = targetIds[targetIndex];
                }
            }
        }

        //[BurstCompile]
        //struct IdentifyBestTarget : IJobNativeMultiHashMapMergedSharedKeyIndices
        //{
        //    [ReadOnly] public NativeArray<float3> targetPositions;
        //    [ReadOnly] public NativeArray<float3> pickerPositions;
        //    [ReadOnly] public NativeArray<byte> pickerTeams;
        //    [ReadOnly] public NativeArray<byte> targetTeams;
        //    [ReadOnly] public NativeArray<byte> pickerState;

        //    void NearestPosition(NativeArray<float3> targets, float3 position, out int nearestPositionIndex, out float nearestDistance)
        //    {
        //        nearestPositionIndex = 0;
        //        nearestDistance = math.lengthsq(position - targets[0]);
        //        for (int i = 1; i < targets.Length; i++)
        //        {
        //            var targetPosition = targets[i];
        //            var distance = math.lengthsq(position - targetPosition);
        //            var nearest = distance < nearestDistance;

        //            nearestDistance = math.select(nearestDistance, distance, nearest);
        //            nearestPositionIndex = math.select(nearestPositionIndex, i, nearest);
        //        }
        //        nearestDistance = math.sqrt(nearestDistance);
        //    }

        //    public void ExecuteFirst(int key)
        //    {

        //    }

        //    public void ExecuteNext(int firstKey, int key)
        //    {
        //    }
        //}
    }
}