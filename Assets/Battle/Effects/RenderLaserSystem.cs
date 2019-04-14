﻿//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Transforms;
//using UnityEngine;
//using Unity;

//using Battle.Combat;
//using System.Collections.Generic;

//namespace Battle.Effects
//{
//    /// <summary>
//    /// Render laser beam effects
//    /// </summary>
//    [UpdateBefore(typeof(CleanUpAttacksSystem)), UpdateAfter(typeof(AttackEntityBufferSystem))]
//    public class RenderAttackLaserSystem : ComponentSystem
//    {
//        protected override void OnCreateManager()
//        {
//            m_entityQuery = EntityManager.WithAll<LaserBeamEffect>().ToEntityQuery();
//            base.OnCreateManager();
//        }

//        protected override void OnUpdate()
//        {
//            var pos = GetComponentDataFromEntity<LocalToWorld>(true);
//            Entities.ForEach(
//                (
//                    ref Attack attack,
//                    ref Instigator attacker,
//                    ref Target target
//                    ) =>
//            {
//                if (!pos.Exists(attacker.Value) || !pos.Exists(target.Value))
//                    UnityEngine.Debug.Log("Position does not exist.");
//                else
//                    UnityEngine.Debug.DrawLine(
//                        pos[attacker.Value].Position,
//                        pos[target.Value].Position,
//                        Color.red,
//                        0.1f);
//            });
//        }
//    }
//}