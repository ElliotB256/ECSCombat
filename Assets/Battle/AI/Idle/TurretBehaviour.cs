using System;
using Unity.Entities;

namespace Battle.AI
{
    /// <summary>
    /// An entity behaves as a turret, searching for targets and looking at them.
    /// </summary>
    [Serializable]
    public struct TurretBehaviour : IComponentData
    {
        public float Range;
    }
}