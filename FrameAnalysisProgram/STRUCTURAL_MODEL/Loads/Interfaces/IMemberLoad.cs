using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Interfaces
{
    /// <summary>
    /// Interface for loads applied on members (elements).
    /// Each member load knows its element and converts to equivalent nodal loads.
    /// </summary>
    public interface IMemberLoad : ILoad
    {
        /// <summary>
        /// The member (element) where the load is applied.
        /// </summary>
        FrameElement2D Element { get; }

        /// <summary>
        /// The fully-fixed (both ends rigid) fixed-end-force vector for this load,
        /// in the member's local coordinates and DOF order [N1, V1, M1, N2, V2, M2].
        /// Moment releases and the local-to-global transform are applied by the
        /// element; this returns the load's contribution as if both ends were fixed.
        /// </summary>
        double[] GetLocalFixedEndForces();
    }
}
