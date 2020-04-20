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
using Unity.Rendering;

namespace Battle.Effects
{
    [
    AlwaysUpdateSystem,
    UpdateAfter(typeof(BeamEffectSystem)),
    UpdateInGroup(typeof(AttackResultSystemsGroup))
    ]
    public class RenderLaserSystem : JobComponentSystem
    {
        private EntityQuery m_query;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_query = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<BeamEffect>(),
                    ComponentType.ReadOnly<BeamEffectStyle>()
                }
            }
            );
        }

        private EntityQuery BeamQuery;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            int numberOfBeams = BeamQuery.CalculateEntityCount();
            var Vertices = new NativeArray<float3>(numberOfBeams * 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var Normals = new NativeArray<float3>(numberOfBeams * 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var UVs = new NativeArray<float2>(numberOfBeams * 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var Triangles = new NativeArray<int>(numberOfBeams * 6, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            Entities
                .WithStoreEntityQueryInField(ref BeamQuery)
                .ForEach(
                (Entity e, int entityInQueryIndex, in BeamEffect beam, in BeamEffectStyle style) =>
                {
                    float3 transverseDir = math.normalize(math.cross((beam.end - beam.start), new float3(0.0f, 1.0f, 0.0f)));
                    float distance = math.length(beam.end - beam.start);

                    int number = 4; //number of vertices per beam. 
                    int vertexStartID = entityInQueryIndex * number;
                    float halfWidth = style.Width / 2.0f;
                    Vertices[vertexStartID + 0] = beam.start - transverseDir * halfWidth;
                    Vertices[vertexStartID + 1] = beam.start + transverseDir * halfWidth;
                    Vertices[vertexStartID + 2] = beam.end + transverseDir * halfWidth;
                    Vertices[vertexStartID + 3] = beam.end - transverseDir * halfWidth;

                    UVs[vertexStartID + 0] = new float2(0.0f, 0.0f);
                    UVs[vertexStartID + 1] = new float2(1.0f, 0.0f);
                    UVs[vertexStartID + 2] = new float2(1.0f, distance);
                    UVs[vertexStartID + 3] = new float2(0.0f, distance);

                    Normals[vertexStartID + 0] = new float3(0.0f, 1.0f, 0.0f);
                    Normals[vertexStartID + 1] = new float3(0.0f, 1.0f, 0.0f);
                    Normals[vertexStartID + 2] = new float3(0.0f, 1.0f, 0.0f);
                    Normals[vertexStartID + 3] = new float3(0.0f, 1.0f, 0.0f);

                    int triangleStartID = entityInQueryIndex * 6;
                    Triangles[triangleStartID + 0] = vertexStartID + 0;
                    Triangles[triangleStartID + 1] = vertexStartID + 1;
                    Triangles[triangleStartID + 2] = vertexStartID + 2;
                    Triangles[triangleStartID + 3] = vertexStartID + 0;
                    Triangles[triangleStartID + 4] = vertexStartID + 2;
                    Triangles[triangleStartID + 5] = vertexStartID + 3;
                }
                )
                .Run();

            Entities.ForEach(
                (RenderMesh mesh, ref LaserRenderer renderer) => 
            {
                mesh.mesh.Clear();
                mesh.mesh.SetVertices(Vertices);
                mesh.mesh.SetUVs(0, UVs);
                mesh.mesh.SetTriangles(Triangles.ToArray(), 0);
                mesh.mesh.SetNormals(Normals);
                mesh.mesh.bounds = new Bounds(Vector3.zero, new Vector3(1f, 1f, 1f) * 10000f);
            }
            ).WithoutBurst().Run();
            Vertices.Dispose();
            Normals.Dispose();
            UVs.Dispose();
            Triangles.Dispose();

            return inputDeps;
        }
    }
}