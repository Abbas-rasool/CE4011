using System;
using System.Collections.Generic;
using System.Text;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL.Geometry
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

        /// <summary>
        /// Prescribed support displacement (settlement) in global X.
        /// Only meaningful when the matching DOF is restrained. Units: length.
        /// </summary>
        public double SettlementUx { get; }

        /// <summary>
        /// Prescribed support displacement (settlement) in global Y.
        /// Only meaningful when the matching DOF is restrained. Units: length.
        /// </summary>
        public double SettlementUy { get; }

        /// <summary>
        /// Prescribed support rotation about global Z.
        /// Only meaningful when the matching DOF is restrained. Units: radians.
        /// </summary>
        public double SettlementRz { get; }

        public SupportCondition(
            Node node,
            bool restrainsUx,
            bool restrainsUy,
            bool restrainsRz,
            double settlementUx = 0.0,
            double settlementUy = 0.0,
            double settlementRz = 0.0)
        {
            Node = node;
            RestrainsUx = restrainsUx;
            RestrainsUy = restrainsUy;
            RestrainsRz = restrainsRz;
            SettlementUx = settlementUx;
            SettlementUy = settlementUy;
            SettlementRz = settlementRz;
        }
    }
}
