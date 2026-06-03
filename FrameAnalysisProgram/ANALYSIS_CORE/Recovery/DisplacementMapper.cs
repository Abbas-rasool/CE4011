using System;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Geometry;
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
    /// Free DOFs take their solved value; restrained DOFs take their prescribed
    /// support displacement (zero unless a settlement is specified).
    /// </summary>
    public class DisplacementMapper
    {
        public double[,] BuildNodalDisplacementMatrix(
            DofMap dofMap,
            CustomVector globalDisplacementVector,
            StructureModel model)
        {
            if (dofMap == null)
                throw new ArgumentNullException(nameof(dofMap));

            if (globalDisplacementVector == null)
                throw new ArgumentNullException(nameof(globalDisplacementVector));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            int nodeCount = model.Nodes.Count;
            double[,] prescribed = BuildPrescribedDisplacements(model);
            double[,] nodalDisplacements = new double[nodeCount, 3];

            for (int nodeId = 1; nodeId <= nodeCount; nodeId++)
            {
                for (int localDof = 0; localDof < 3; localDof++)
                {
                    int equation = dofMap.GetEquation(nodeId, localDof);

                    nodalDisplacements[nodeId - 1, localDof] = equation == 0
                        ? prescribed[nodeId - 1, localDof]                 // restrained: prescribed settlement (0 if none)
                        : globalDisplacementVector.Get(equation - 1);      // free: solved value
                }
            }

            return nodalDisplacements;
        }

        /// <summary>
        /// Builds a [node, dof] table of prescribed support displacements from the
        /// model's supports. Entries are zero where no settlement is specified.
        /// </summary>
        public static double[,] BuildPrescribedDisplacements(StructureModel model)
        {
            double[,] prescribed = new double[model.Nodes.Count, 3];

            foreach (SupportCondition support in model.Supports)
            {
                int row = support.Node.Id - 1;
                prescribed[row, 0] = support.SettlementUx;
                prescribed[row, 1] = support.SettlementUy;
                prescribed[row, 2] = support.SettlementRz;
            }

            return prescribed;
        }
    }
}
