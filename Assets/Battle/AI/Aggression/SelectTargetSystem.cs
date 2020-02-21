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
    /// Selects targets for entities.
    /// </summary>
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(AISystemGroup))]
    public class SelectTargetSystem : JobComponentSystem
    {
        private EntityQuery TargetQuery;
        private EntityQuery PickerQuery;

        /// <summary>
        /// Cell size used for hash map sorting
        /// </summary>
        public const float HASH_CELL_SIZE = 10.0f;

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            TargetQuery.AddDependency(inputDependencies);
            PickerQuery.AddDependency(inputDependencies);
            int targetN = TargetQuery.CalculateEntityCount();
            int pickerN = PickerQuery.CalculateEntityCount();

            // Allocate arrays used by the system
            var pickerPositions = PickerQuery.ToComponentDataArray<AggroLocation>(Allocator.TempJob, out var pposJH);
            var targetPositions = TargetQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob, out var tposJH);
            var targetTeams = TargetQuery.ToComponentDataArray<Team>(Allocator.TempJob, out var tteamJH);
            var targetIDs = TargetQuery.ToEntityArray(Allocator.TempJob, out JobHandle tidJH);
            var targetBins = new NativeMultiHashMap<int, int>(targetN, Allocator.TempJob);

            // Get target types
            var targetTypes = new NativeArray<AgentCategory.eType>(targetN, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var ttypeJH = new GetTargetTypesJob
            {
                TargetCategories = GetArchetypeChunkComponentType<AgentCategory>(true),
                TargetTypes = targetTypes
            }.Schedule(TargetQuery, inputDependencies);

            var initialiseJH = JobHandle.CombineDependencies(pposJH, tposJH, JobHandle.CombineDependencies(tteamJH, tidJH, ttypeJH));

            // Sort target positions into a hashmap.
            var hashTargetPosJob = new HashPositions() { CellSize = HASH_CELL_SIZE, hashMap = targetBins.AsParallelWriter() }.Schedule(TargetQuery, tposJH);
            var hashBarrier = JobHandle.CombineDependencies(hashTargetPosJob, initialiseJH);

            // Loop through picker entities and find suitable targets.
            var findTargetsJH = new IdentifyBestTargetChunkJob()
            {
                CellSize = HASH_CELL_SIZE,
                PickerAggroRadii = GetArchetypeChunkComponentType<AggroRadius>(true),
                PickerLocalToWorld = GetArchetypeChunkComponentType<AggroLocation>(true),
                PickerTargets = GetArchetypeChunkComponentType<Target>(false),
                PickerTeams = GetArchetypeChunkComponentType<Team>(true),
                PickerOrders = GetArchetypeChunkComponentType<TargetingOrders>(true),
                Targets = targetIDs,
                TargetPositions = targetPositions,
                TargetTeams = targetTeams,
                TargetMap = targetBins,
                TargetTypes = targetTypes,
            }.Schedule(PickerQuery, hashBarrier);

            // Dispose of native arrays
            pickerPositions.Dispose(findTargetsJH);
            targetPositions.Dispose(findTargetsJH);
            targetTeams.Dispose(findTargetsJH);
            targetIDs.Dispose(findTargetsJH);
            targetBins.Dispose(findTargetsJH);
            targetTypes.Dispose(findTargetsJH);

            return findTargetsJH;
        }

        protected override void OnCreate()
        {
            TargetQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ReadOnly<Team>(),
                    ComponentType.ReadOnly<Targetable>()
                }
            });

            PickerQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ReadOnly<Team>(),
                    ComponentType.ReadOnly<AggroRadius>(),
                    ComponentType.ReadOnly<AggroLocation>(),
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
            public NativeMultiHashMap<int, int>.ParallelWriter hashMap;
            public float CellSize;

            public void Execute(Entity entity, int index, [ReadOnly] ref LocalToWorld localToWorld)
            {
                var position = localToWorld.Position;
                float2 vec = new float2(position.x, position.z);
                int2 rounded = new int2(math.floor(vec / CellSize));
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
            [ReadOnly] public ArchetypeChunkComponentType<AggroLocation> PickerLocalToWorld;
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