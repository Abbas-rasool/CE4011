namespace FrameAnalysisProgram.STRUCTURAL_MODEL
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
    }
}
