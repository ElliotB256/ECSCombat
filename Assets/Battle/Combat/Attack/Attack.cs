using System;
using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// An attack made by one entity against another.
    /// </summary>
    [Serializable]
    public struct Attack : IComponentData
    {
        public float Accuracy;

        public enum eResult : byte
        {
            Hit,
            Miss
        }

        public eResult Result;

        public static Attack New(float Accuracy)
        {
            return new Attack { Accuracy = Accuracy, Result = eResult.Hit };
        }
    }
}