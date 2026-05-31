using System;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Geometry;

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
            MarkRotationlessNodes(model, equationNumbers);
            int numberOfEquations = AssignActiveEquationNumbers(equationNumbers);

            var dofMap = new DofMap(equationNumbers, numberOfEquations);

            return dofMap;
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

        /// <summary>
        /// Truss elements contribute no rotational stiffness, so a node connected
        /// only to truss elements has a zero-stiffness Rz DOF — which would make the
        /// global matrix singular. Restrain Rz at any node not attached to a frame
        /// element so such DOFs are never numbered.
        /// </summary>
        private void MarkRotationlessNodes(StructureModel model, int[,] equationNumbers)
        {
            int nodeCount = model.Nodes.Count;
            bool[] hasRotationalStiffness = new bool[nodeCount];

            foreach (StructuralElement2D element in model.Elements)
            {
                if (element is FrameElement2D)
                {
                    hasRotationalStiffness[element.StartNode.Id - 1] = true;
                    hasRotationalStiffness[element.EndNode.Id - 1] = true;
                }
            }

            for (int i = 0; i < nodeCount; i++)
            {
                if (!hasRotationalStiffness[i])
                    equationNumbers[i, 2] = -1;   // restrain Rz (no rotational stiffness)
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