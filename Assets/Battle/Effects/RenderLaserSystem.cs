using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity;

using Battle.Combat;
using System.Collections.Generic;

namespace Battle.Effects
{
    /// <summary>
    /// Render laser beam effects. Hacky.
    /// </summary>
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(LaserEffectSystem))]
    public class RenderAttackLaserSystem : JobComponentSystem
    {
        private RenderLaserComponent m_renderer;
        private EntityQuery m_query;

        protected override void OnCreateManager()
        {
            base.OnCreateManager();
            m_renderer = Object.FindObjectOfType<RenderLaserComponent>();
            m_query = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<LaserBeamEffect>() }
            }
            );
        }
        protected bool hasRunBefore = false;

        protected struct ListLasersJob : IJobForEachWithEntity<LaserBeamEffect>
        {
            [WriteOnly] public NativeArray<LaserBeamEffect> lasers;

            public void Execute(Entity e, int index, ref LaserBeamEffect laserBeam)
            {
                lasers[index] = laserBeam;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (m_renderer == null)
            {
                Debug.LogWarning("You must add a GameObject with RenderLaserComponent to the scene.");
                return inputDeps;
            }

            if (hasRunBefore)
                m_renderer.Beams.Dispose();

            m_renderer.Beams = new NativeArray<LaserBeamEffect>(m_query.CalculateLength(), Allocator.TempJob);
            hasRunBefore = true;
            var jobHandle = new ListLasersJob() { lasers = m_renderer.Beams }.Schedule(m_query, inputDeps);
            m_renderer.LaserSystemJob = jobHandle;
            return jobHandle;
        }

        protected override void OnStopRunning()
        {
            if (hasRunBefore)
                m_renderer.Beams.Dispose();
        }
    }
}