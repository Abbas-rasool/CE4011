using System;
using System.Collections.Generic;
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

        public FrameAnalysisResult(
            DofMap dofMap,
            SparseMatrix globalStiffnessMatrix,
            CustomVector globalLoadVector,
            CustomVector globalDisplacementVector,
            double[,] nodalDisplacements,
            List<ElementEndForceResult> elementEndForces)
        {
            DofMap = dofMap ?? throw new ArgumentNullException(nameof(dofMap));
            GlobalStiffnessMatrix = globalStiffnessMatrix ?? throw new ArgumentNullException(nameof(globalStiffnessMatrix));
            GlobalLoadVector = globalLoadVector ?? throw new ArgumentNullException(nameof(globalLoadVector));
            GlobalDisplacementVector = globalDisplacementVector ?? throw new ArgumentNullException(nameof(globalDisplacementVector));
            NodalDisplacements = nodalDisplacements ?? throw new ArgumentNullException(nameof(nodalDisplacements));
            ElementEndForces = elementEndForces ?? throw new ArgumentNullException(nameof(elementEndForces));
        }
    }
}