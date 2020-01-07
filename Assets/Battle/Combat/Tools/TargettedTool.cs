using System;
using Unity.Entities;

namespace Battle.Combat.AttackSources
{
    /// <summary>
    /// A targetted tool does a thing to a target when in range and within the cone.
    /// </summary>
    [Serializable]
    public struct TargettedTool : IComponentData
    {
        /// <summary>
        /// Angular width of a cone, in radians, within which the tool may apply to the Target.
        /// </summary>
        public float Cone;

        /// <summary>
        /// Range of this tool.
        /// </summary>
        public float Range;

        /// <summary>
        /// Whether this tool is armed. If true, the tool will fire as soon as possible.
        /// </summary>
        public bool Armed;

        /// <summary>
        /// True when this tool is being discharged in the current frame.
        /// </summary>
        public bool Firing;
    }
}