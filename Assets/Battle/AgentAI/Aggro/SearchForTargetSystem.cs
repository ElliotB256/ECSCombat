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
        private NativeArray<AgentCategory.eType> m_targetTypes;
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
            int targetNum = m_targetQuery.CalculateEntityCount();
            int pickerNum = m_pickerQuery.CalculateEntityCount();
            m_targetBins = new NativeMultiHashMap<int, int>(targetNum, Allocator.TempJob);

            m_targetTypes = new NativeArray<AgentCategory.eType>(targetNum, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var targetTypesJH = new GetTargetTypesJob
            {
                TargetCategories = GetArchetypeChunkComponentType<AgentCategory>(true),
                TargetTypes = m_targetTypes
            }.Schedule(m_targetQuery, inputDependencies);
            m_targetPositions = m_targetQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob, out var copyTargetPosJob);
            m_targetTeams = m_targetQuery.ToComponentDataArray<Team>(Allocator.TempJob, out var copyTargetTeamsJob);
            m_targetIds = m_targetQuery.ToEntityArray(Allocator.TempJob, out JobHandle copyTargetEntityJob);
            var copyJobs = JobHandle.CombineDependencies(copyTargetEntityJob, copyTargetTeamsJob);
            copyJobs = JobHandle.CombineDependencies(copyJobs, targetTypesJH);

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
                PickerOrders = GetArchetypeChunkComponentType<TargetingOrders>(true),
                Targets = m_targetIds,
                TargetPositions = m_targetPositions,
                TargetTeams = m_targetTeams,
                TargetMap = m_targetBins,
                TargetTypes = m_targetTypes,
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
            m_targetTypes.Dispose();
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
        /// Gets the types of all targets, where possible. Otherwise, sets Type to None.
        /// </summary>
        [BurstCompile]
        struct GetTargetTypesJob : IJobChunk
        {
            [WriteOnly] public NativeArray<AgentCategory.eType> TargetTypes;
            [ReadOnly] public ArchetypeChunkComponentType<AgentCategory> TargetCategories;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                if (chunk.Has(TargetCategories))
                {
                    var categories = chunk.GetNativeArray(TargetCategories);
                    for (int i = 0; i < chunk.Count; i++)
                        TargetTypes[i + firstEntityIndex] = categories[i].Type;
                }
                else
                {
                    for (int i = 0; i < chunk.Count; i++)
                        TargetTypes[i + firstEntityIndex] = AgentCategory.eType.None;
                }
            }
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
            [ReadOnly] public ArchetypeChunkComponentType<TargetingOrders> PickerOrders;
            public ArchetypeChunkComponentType<Target> PickerTargets;

            //Target arrays
            [ReadOnly] public NativeArray<Entity> Targets;
            [ReadOnly] public NativeArray<Team> TargetTeams;
            [ReadOnly] public NativeArray<LocalToWorld> TargetPositions;
            [ReadOnly] public NativeMultiHashMap<int, int> TargetMap;
            [ReadOnly] public NativeArray<AgentCategory.eType> TargetTypes;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var localToWorlds = chunk.GetNativeArray(PickerLocalToWorld);
                var aggroRadii = chunk.GetNativeArray(PickerAggroRadii);
                var teams = chunk.GetNativeArray(PickerTeams);
                var pickerTargets = chunk.GetNativeArray(PickerTargets);
                var pickerTeams = chunk.GetNativeArray(PickerTeams);

                // Targeting orders
                var hasPickerOrders = chunk.Has(PickerOrders);
                var pickerOrders = chunk.GetNativeArray(PickerOrders);
                var defaultPickerOrders = new TargetingOrders { Discouraged = AgentCategory.eType.None, Preferred = AgentCategory.eType.None };

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

                    var orders = hasPickerOrders ? pickerOrders[picker] : defaultPickerOrders;

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
                                orders,
                                TargetTypes[targetIndex],
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
                                orders,
                                TargetTypes[targetIndex],
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
                TargetingOrders orders,
                AgentCategory.eType targetType,
                ref float score,
                ref Entity currentTarget,
                Entity candidate)
            {
                // Cannot target if on the same team.
                if (pickerTeam.ID == targetTeam.ID)
                    return;

                // Cannot target if outside aggro radius.
                var newScore = math.lengthsq(targetPos - pickerPos);
                if (newScore > aggroRadius * aggroRadius)
                    return;

                // Favored and discouraged targets.
                if ((orders.Preferred & targetType) > 0)
                    newScore /= 5f;
                if ((orders.Discouraged & targetType) > 0)
                    newScore *= 5f;

                if (newScore < score)
                {
                    score = newScore;
                    currentTarget = candidate;
                }
            }
        }
    }
}