using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

using Battle.Combat;

namespace Battle.Debugging
{
    /// <summary>
    /// Draws lines between 
    /// </summary>
    [UpdateBefore(typeof(CleanUpAttacksSystem)), UpdateAfter(typeof(AttackEntityBufferSystem))]
    public class DebugDrawAttackLasers : ComponentSystem
    {
        protected override void OnCreate()
        {
            //Enabled = false;
        }

        protected override void OnUpdate()
        {
            var pos = GetComponentDataFromEntity<LocalToWorld>(true);
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
                        pos[attacker.Value].Position,
                        pos[target.Value].Position,
                        Color.red,
                        0.1f);
            });           
        }
    }
}