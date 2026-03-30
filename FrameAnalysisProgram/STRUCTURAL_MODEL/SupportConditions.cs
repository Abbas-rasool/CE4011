using System;
using System.Collections.Generic;
using System.Text;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL
{
    /// <summary>
    /// Represents support restraints at a node for a 2D frame.
    /// Degrees of freedom: Ux, Uy, Rz.
    /// </summary>
    public class SupportCondition
    {
        /// <summary>
        /// Node where the support is applied.
        /// </summary>
        public Node Node { get; }

        /// <summary>
        /// True if translation in global X is restrained.
        /// </summary>
        public bool RestrainsUx { get; }

        /// <summary>
        /// True if translation in global Y is restrained.
        /// </summary>
        public bool RestrainsUy { get; }

        /// <summary>
        /// True if rotation about global Z is restrained.
        /// </summary>
        public bool RestrainsRz { get; }

        public SupportCondition(Node node, bool restrainsUx, bool restrainsUy, bool restrainsRz)
        {
            Node = node;
            RestrainsUx = restrainsUx;
            RestrainsUy = restrainsUy;
            RestrainsRz = restrainsRz;
        }
    }
}
