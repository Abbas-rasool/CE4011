namespace FrameAnalysisProgram.STRUCTURAL_MODEL
{
    /// <summary>
    /// Base interface for all load types.
    /// Each load type implements its own assembly logic.
    /// </summary>
    public interface ILoad
    {
        /// <summary>
        /// Assembles this load into the global load vector.
        /// </summary>
        void AssembleIntoVector(CustomVector globalLoadVector, DofMap dofMap, StructureModel model);
    }
}
