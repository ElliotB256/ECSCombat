using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

using Battle.Combat;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Rendering;
using Battle.Movement;
using Unity.Mathematics;
using UnityEditor;

namespace Battle.Effects
{
    /// <summary>
    /// Spawns effects of shields being struck.
    /// </summary>
    [
        UpdateAfter(typeof(PostAttackEntityBuffer)),
        //UpdateBefore(typeof(LateSimulationSystemGroup))
    ]
    public class SpawnShieldEffectSystem : ComponentSystem
    {
        EndSimulationEntityCommandBufferSystem BufferSystem;
        Material ShieldMaterial;
        Mesh Mesh;

        protected override void OnCreate()
        {
            BufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            ShieldMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/Art/Effects/Shields/ShieldMaterial.mat", typeof(Material));
            Mesh = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/Art/Misc/ScalePlane.fbx", typeof(Mesh));

            if (Mesh == null)
                throw new System.Exception("Could not load mesh.");
            if (ShieldMaterial == null)
                throw new System.Exception("Could not load shield material.");
        }

        protected override void OnUpdate()
        {
            var Buffer = BufferSystem.CreateCommandBuffer();
            Entities.ForEach(
                (ref ShieldHitEffect effect, ref LocalToWorld localToWorld, ref Shield shield) =>
                {
                    var e = Buffer.CreateEntity();
                    Buffer.AddComponent(e, new Lifetime { Value = 0.3f });
                    Buffer.AddComponent(e, new Translation { Value = localToWorld.Position });
                    Buffer.AddComponent(e, new Rotation { Value = quaternion.LookRotation(effect.HitDirection, new float3(0.0f, 1.0f, 0.0f)) });
                    Buffer.AddComponent(e, new Scale { Value = shield.Radius });
                    Buffer.AddComponent(e, new LocalToWorld { });
                    Buffer.AddSharedComponent(e,
                    new RenderMesh
                    {
                        castShadows = UnityEngine.Rendering.ShadowCastingMode.Off,
                        receiveShadows = false,
                        mesh = Mesh,
                        material = ShieldMaterial
                    });
                }
                );
        }
    }
}