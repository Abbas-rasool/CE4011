using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Geometry;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Interfaces;
using Matrix_Library.MAIN_TYPES;
using StructuralLoads;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL.Loads
{
    /// <summary>
    /// Represents a nodal load (point load at a joint).
    /// Components: Fx, Fy, Mz in global coordinates.
    /// </summary>
    public class NodalLoad : INodalLoad
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

        /// <summary>
        /// The physical nature of this action (dead, live, wind, …). Defaults to
        /// <see cref="eLoadNature.Dead"/> until the caller specifies it.
        /// </summary>
        public eLoadNature Nature { get; }

        public NodalLoad(Node node, double fx, double fy, double mz, eLoadNature nature = eLoadNature.Dead)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            Fx = fx;
            Fy = fy;
            Mz = mz;
            Nature = nature;
        }

        public void AssembleIntoVector(CustomVector globalLoadVector, DofMap dofMap, StructureModel model)
        {
            if (globalLoadVector == null)
                throw new ArgumentNullException(nameof(globalLoadVector));
            if (dofMap == null)
                throw new ArgumentNullException(nameof(dofMap));

            int nodeId = Node.Id;

            int eqUx = dofMap.GetEquation(nodeId, DofType.Ux);
            int eqUy = dofMap.GetEquation(nodeId, DofType.Uy);
            int eqRz = dofMap.GetEquation(nodeId, DofType.Rz);

            if (eqUx != 0)
                globalLoadVector.AddToEntry(eqUx - 1, Fx);
            if (eqUy != 0)
                globalLoadVector.AddToEntry(eqUy - 1, Fy);
            if (eqRz != 0)
                globalLoadVector.AddToEntry(eqRz - 1, Mz);
        }
    }
}
