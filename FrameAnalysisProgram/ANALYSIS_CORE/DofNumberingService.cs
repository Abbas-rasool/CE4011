using System;
using FrameAnalysisProgram.STRUCTURAL_MODEL;

namespace FrameAnalysisProgram.ANALYSIS_CORE
{
    /// <summary>
    /// Builds the global degree-of-freedom numbering for a 2D frame structure.
    ///
    /// Purpose:
    /// Assign consecutive equation numbers to active DOFs and assign zero
    /// to restrained DOFs.
    ///
    /// DOF order at each node:
    /// 0 -> Ux
    /// 1 -> Uy
    /// 2 -> Rz
    /// </summary>
    public class DofNumberingService
    {
        public DofMap BuildEquationNumbers(StructureModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            int nodeCount = model.Nodes.Count;
            int[,] equationNumbers = new int[nodeCount, 3];

            MarkRestrainedDofs(model, equationNumbers);
            int numberOfEquations = AssignActiveEquationNumbers(equationNumbers);

            return new DofMap(equationNumbers, numberOfEquations);
        }

        private void MarkRestrainedDofs(StructureModel model, int[,] equationNumbers)
        {
            foreach (SupportCondition support in model.Supports)
            {
                int nodeIndex = support.Node.Id - 1;

                if (nodeIndex < 0 || nodeIndex >= model.Nodes.Count)
                    throw new InvalidOperationException(
                        $"Support references invalid node ID {support.Node.Id}.");

                if (support.RestrainsUx)
                    equationNumbers[nodeIndex, 0] = -1;

                if (support.RestrainsUy)
                    equationNumbers[nodeIndex, 1] = -1;

                if (support.RestrainsRz)
                    equationNumbers[nodeIndex, 2] = -1;
            }
        }

        private int AssignActiveEquationNumbers(int[,] equationNumbers)
        {
            int equation = 0;
            int nodeCount = equationNumbers.GetLength(0);

            for (int i = 0; i < nodeCount; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (equationNumbers[i, j] == -1)
                    {
                        equationNumbers[i, j] = 0;
                    }
                    else
                    {
                        equation++;
                        equationNumbers[i, j] = equation;
                    }
                }
            }

            return equation;
        }
    }
}