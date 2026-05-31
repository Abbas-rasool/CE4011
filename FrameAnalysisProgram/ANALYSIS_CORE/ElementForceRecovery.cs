using System;
using System.Collections.Generic;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Interfaces;
using Matrix_Library.MAIN_TYPES;

namespace FrameAnalysisProgram.ANALYSIS_CORE
{
    /// <summary>
    /// Recovers local element end forces from the solved structural displacements.
    ///
    /// Main steps:
    /// 1. Map global displacement vector to nodal displacement matrix
    /// 2. Extract each element's 6 global displacement components
    /// 3. Transform element displacements to local coordinates
    /// 4. Compute local end forces using the local stiffness matrix
    /// </summary>
    public class ElementForceRecovery
    {
        private readonly DisplacementMapper _displacementMapper;

        public ElementForceRecovery(DisplacementMapper displacementMapper)
        {
            _displacementMapper = displacementMapper ?? throw new ArgumentNullException(nameof(displacementMapper));
        }

        public List<ElementEndForceResult> ComputeEndForces(
            StructureModel model,
            DofMap dofMap,
            CustomVector globalDisplacementVector)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (dofMap == null)
                throw new ArgumentNullException(nameof(dofMap));

            if (globalDisplacementVector == null)
                throw new ArgumentNullException(nameof(globalDisplacementVector));

            double[,] nodalDisplacements = _displacementMapper.BuildNodalDisplacementMatrix(
                dofMap,
                globalDisplacementVector,
                model.Nodes.Count);

            List<ElementEndForceResult> results = new List<ElementEndForceResult>();

            foreach (FrameElement2D element in model.Elements)
            {
                double[] globalElementDisplacements = element.GetGlobalDisplacementVector(nodalDisplacements);

                // Total fully-fixed fixed-end forces from every span load on this member.
                double[] fixedEndForces = SumMemberLoadFixedEndForces(model, element);

                // {Q} = [k]{d} + {Ff_released}
                double[] localEndForces = element.GetLocalEndForceVector(globalElementDisplacements, fixedEndForces);

                ElementEndForceResult result = new ElementEndForceResult(element, localEndForces);
                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Sums the fully-fixed local fixed-end-force vectors of every member load
        /// acting on the given element. Release condensation is applied later by the
        /// element so the same modified vector is used for loads and recovery.
        /// </summary>
        private static double[] SumMemberLoadFixedEndForces(StructureModel model, FrameElement2D element)
        {
            double[] total = new double[6];

            foreach (IMemberLoad memberLoad in model.MemberLoads)
            {
                if (!ReferenceEquals(memberLoad.Element, element))
                    continue;

                double[] f = memberLoad.GetLocalFixedEndForces();
                for (int i = 0; i < 6; i++)
                    total[i] += f[i];
            }

            return total;
        }
    }
}