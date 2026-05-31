using System;
using System.Collections.Generic;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Geometry;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Interfaces;

namespace FrameAnalysisProgram.ANALYSIS_CORE
{
    /// <summary>
    /// Recovers support reactions from the solved element end forces.
    ///
    /// Method (element end-force / equilibrium):
    /// For each node DOF, the reaction equals the sum of the connected elements'
    /// global end forces minus any directly applied joint load:
    ///     R = Σ (R_e^T {Q_local})  −  P_joint
    /// Member (span) loads are already captured inside {Q_local} via fixed-end
    /// forces, so they are not subtracted again. Reactions are reported only at
    /// restrained DOFs; at free DOFs the value is ~0 by equilibrium.
    /// </summary>
    public class ReactionRecovery
    {
        public List<NodalReaction> Compute(
            StructureModel model,
            IReadOnlyList<ElementEndForceResult> elementEndForces)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (elementEndForces == null)
                throw new ArgumentNullException(nameof(elementEndForces));

            int nodeCount = model.Nodes.Count;

            // Accumulate global element end forces at each node DOF: [Ux, Uy, Rz].
            double[,] nodalForces = new double[nodeCount, 3];

            foreach (ElementEndForceResult result in elementEndForces)
            {
                StructuralElement2D element = result.Element;
                double[] globalForces = element.GetGlobalEndForces(result.LocalEndForces);
                (int NodeId, DofType Dof)[] addresses = element.GetDofAddresses();

                for (int i = 0; i < addresses.Length; i++)
                    nodalForces[addresses[i].NodeId - 1, (int)addresses[i].Dof] += globalForces[i];
            }

            // Subtract directly applied nodal loads.
            foreach (INodalLoad load in model.Loads)
            {
                int nodeIndex = load.Node.Id - 1;
                nodalForces[nodeIndex, 0] -= load.Fx;
                nodalForces[nodeIndex, 1] -= load.Fy;
                nodalForces[nodeIndex, 2] -= load.Mz;
            }

            // Report reactions at restrained DOFs only.
            List<NodalReaction> reactions = new List<NodalReaction>();

            foreach (SupportCondition support in model.Supports)
            {
                int nodeIndex = support.Node.Id - 1;

                reactions.Add(new NodalReaction(
                    support.Node.Id,
                    support.RestrainsUx ? nodalForces[nodeIndex, 0] : 0.0,
                    support.RestrainsUy ? nodalForces[nodeIndex, 1] : 0.0,
                    support.RestrainsRz ? nodalForces[nodeIndex, 2] : 0.0,
                    support.RestrainsUx,
                    support.RestrainsUy,
                    support.RestrainsRz));
            }

            return reactions;
        }
    }
}
