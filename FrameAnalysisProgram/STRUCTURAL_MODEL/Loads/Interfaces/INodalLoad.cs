using FrameAnalysisProgram.STRUCTURAL_MODEL.Geometry;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Interfaces
{
    /// <summary>
    /// Interface for loads applied at nodes (joints).
    /// </summary>
    public interface INodalLoad : ILoad
    {
        /// <summary>
        /// The node where the load is applied.
        /// </summary>
        Node Node { get; }

        /// <summary>
        /// Global force component in X. Units: force.
        /// </summary>
        double Fx { get; }

        /// <summary>
        /// Global force component in Y. Units: force.
        /// </summary>
        double Fy { get; }

        /// <summary>
        /// Global moment about Z. Units: force * length.
        /// </summary>
        double Mz { get; }
    }
}
