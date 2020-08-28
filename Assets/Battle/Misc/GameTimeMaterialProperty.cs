using System;
using Unity.Entities;
using Unity.Rendering;

namespace Battle.Combat
{
    [GenerateAuthoringComponent]
    [Serializable]
    [MaterialProperty("GameTime", MaterialPropertyFormat.Float)]
    public struct GameTimeMaterialProperty : IComponentData
    {
        public float Value;
    }
}