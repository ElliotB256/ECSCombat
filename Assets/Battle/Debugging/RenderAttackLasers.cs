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

namespace Battle.Debug
{
    /// <summary>
    /// Draws lines between 
    /// </summary>
    [UpdateBefore(typeof(CleanUpAttacksSystem)), UpdateAfter(typeof(AttackEntityBufferSystem))]
    public class RenderAttackLasers : ComponentSystem
    {
        protected override void OnCreate()
        {
            //Enabled = false;
        }

        protected override void OnUpdate()
        {
            var pos = GetComponentDataFromEntity<Translation>(true);
            Entities.ForEach(
                (
                    ref Attack attack,
                    ref Instigator attacker,
                    ref Target target
                    ) =>
            {
                if (!pos.Exists(attacker.Value) || !pos.Exists(target.Value))
                    UnityEngine.Debug.Log("Position does not exist.");
                else
                    UnityEngine.Debug.DrawLine(
                        pos[attacker.Value].Value,
                        pos[target.Value].Value,
                        Color.red,
                        0.1f);
            });           
        }
    }
}