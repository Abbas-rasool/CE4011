using FrameAnalysisProgram.ANALYSIS_CORE;
using Matrix_Library.MAIN_TYPES;
using StructuralLoads;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Interfaces
{
    /// <summary>
    /// Base interface for all load types.
    /// Each load type implements its own assembly logic.
    /// </summary>
    public interface ILoad
    {
        /// <summary>
        /// The physical nature of this action (dead, live, wind, thermal, …). Used downstream to
        /// build the set of present loads a design code's combinations are generated from.
        /// </summary>
        eLoadNature Nature { get; }

        /// <summary>
        /// Assembles this load into the global load vector.
        /// </summary>
        void AssembleIntoVector(CustomVector globalLoadVector, DofMap dofMap, StructureModel model);
    }
}
