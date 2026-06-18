using System;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;
using Matrix_Library.MAIN_TYPES;

namespace FrameAnalysisProgram.ANALYSIS_CORE
{
    /// <summary>
    /// Adds the equivalent nodal loads produced by prescribed support settlements
    /// to the free-DOF load vector, implementing the partitioned relation
    ///
    ///     Kff * Uf = Ff - Kfr * Ur
    ///
    /// The (-Kfr * Ur) term is assembled element-wise: for each element, the
    /// contribution of its settled (restrained) DOFs to its free DOFs is
    /// -Ke[a, b] * Ur_b, taken from the element's global stiffness matrix Ke.
    /// This avoids forming the rectangular Kfr block explicitly.
    /// </summary>
    public class SettlementLoadBuilder
    {
        public void Apply(StructureModel model, DofMap dofMap, CustomVector globalLoadVector)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (dofMap == null)
                throw new ArgumentNullException(nameof(dofMap));

            if (globalLoadVector == null)
                throw new ArgumentNullException(nameof(globalLoadVector));

            double[,] prescribed = DisplacementMapper.BuildPrescribedDisplacements(model);

            foreach (StructuralElement2D element in model.Elements)
            {
                int[] dofIndices = element.GetGlobalDofIndices(dofMap);          // 1-based; 0 = restrained
                (int NodeId, DofType Dof)[] addresses = element.GetDofAddresses();

                bool elementHasSettlement = false;
                double[] localSettlement = new double[dofIndices.Length];
                for (int b = 0; b < dofIndices.Length; b++)
                {
                    if (dofIndices[b] != 0)
                        continue; // free DOF, not a prescribed support value

                    double value = prescribed[addresses[b].NodeId - 1, (int)addresses[b].Dof];
                    if (value != 0.0)
                    {
                        localSettlement[b] = value;
                        elementHasSettlement = true;
                    }
                }

                if (!elementHasSettlement)
                    continue;

                double[,] ke = element.GetGlobalStiffnessMatrix();

                for (int a = 0; a < dofIndices.Length; a++)
                {
                    int equationA = dofIndices[a];
                    if (equationA == 0)
                        continue; // restrained free-side row not part of the solve

                    double delta = 0.0;
                    for (int b = 0; b < dofIndices.Length; b++)
                    {
                        if (localSettlement[b] != 0.0)
                            delta += ke[a, b] * localSettlement[b];
                    }

                    if (delta != 0.0)
                        globalLoadVector.AddToEntry(equationA - 1, -delta);
                }
            }
        }
    }
}
