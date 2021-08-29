using UnityEngine;

using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using Battle.Combat;

namespace Battle.Effects
{
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(AttackResultSystemsGroup))]
        [UpdateAfter(typeof(ShieldsAbsorbDamageSystem))]
        [UpdateAfter(typeof(BeamEffectSystem))]
    public class BeamEffectRenderSystem : SystemBase
    {

        Segments.Batch _batch;
        EntityQuery _queryBeams;

        protected override void OnCreate ()
        {
            string laserBeamPath = "laser-beam";
            Material laserBeamMat = Resources.Load<Material>( laserBeamPath );
            if( laserBeamMat==null ) Debug.LogWarning($"laserBeamMat not found in Resources under given path: \"{laserBeamPath}\"");

            Segments.Core.CreateBatch( out _batch , laserBeamMat );
        }

        protected override void OnDestroy ()
        {
            _batch.Dispose();
        }

        protected override void OnUpdate ()
        {
            var buffer = _batch.buffer;
            buffer.Length = _queryBeams.CalculateEntityCount();

            Dependency =
            Entities
                .WithStoreEntityQueryInField( ref _queryBeams )
                .WithNativeDisableParallelForRestriction( buffer )
                .ForEach( ( int entityInQueryIndex , in BeamEffect beam ) =>
                {
                    buffer[entityInQueryIndex] = new float3x2{
                        c0 = beam.start ,
                        c1 = beam.end
                    };
                } )
                .WithBurst()
                .ScheduleParallel( JobHandle.CombineDependencies( Dependency , _batch.Dependency ) );
            
            _batch.Dependency = Dependency;
        }
    }
}
