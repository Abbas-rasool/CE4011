using System;
using System.Collections.Generic;
using System.Text;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using Matrix_Library.MAIN_TYPES;

namespace FrameAnalysisProgram.ANALYSIS_CORE
{
    /// <summary>
    /// Assembles the global structural stiffness matrix for a 2D frame model.
    ///
    /// Purpose:
    /// - Collect element global stiffness contributions
    /// - Map local element DOFs to global structural equation numbers
    /// - Store the assembled matrix in the custom sparse matrix format
    ///
    /// Assumptions:
    /// - Element global stiffness matrices are 6x6
    /// - DofMap equation numbers are 1-based for active DOFs
    /// - Equation number 0 means restrained / inactive DOF
    /// - Global stiffness matrix is symmetric
    /// </summary>
    public class GlobalStiffnessAssembler
    {
        /// <summary>
        /// Builds and returns the assembled global stiffness matrix.
        /// </summary>
        public SparseMatrix Assemble(StructureModel model, DofMap dofMap)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (dofMap == null)
                throw new ArgumentNullException(nameof(dofMap));

            SparseMatrix globalMatrix = new SparseMatrix(dofMap.NumberOfEquations);

            foreach (FrameElement2D element in model.Elements)
            {
                AssembleElement(globalMatrix, element, dofMap);
            }

            return globalMatrix;
        }


        private void AssembleElement(SparseMatrix globalMatrix, FrameElement2D element, DofMap dofMap)
        {
            double[,] elementGlobalStiffness = element.GetGlobalStiffnessMatrix();
            int[] elementDofMap = dofMap.GetElementDofMap(element);

            for (int p = 0; p < 6; p++)
            {
                int globalP = elementDofMap[p];

                if (globalP == 0)
                    continue;

                for (int q = 0; q <= p; q++)
                {
                    int globalQ = elementDofMap[q];

                    if (globalQ == 0)
                        continue;

                    double value = elementGlobalStiffness[p, q];

                    if (Math.Abs(value) < 1e-14)
                        continue;

                    globalMatrix.AddToEntrySymmetric(globalP - 1, globalQ - 1, value);
                }
            }
        }
    }
}

