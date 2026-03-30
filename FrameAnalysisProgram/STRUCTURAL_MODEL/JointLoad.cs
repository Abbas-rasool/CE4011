using System;
using System.Collections.Generic;
using System.Text;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL
{
    /// <summary>
    /// Represents a nodal load applied at a node in global coordinates.
    /// Components: Fx, Fy, Mz.
    /// </summary>
    public class JointLoad
    {
        /// <summary>
        /// Node where the load is applied.
        /// </summary>
        public Node Node { get; }

        /// <summary>
        /// Force component in the global X direction.
        /// Units: force
        /// </summary>
        public double Fx { get; }

        /// <summary>
        /// Force component in the global Y direction.
        /// Units: force
        /// </summary>
        public double Fy { get; }

        /// <summary>
        /// Moment component about the global Z axis.
        /// Units: force * length
        /// </summary>
        public double Mz { get; }

        public JointLoad(Node node, double fx, double fy, double mz)
        {
            Node = node;
            Fx = fx;
            Fy = fy;
            Mz = mz;
        }
    }
}
