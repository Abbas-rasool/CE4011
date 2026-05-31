using System;
using Matrix_Library.MAIN_TYPES;

namespace FrameAnalysisProgram.ANALYSIS_CORE
{
    /// <summary>
    /// Maps the solved global displacement vector back to nodal displacement form.
    ///
    /// Output format:
    /// Rows   -> node index (Node ID - 1)
    /// Columns:
    ///   0 -> Ux
    ///   1 -> Uy
    ///   2 -> Rz
    ///
    /// Restrained DOFs are assigned zero.
    /// </summary>
    public class DisplacementMapper
    {
        public double[,] BuildNodalDisplacementMatrix(DofMap dofMap, CustomVector globalDisplacementVector, int nodeCount)
        {
            if (dofMap == null)
                throw new ArgumentNullException(nameof(dofMap));

            if (globalDisplacementVector == null)
                throw new ArgumentNullException(nameof(globalDisplacementVector));

            if (nodeCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(nodeCount), "Node count must be positive.");

            double[,] nodalDisplacements = new double[nodeCount, 3];

            for (int nodeId = 1; nodeId <= nodeCount; nodeId++)
            {
                for (int localDof = 0; localDof < 3; localDof++)
                {
                    int equation = dofMap.GetEquation(nodeId, localDof);

                    if (equation == 0)
                    {
                        nodalDisplacements[nodeId - 1, localDof] = 0.0;
                    }
                    else
                    {
                        nodalDisplacements[nodeId - 1, localDof] = globalDisplacementVector.Get(equation - 1);
                    }
                }
            }

            return nodalDisplacements;
        }
    }
}