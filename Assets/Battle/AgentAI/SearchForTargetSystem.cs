using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Battle.Combat;

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

        private NativeArray<Translation> m_targetPositions;
        private NativeArray<Translation> m_pickerPositions;
        private NativeArray<Team> m_pickerTeams;
        private NativeArray<Team> m_targetTeams;
        private NativeArray<Entity> m_targetIds;
        private NativeArray<AggroRadius> m_aggroRadii;

        private NativeMultiHashMap<int, int> m_targetBins;

        /// <summary>
        /// Cell size for sorting via hash map
        /// </summary>
        public float UnitCellSize = 10.0f;

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            // if not first time, dispose of memory allocated on previous iteration.
            if (hasRunBefore)
                DisposeNatives();

            m_targetQuery.AddDependency(inputDependencies);
            m_pickerQuery.AddDependency(inputDependencies);
            int targetNum = m_targetQuery.CalculateLength();
            int pickerNum = m_pickerQuery.CalculateLength();

            // Allocate native arrays used by this job and copy data into NativeArrays

            //NativeMultiHashMaps are used to sort the entities into space.
            //  The basic principle is as follows. The position of each entity is hashed into a integer, which thus encodes
            //  all coordinates in a single number. Two nearby entities hash to the same number, and so get put into the
            //  same bin. We can then look into each bin to figure out which entities are near each other.
            m_targetBins = new NativeMultiHashMap<int, int>(targetNum, Allocator.TempJob);

            m_targetPositions = m_targetQuery.ToComponentDataArray<Translation>(Allocator.TempJob, out var copyTargetPosJob);
            m_pickerPositions = m_pickerQuery.ToComponentDataArray<Translation>(Allocator.TempJob, out var copyPickerPosJob);
            m_targetTeams = m_targetQuery.ToComponentDataArray<Team>(Allocator.TempJob, out var copyTargetTeamsJob);
            m_pickerTeams = m_pickerQuery.ToComponentDataArray<Team>(Allocator.TempJob, out var copyPickerTeamsJob);
            m_targetIds = m_targetQuery.ToEntityArray(Allocator.TempJob, out JobHandle copyTargetEntityJob);
            m_aggroRadii = m_pickerQuery.ToComponentDataArray<AggroRadius>(Allocator.TempJob, out JobHandle copyAggroRadii);
            var miscCopies = JobHandle.CombineDependencies(copyAggroRadii,
                JobHandle.CombineDependencies(copyTargetEntityJob, copyPickerTeamsJob, copyTargetTeamsJob)
                );

            // Once positions are copied over, we sort the positions into a hashmap.
            var hashTargetPosition = new HashPositions() { CellSize = UnitCellSize, hashMap = m_targetBins.ToConcurrent() };
            var hashTargetJob = hashTargetPosition.Schedule(m_targetQuery, copyTargetPosJob);
            var hashBarrier = JobHandle.CombineDependencies(copyPickerPosJob, hashTargetJob, miscCopies);

            // Having sorted entities into buckets by spatial pos, we loop through the entities and find nearby entities (in nearby buckets).
            var findTargets = new IdentifyBestTarget()
            {
                targetPositions = m_targetPositions,
                pickerPositions = m_pickerPositions,
                pickerTeams = m_pickerTeams,
                targetTeams = m_targetTeams,
                targetMap = m_targetBins,
                targetIds = m_targetIds,
                CellSize = UnitCellSize,
                aggroRadii = m_aggroRadii
            };
            var findTargetsJH = findTargets.Schedule(m_pickerQuery, hashBarrier);

            hasRunBefore = true;

            return findTargetsJH;
        }

        /// <summary>
        /// Clean up NativeArrays used in the job.
        /// </summary>
        public void DisposeNatives()
        {
            m_targetBins.Dispose();
            m_targetPositions.Dispose();
            m_targetTeams.Dispose();
            m_pickerPositions.Dispose();
            m_pickerTeams.Dispose();
            m_targetIds.Dispose();
            m_aggroRadii.Dispose();
        }

        protected override void OnStopRunning()
        {
            // if memory exists, dispose of memory allocated on previous iteration.
            if (hasRunBefore)
                DisposeNatives();
        }

        protected override void OnCreate()
        {
            m_targetQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<Team>(),
                    ComponentType.ReadOnly<Targetable>()
                }
            });

            m_pickerQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<Team>(),
                    ComponentType.ReadOnly<AggroRadius>(),
                    ComponentType.ReadWrite<Target>()
                    //ComponentType.ReadOnly<IdleBehaviour>()
                }
            });
        }

        /// <summary>
        /// Sorts positions into a hashmap.
        /// </summary>
        [BurstCompile]
        struct HashPositions : IJobForEachWithEntity<Translation>
        {
            public NativeMultiHashMap<int, int>.Concurrent hashMap;
            public float CellSize;

            public void Execute(Entity entity, int index, [ReadOnly] ref Translation pos)
            {
                float2 vec = new float2(pos.Value.x, pos.Value.z);
                var hash = Hash(BinCoordinates(vec, CellSize));
                hashMap.Add(hash, index);
            }

            public static int2 BinCoordinates(float2 position, float CellSize)
            {
                return new int2(math.floor(position / CellSize));
            }

            public static int Hash(int2 binCoords)
            {
                return (int)math.hash(binCoords);
            }
        }

        /// <summary>
        /// Identifies the best target for each picker.
        /// </summary>
        [BurstCompile]
        struct IdentifyBestTarget : IJobForEachWithEntity<Target> // was IJobParallelFor.
        {
            public float CellSize;

            [ReadOnly] public NativeArray<Translation> targetPositions;
            [ReadOnly] public NativeArray<Translation> pickerPositions;
            [ReadOnly] public NativeArray<Team> pickerTeams;
            [ReadOnly] public NativeArray<Team> targetTeams;
            [ReadOnly] public NativeMultiHashMap<int, int> targetMap;
            [ReadOnly] public NativeArray<Entity> targetIds;
            [ReadOnly] public NativeArray<AggroRadius> aggroRadii;


            public void Execute(Entity picker, int pickerIndex, ref Target pickerTarget)
            {
                if (pickerTarget.Value != Entity.Null)
                    return;

                float2 vec = new float2(pickerPositions[pickerIndex].Value.x, pickerPositions[pickerIndex].Value.z);
                float radius = aggroRadii[pickerIndex].Value;

                // We need to search not only the picker bin, but any adjacent bins aswell if radius > binSize.
                var minBinCoords = HashPositions.BinCoordinates(vec-radius, CellSize);
                var maxBinCoords = HashPositions.BinCoordinates(vec+radius, CellSize);

                float score = float.PositiveInfinity;
                Entity target = Entity.Null;
                bool found = false;

                for (int x = minBinCoords.x; x <= maxBinCoords.x; x++)
                {
                    for (int y = minBinCoords.y; y <= maxBinCoords.y; y++)
                    {
                        // Identify bucket to search
                        var hash = HashPositions.Hash(new int2(x,y));

                        // Iterate over the hash map of positions. For each associated entity, determine if it is a good target.
                        if (!targetMap.TryGetFirstValue(hash, out int targetIndex, out NativeMultiHashMapIterator<int> iterator))
                            continue;

                        TestTarget(pickerIndex, targetIndex, radius, ref score, ref found, ref target);

                        while (targetMap.TryGetNextValue(out targetIndex, ref iterator))
                            TestTarget(pickerIndex, targetIndex, radius, ref score, ref found, ref target);
                    }
                }

                // If we found a target, write it.
                if (found)
                    pickerTarget.Value = target;
            }

            /// <summary>
            /// Test a target entity to see if it is a favorable target
            /// </summary>
            /// <param name="targetIndex">index of entity to test</param>
            /// <param name="score">the current score, to beat</param>
            /// <param name=""></param>
            /// <param name="currentTarget">the current target entity</param>
            public void TestTarget(int pickerIndex, int targetIndex, float radius, ref float score, ref bool found, ref Entity currentTarget)
            {
                // Cannot target if on the same team.
                if (pickerTeams[pickerIndex].ID == targetTeams[targetIndex].ID)
                    return;

                var distanceSq = math.lengthsq(targetPositions[targetIndex].Value - pickerPositions[pickerIndex].Value);

                if (distanceSq > radius * radius)
                    return;

                found = true;

                if (distanceSq < score)
                {
                    score = distanceSq;
                    currentTarget = targetIds[targetIndex];
                }
            }
        }
    }
}