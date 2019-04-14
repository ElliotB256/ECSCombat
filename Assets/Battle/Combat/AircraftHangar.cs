using System;
using Unity.Entities;
using UnityEngine;

namespace Battle.Combat
{
    /// <summary>
    /// An aircraft hangar used to spawn ships
    /// </summary>
    [Serializable]
    public struct AircraftHangar : IComponentData
    {
        public Entity Archetype;
    }
}