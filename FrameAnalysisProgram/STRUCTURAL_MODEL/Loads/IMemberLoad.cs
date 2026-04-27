namespace FrameAnalysisProgram.STRUCTURAL_MODEL
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
    }
}
