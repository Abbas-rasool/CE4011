using System;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Interfaces;
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

            // Nodal loads and member (span) loads both assemble themselves through
            // the common ILoad contract. Member loads contribute equivalent nodal
            // loads derived from their fixed-end forces via the element.
            foreach (INodalLoad load in model.Loads)
            {
                load.AssembleIntoVector(globalLoadVector, dofMap, model);
            }

            foreach (IMemberLoad memberLoad in model.MemberLoads)
            {
                memberLoad.AssembleIntoVector(globalLoadVector, dofMap, model);
            }

            return globalLoadVector;
        }
    }
}