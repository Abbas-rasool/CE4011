using System;
using FrameAnalysisProgram.ANALYSIS_CORE;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL
{
    public enum MomentRelease
    {
        None,    // fully rigid both ends (default)
        Start,   // hinge at start node
        End,     // hinge at end node
        Both     // pinned both ends — axial only
    }

    /// <summary>
    /// 2D frame element with 3 DOFs per node: Ux, Uy, Rz.
    /// Local DOF order: [u1, v1, r1, u2, v2, r2]
    ///
    /// Assumptions:
    /// - Linear elastic, small displacements
    /// - Prismatic member, Euler-Bernoulli beam theory
    /// </summary>
    public class FrameElement2D : StructuralElement2D
    {
        public MomentRelease Release { get; }

        public FrameElement2D(
            int id,
            Node startNode,
            Node endNode,
            Material material,
            SectionProperty section,
            MomentRelease release = MomentRelease.None)   // default keeps existing behaviour
            : base(id, startNode, endNode, material, section)
        {
            Release = release;
        }

        public override double[,] GetLocalStiffnessMatrix()
        {
            double L = Length;
            if (L <= 0.0)
                throw new InvalidOperationException($"Element {Id} has zero length.");

            double E = Material.ElasticModulus;
            double A = Section.Area;
            double I = Section.MomentOfInertia;
            double eaL = E * A / L;

            double[,] k = new double[6, 6];

            // Axial — same for all release cases
            k[0, 0] = eaL; k[0, 3] = -eaL;
            k[3, 0] = -eaL; k[3, 3] = eaL;

            // Bending — depends on release condition
            switch (Release)
            {
                case MomentRelease.None:
                    ApplyFullBending(k, E, I, L);
                    break;

                case MomentRelease.Start:
                    ApplyStartHingeBending(k, E, I, L);
                    break;

                case MomentRelease.End:
                    ApplyEndHingeBending(k, E, I, L);
                    break;

                case MomentRelease.Both:
                    // Both ends pinned: no bending stiffness, axial only
                    break;
            }

            return k;
        }

        // -------------------------------------------------------------------------
        // Bending stiffness helpers — each derived via static condensation
        // -------------------------------------------------------------------------

        /// <summary>
        /// Standard Euler-Bernoulli bending stiffness, no releases.
        /// DOFs: [v1, r1, v2, r2] → indices [1, 2, 4, 5]
        /// </summary>
        private static void ApplyFullBending(double[,] k, double E, double I, double L)
        {
            double ei = E * I;
            double ei12_L3 = 12.0 * ei / (L * L * L);
            double ei6_L2 = 6.0 * ei / (L * L);
            double ei4_L = 4.0 * ei / L;
            double ei2_L = 2.0 * ei / L;

            k[1, 1] = ei12_L3; k[1, 2] = ei6_L2; k[1, 4] = -ei12_L3; k[1, 5] = ei6_L2;
            k[2, 1] = ei6_L2; k[2, 2] = ei4_L; k[2, 4] = -ei6_L2; k[2, 5] = ei2_L;
            k[4, 1] = -ei12_L3; k[4, 2] = -ei6_L2; k[4, 4] = ei12_L3; k[4, 5] = -ei6_L2;
            k[5, 1] = ei6_L2; k[5, 2] = ei2_L; k[5, 4] = -ei6_L2; k[5, 5] = ei4_L;
        }

        /// <summary>
        /// Condensed bending stiffness with hinge at start node (r1 released).
        /// r1 row and column remain zero.
        /// Active bending DOFs: [v1, v2, r2] → indices [1, 4, 5]
        /// </summary>
        private static void ApplyStartHingeBending(double[,] k, double E, double I, double L)
        {
            double ei = E * I;
            double ei3_L3 = 3.0 * ei / (L * L * L);
            double ei3_L2 = 3.0 * ei / (L * L);
            double ei3_L = 3.0 * ei / L;

            k[1, 1] = ei3_L3; k[1, 4] = -ei3_L3; k[1, 5] = ei3_L2;
            k[4, 1] = -ei3_L3; k[4, 4] = ei3_L3; k[4, 5] = -ei3_L2;
            k[5, 1] = ei3_L2; k[5, 4] = -ei3_L2; k[5, 5] = ei3_L;
            // k[2, *] and k[*, 2] remain zero — r1 is released
        }

        /// <summary>
        /// Condensed bending stiffness with hinge at end node (r2 released).
        /// r2 row and column remain zero.
        /// Active bending DOFs: [v1, r1, v2] → indices [1, 2, 4]
        /// </summary>
        private static void ApplyEndHingeBending(double[,] k, double E, double I, double L)
        {
            double ei = E * I;
            double ei3_L3 = 3.0 * ei / (L * L * L);
            double ei3_L2 = 3.0 * ei / (L * L);
            double ei3_L = 3.0 * ei / L;

            k[1, 1] = ei3_L3; k[1, 2] = ei3_L2; k[1, 4] = -ei3_L3;
            k[2, 1] = ei3_L2; k[2, 2] = ei3_L; k[2, 4] = -ei3_L2;
            k[4, 1] = -ei3_L3; k[4, 2] = -ei3_L2; k[4, 4] = ei3_L3;
            // k[5, *] and k[*, 5] remain zero — r2 is released
        }

        /// <summary>
        /// Full 6x6 local stiffness with no releases (both ends rigid).
        /// Used as the basis for statically condensing fixed-end forces when
        /// the member has moment releases.
        /// </summary>
        private double[,] GetFullyFixedLocalStiffnessMatrix()
        {
            double L = Length;
            if (L <= 0.0)
                throw new InvalidOperationException($"Element {Id} has zero length.");

            double E = Material.ElasticModulus;
            double A = Section.Area;
            double I = Section.MomentOfInertia;
            double eaL = E * A / L;

            double[,] k = new double[6, 6];
            k[0, 0] = eaL; k[0, 3] = -eaL;
            k[3, 0] = -eaL; k[3, 3] = eaL;
            ApplyFullBending(k, E, I, L);
            return k;
        }


        // -------------------------------------------------------------------------
        // Frame-specific: 6x6 rotation matrix
        // -------------------------------------------------------------------------

        /// <summary>
        /// Forms the 6x6 rotation matrix.
        /// Relation: {d_local} = [R] {d_global}
        /// </summary>
        public double[,] GetRotationMatrix()
        {
            double c = CosX;
            double s = SinX;

            double[,] R = new double[6, 6];

            R[0, 0] = c; R[0, 1] = s;
            R[1, 0] = -s; R[1, 1] = c;
            R[2, 2] = 1.0;

            R[3, 3] = c; R[3, 4] = s;
            R[4, 3] = -s; R[4, 4] = c;
            R[5, 5] = 1.0;

            return R;
        }


        public override double[,] GetGlobalStiffnessMatrix()
        {
            double[,] R = GetRotationMatrix();
            double[,] RT = Transpose(R);
            double[,] kL = GetLocalStiffnessMatrix();

            return MultiplyMatrixMatrix(MultiplyMatrixMatrix(RT, kL), R);
        }

        public override int[] GetGlobalDofIndices(DofMap dofMap)
        {
            if (dofMap == null)
                throw new ArgumentNullException(nameof(dofMap));

            return new[]
            {
                dofMap.GetEquation(StartNode.Id, DofType.Ux),
                dofMap.GetEquation(StartNode.Id, DofType.Uy),
                dofMap.GetEquation(StartNode.Id, DofType.Rz),
                dofMap.GetEquation(EndNode.Id,   DofType.Ux),
                dofMap.GetEquation(EndNode.Id,   DofType.Uy),
                dofMap.GetEquation(EndNode.Id,   DofType.Rz)
            };
        }

        public override double[] GetLocalDisplacementVector(double[] globalDisplacements)
        {
            if (globalDisplacements == null)
                throw new ArgumentNullException(nameof(globalDisplacements));

            if (globalDisplacements.Length != 6)
                throw new ArgumentException("Frame element global displacement vector must have length 6.");

            return MultiplyMatrixVector(GetRotationMatrix(), globalDisplacements);
        }

        /// <summary>
        /// Extracts this element's 6 global displacement components from the
        /// nodal displacement matrix (rows = Node ID - 1; columns = [Ux, Uy, Rz]).
        /// Restrained DOFs are already zeroed by the displacement mapper.
        /// </summary>
        public double[] GetGlobalDisplacementVector(double[,] nodalDisplacements)
        {
            if (nodalDisplacements == null)
                throw new ArgumentNullException(nameof(nodalDisplacements));

            if (nodalDisplacements.GetLength(1) != 3)
                throw new ArgumentException("Nodal displacement matrix must have 3 columns: [Ux, Uy, Rz].");

            int s = StartNode.Id - 1;
            int e = EndNode.Id - 1;

            return new double[]
            {
                nodalDisplacements[s, 0], nodalDisplacements[s, 1], nodalDisplacements[s, 2],
                nodalDisplacements[e, 0], nodalDisplacements[e, 1], nodalDisplacements[e, 2]
            };
        }

        // -------------------------------------------------------------------------
        // Fixed-end forces / equivalent nodal loads (member load handling)
        //
        // Sign convention (local DOF order [u1, v1, r1, u2, v2, r2]):
        //   Member end forces  {Q} = [k_local]{d_local} + {Ff}
        //   Equivalent joint loads added to the structure = -[R]^T {Ff}
        // where {Ff} is the fully-fixed fixed-end force vector supplied by the load.
        // -------------------------------------------------------------------------

        /// <summary>
        /// The local DOF indices of the released rotations for this member,
        /// in the order they should be condensed out.
        /// </summary>
        private int[] ReleasedRotationalDofs()
        {
            switch (Release)
            {
                case MomentRelease.Start: return new[] { 2 };
                case MomentRelease.End: return new[] { 5 };
                case MomentRelease.Both: return new[] { 2, 5 };
                default: return Array.Empty<int>();
            }
        }

        /// <summary>
        /// Statically condenses a fully-fixed local fixed-end-force vector to
        /// account for this member's moment releases. The released rotational
        /// DOF carries zero moment; its restraint is redistributed to the
        /// remaining DOFs (Gaussian elimination of the released DOF).
        /// </summary>
        public double[] ApplyReleasesToLocalForces(double[] fixedEndForces)
        {
            if (fixedEndForces == null)
                throw new ArgumentNullException(nameof(fixedEndForces));
            if (fixedEndForces.Length != 6)
                throw new ArgumentException("Local fixed-end force vector must have length 6.");

            double[] f = (double[])fixedEndForces.Clone();

            int[] released = ReleasedRotationalDofs();
            if (released.Length == 0)
                return f;

            double[,] k = GetFullyFixedLocalStiffnessMatrix();

            foreach (int j in released)
            {
                double kjj = k[j, j];
                if (Math.Abs(kjj) < 1e-30)
                    continue;

                // Condense the load vector: f_i -= k_ij * f_j / k_jj
                double fj = f[j];
                for (int i = 0; i < 6; i++)
                    f[i] -= k[i, j] * fj / kjj;
                f[j] = 0.0;

                // Condense the stiffness so a subsequent release sees the
                // updated (reduced) member, then drop row/column j.
                double[,] kNext = new double[6, 6];
                for (int r = 0; r < 6; r++)
                    for (int c = 0; c < 6; c++)
                        kNext[r, c] = k[r, c] - k[r, j] * k[j, c] / kjj;
                for (int t = 0; t < 6; t++)
                {
                    kNext[j, t] = 0.0;
                    kNext[t, j] = 0.0;
                }
                k = kNext;
            }

            return f;
        }

        /// <summary>
        /// Converts a fully-fixed local fixed-end-force vector into the
        /// equivalent global nodal load vector to add to the structural load
        /// vector. Applies any moment releases, transforms to global, and negates
        /// (equivalent joint loads are the negative of the fixed-end forces).
        /// Order matches <see cref="GetGlobalDofIndices"/>.
        /// </summary>
        public double[] GetEquivalentGlobalNodalLoads(double[] localFixedEndForces)
        {
            double[] released = ApplyReleasesToLocalForces(localFixedEndForces);

            double[,] RT = Transpose(GetRotationMatrix());
            double[] global = MultiplyMatrixVector(RT, released);

            for (int i = 0; i < global.Length; i++)
                global[i] = -global[i];

            return global;
        }

        /// <summary>
        /// Local end forces including the effect of member (span) loads:
        /// {Q} = [k_local]{d_local} + {Ff_released}.
        /// Pass the summed fully-fixed local fixed-end forces of every member
        /// load acting on this element.
        /// </summary>
        public double[] GetLocalEndForceVector(double[] globalDisplacements, double[] localFixedEndForces)
        {
            double[] q = GetLocalEndForceVector(globalDisplacements);   // [k]{d}
            double[] released = ApplyReleasesToLocalForces(localFixedEndForces);

            for (int i = 0; i < 6; i++)
                q[i] += released[i];

            return q;
        }
    }
}
