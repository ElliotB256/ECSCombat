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
    [AlwaysUpdateSystem]
    public class SearchForTargetSystem : JobComponentSystem
    {
        bool hasRunBefore = false;

        private EntityQuery m_targetQuery;
        private EntityQuery m_pickerQuery;
        private NativeArray<LocalToWorld> m_targetPositions;
        private NativeArray<Team> m_targetTeams;
        private NativeArray<Entity> m_targetIds;
        private NativeMultiHashMap<int, int> m_targetBins;

        /// <summary>
        /// Cell size used for hash map sorting
        /// </summary>
        public const float HASH_CELL_SIZE = 10.0f;

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            // Dispose of memory allocated on previous iteration.
            if (hasRunBefore)
                DisposeNatives();

            m_targetQuery.AddDependency(inputDependencies);
            m_pickerQuery.AddDependency(inputDependencies);
            int targetNum = m_targetQuery.CalculateLength();
            int pickerNum = m_pickerQuery.CalculateLength();
            m_targetBins = new NativeMultiHashMap<int, int>(targetNum, Allocator.TempJob);

            m_targetPositions = m_targetQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob, out var copyTargetPosJob);
            m_targetTeams = m_targetQuery.ToComponentDataArray<Team>(Allocator.TempJob, out var copyTargetTeamsJob);
            m_targetIds = m_targetQuery.ToEntityArray(Allocator.TempJob, out JobHandle copyTargetEntityJob);
            var copyJobs = JobHandle.CombineDependencies(copyTargetEntityJob, copyTargetTeamsJob);

            // Once positions are copied over, we sort the positions into a hashmap.
            var hashTargetPosJob = new HashPositions() { CellSize = HASH_CELL_SIZE, hashMap = m_targetBins.ToConcurrent() }.Schedule(m_targetQuery, copyTargetPosJob);
            var hashBarrier = JobHandle.CombineDependencies(hashTargetPosJob, copyJobs);

            // Having sorted entities into buckets by spatial pos, we loop through the entities and find nearby entities (in nearby buckets).
            var findTargetsJH = new IdentifyBestTargetChunkJob()
            {
                CellSize = HASH_CELL_SIZE,
                PickerAggroRadii = GetArchetypeChunkComponentType<AggroRadius>(true),
                PickerLocalToWorld = GetArchetypeChunkComponentType<LocalToWorld>(true),
                PickerTargets = GetArchetypeChunkComponentType<Target>(false),
                PickerTeams = GetArchetypeChunkComponentType<Team>(true),
                Targets = m_targetIds,
                TargetPositions = m_targetPositions,
                TargetTeams = m_targetTeams,
                TargetMap = m_targetBins
            }.Schedule(m_pickerQuery, hashBarrier);

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
            m_targetIds.Dispose();
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
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ReadOnly<Team>(),
                    ComponentType.ReadOnly<Targetable>()
                }
            });

            m_pickerQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ReadOnly<Team>(),
                    ComponentType.ReadOnly<AggroRadius>(),
                    ComponentType.ReadWrite<Target>()
                }
            });
        }

        /// <summary>
        /// Sorts positions into a hashmap.
        /// </summary>
        [BurstCompile]
        struct HashPositions : IJobForEachWithEntity<LocalToWorld>
        {
            public NativeMultiHashMap<int, int>.Concurrent hashMap;
            public float CellSize;

            public void Execute(Entity entity, int index, [ReadOnly] ref LocalToWorld localToWorld)
            {
                var position = localToWorld.Position;
                float2 vec = new float2(position.x, position.z);
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
        /// Respects targeting orders where possible.
        /// </summary>
        [BurstCompile]
        struct IdentifyBestTargetChunkJob : IJobChunk
        {
            public float CellSize;

            //Picker components
            [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> PickerLocalToWorld;
            [ReadOnly] public ArchetypeChunkComponentType<AggroRadius> PickerAggroRadii;
            [ReadOnly] public ArchetypeChunkComponentType<Team> PickerTeams;
            public ArchetypeChunkComponentType<Target> PickerTargets;

            //Target arrays
            [ReadOnly] public NativeArray<Entity> Targets;
            [ReadOnly] public NativeArray<Team> TargetTeams;
            [ReadOnly] public NativeArray<LocalToWorld> TargetPositions;
            [ReadOnly] public NativeMultiHashMap<int, int> TargetMap;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var localToWorlds = chunk.GetNativeArray(PickerLocalToWorld);
                var aggroRadii = chunk.GetNativeArray(PickerAggroRadii);
                var teams = chunk.GetNativeArray(PickerTeams);
                var pickerTargets = chunk.GetNativeArray(PickerTargets);
                var pickerTeams = chunk.GetNativeArray(PickerTeams);

                for (int picker = 0; picker < chunk.Count; picker++)
                {
                    // Ignore entities which already have a target.
                    if (pickerTargets[picker].Value != Entity.Null)
                        continue;

                    // Initialise target loop variables.
                    float score = float.PositiveInfinity;
                    Entity currentTarget = Entity.Null;

                    // Search all bins that cover the given aggro radius.
                    float radius = aggroRadii[picker].Value;
                    var pickerPosition = localToWorlds[picker].Position;
                    float2 vec = new float2(pickerPosition.x, pickerPosition.z);
                    var minBinCoords = HashPositions.BinCoordinates(vec - radius, CellSize);
                    var maxBinCoords = HashPositions.BinCoordinates(vec + radius, CellSize);

                    for (int x = minBinCoords.x; x <= maxBinCoords.x; x++)
                    {
                        for (int y = minBinCoords.y; y <= maxBinCoords.y; y++)
                        {
                            // Identify bucket to search
                            var hash = HashPositions.Hash(new int2(x, y));

                            // Check targets within each bucket.
                            if (!TargetMap.TryGetFirstValue(hash, out int targetIndex, out NativeMultiHashMapIterator<int> iterator))
                                continue;
                            CheckTarget(
                                pickerTeams[picker],
                                TargetTeams[targetIndex],
                                TargetPositions[targetIndex].Position,
                                pickerPosition,
                                aggroRadii[picker].Value,
                                ref score,
                                ref currentTarget,
                                Targets[targetIndex]
                                );

                            while (TargetMap.TryGetNextValue(out targetIndex, ref iterator))
                                CheckTarget(
                                pickerTeams[picker],
                                TargetTeams[targetIndex],
                                TargetPositions[targetIndex].Position,
                                pickerPosition,
                                aggroRadii[picker].Value,
                                ref score,
                                ref currentTarget,
                                Targets[targetIndex]
                                );
                        }
                    }

                    // If a target was found, write it.
                    if (currentTarget != Entity.Null)
                        pickerTargets[picker] = new Target { Value = currentTarget };
                }
            }

            /// <summary>
            /// Test a given entity to determine if it is the best target.
            /// Overwrites currentTarget if the tested entity is best.
            /// </summary>
            /// <param name="targetIndex">index of entity to test</param>
            /// <param name="score">the current score, to beat</param>
            /// <param name=""></param>
            /// <param name="currentTarget">the current target entity</param>
            public void CheckTarget(
                Team pickerTeam,
                Team targetTeam,
                float3 targetPos,
                float3 pickerPos,
                float aggroRadius,
                ref float score,
                ref Entity currentTarget,
                Entity candidate)
            {
                // Cannot target if on the same team.
                if (pickerTeam.ID == targetTeam.ID)
                    return;

                // Cannot target if outside aggro radius.
                var distanceSq = math.lengthsq(targetPos - pickerPos);
                if (distanceSq > aggroRadius * aggroRadius)
                    return;

                if (distanceSq < score)
                {
                    score = distanceSq;
                    currentTarget = candidate;
                }
            }
        }
    }
}