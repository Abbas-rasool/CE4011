using System;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using Matrix_Library.MAIN_TYPES;

namespace FrameAnalysisProgram.ANALYSIS_CORE
{
    /// <summary>
    /// Builds the global structural load vector for a 2D frame model.
    ///
    /// Purpose:
    /// - Collect nodal loads from the structure model
    /// - Map nodal DOFs to global equation numbers
    /// - Assemble the loads into the custom global vector
    ///
    /// DOF order at each node:
    /// 0 -> Ux
    /// 1 -> Uy
    /// 2 -> Rz
    ///
    /// Assumptions:
    /// - DofMap equation numbers are 1-based for active DOFs
    /// - Equation number 0 means restrained / inactive DOF
    /// - Loads are given in global coordinates
    /// </summary>
    public class LoadVectorBuilder
    {
        /// <summary>
        /// Builds and returns the assembled global load vector.
        /// </summary>
        public CustomVector Build(StructureModel model, DofMap dofMap)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (dofMap == null)
                throw new ArgumentNullException(nameof(dofMap));

            CustomVector globalLoadVector = new CustomVector(dofMap.NumberOfEquations);

            foreach (JointLoad load in model.Loads)
            {
                AssembleJointLoad(globalLoadVector, load, dofMap);
            }

            return globalLoadVector;
        }

        /// <summary>
        /// Assembles one nodal load into the global load vector.
        /// </summary>
        private void AssembleJointLoad(CustomVector globalLoadVector, JointLoad load, DofMap dofMap)
        {
            int nodeId = load.Node.Id;

            int eqUx = dofMap.GetEquation(nodeId, 0);
            int eqUy = dofMap.GetEquation(nodeId, 1);
            int eqRz = dofMap.GetEquation(nodeId, 2);

            if (eqUx != 0)
                globalLoadVector.AddToEntry(eqUx - 1, load.Fx);

            if (eqUy != 0)
                globalLoadVector.AddToEntry(eqUy - 1, load.Fy);

            if (eqRz != 0)
                globalLoadVector.AddToEntry(eqRz - 1, load.Mz);
        }
    }
}