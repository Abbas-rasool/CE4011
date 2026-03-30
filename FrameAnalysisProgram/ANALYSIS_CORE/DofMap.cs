using System;
using FrameAnalysisProgram.STRUCTURAL_MODEL;

namespace FrameAnalysisProgram.ANALYSIS_CORE
{
    /// <summary>
    /// Stores the global equation numbering of the active degrees of freedom
    /// for a 2D frame structure.
    ///
    /// Degrees of freedom per node:
    /// 0 -> Ux
    /// 1 -> Uy
    /// 2 -> Rz
    ///
    /// Convention:
    /// - 0 means restrained / inactive degree of freedom
    /// - positive integer means active equation number
    /// </summary>
    public class DofMap
    {
        /// <summary>
        /// Equation number table.
        /// Rows correspond to node indices (Node ID - 1).
        /// Columns correspond to local nodal DOFs: [Ux, Uy, Rz].
        /// </summary>
        public int[,] EquationNumbers { get; }

        /// <summary>
        /// Total number of active equations in the structure.
        /// </summary>
        public int NumberOfEquations { get; }

        public DofMap(int[,] equationNumbers, int numberOfEquations)
        {
            EquationNumbers = equationNumbers ?? throw new ArgumentNullException(nameof(equationNumbers));
            NumberOfEquations = numberOfEquations;
        }

        /// <summary>
        /// Returns the global equation number for a given node ID and local DOF index.
        /// Local DOF: 0 = Ux, 1 = Uy, 2 = Rz
        /// </summary>
        public int GetEquation(int nodeId, int localDof)
        {
            if (nodeId < 1 || nodeId > EquationNumbers.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(nodeId), "Node ID is out of range.");

            if (localDof < 0 || localDof > 2)
                throw new ArgumentOutOfRangeException(nameof(localDof), "Local DOF must be 0, 1, or 2.");

            return EquationNumbers[nodeId - 1, localDof];
        }

        /// <summary>
        /// Builds the 6-entry structural DOF map for a frame element:
        /// [start Ux, start Uy, start Rz, end Ux, end Uy, end Rz]
        /// </summary>
        public int[] GetElementDofMap(FrameElement2D element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return new int[]
            {
                GetEquation(element.StartNode.Id, 0),
                GetEquation(element.StartNode.Id, 1),
                GetEquation(element.StartNode.Id, 2),
                GetEquation(element.EndNode.Id, 0),
                GetEquation(element.EndNode.Id, 1),
                GetEquation(element.EndNode.Id, 2)
            };
        }
    }
}