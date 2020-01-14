using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

using Battle.Combat;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Rendering;

namespace Battle.Effects
{
    ///// <summary>
    ///// Spawns effects of shields being struck.
    ///// </summary>
    //[
    //    UpdateAfter(typeof(PostAttackEntityBuffer)),
    //    UpdateBefore(typeof(DeleteEntitiesSystem))
    //]
    //public class SpawnShieldEffectSystem : JobComponentSystem
    //{
    //    EndSimulationEntityCommandBufferSystem Buffer;

    //    protected struct CreateLaserEffectJob : IJobForEachWithEntity<Attack, Instigator, Target, BeamEffectStyle>
    //    {
           
    //    }

        

    //    protected override void OnCreateManager()
    //    {
    //        Buffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    //    }

    //    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    //    {
           
    //    }
    //}
}