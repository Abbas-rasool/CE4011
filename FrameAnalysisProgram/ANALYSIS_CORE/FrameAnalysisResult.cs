using System;
using System.Collections.Generic;
using FrameAnalysisProgram.ANALYSIS_CORE.Validation;
using Matrix_Library.MAIN_TYPES;

namespace FrameAnalysisProgram.ANALYSIS_CORE
{
    /// <summary>
    /// Stores the main results of a 2D frame analysis.
    /// </summary>
    public class FrameAnalysisResult
    {
        public DofMap DofMap { get; }
        public SparseMatrix GlobalStiffnessMatrix { get; }
        public CustomVector GlobalLoadVector { get; }
        public CustomVector GlobalDisplacementVector { get; }

        /// <summary>
        /// Nodal displacement matrix.
        /// Rows   -> node index (Node ID - 1)
        /// Columns -> [Ux, Uy, Rz]
        /// </summary>
        public double[,] NodalDisplacements { get; }

        /// <summary>
        /// Local member end force results.
        /// </summary>
        public List<ElementEndForceResult> ElementEndForces { get; }

        /// <summary>
        /// Support reactions at restrained DOFs, in global coordinates.
        /// </summary>
        public List<NodalReaction> Reactions { get; }

        /// <summary>
        /// Non-fatal validation findings (warnings / informational notes such as
        /// static indeterminacy). Fatal problems are raised as exceptions instead.
        /// </summary>
        public IReadOnlyList<ValidationMessage> ValidationMessages { get; }

        public FrameAnalysisResult(
            DofMap dofMap,
            SparseMatrix globalStiffnessMatrix,
            CustomVector globalLoadVector,
            CustomVector globalDisplacementVector,
            double[,] nodalDisplacements,
            List<ElementEndForceResult> elementEndForces,
            List<NodalReaction> reactions,
            IReadOnlyList<ValidationMessage> validationMessages = null)
        {
            DofMap = dofMap ?? throw new ArgumentNullException(nameof(dofMap));
            GlobalStiffnessMatrix = globalStiffnessMatrix ?? throw new ArgumentNullException(nameof(globalStiffnessMatrix));
            GlobalLoadVector = globalLoadVector ?? throw new ArgumentNullException(nameof(globalLoadVector));
            GlobalDisplacementVector = globalDisplacementVector ?? throw new ArgumentNullException(nameof(globalDisplacementVector));
            NodalDisplacements = nodalDisplacements ?? throw new ArgumentNullException(nameof(nodalDisplacements));
            ElementEndForces = elementEndForces ?? throw new ArgumentNullException(nameof(elementEndForces));
            Reactions = reactions ?? throw new ArgumentNullException(nameof(reactions));
            ValidationMessages = validationMessages ?? new List<ValidationMessage>();
        }
    }
}