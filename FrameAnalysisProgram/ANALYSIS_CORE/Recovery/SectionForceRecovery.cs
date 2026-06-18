using System;
using System.Collections.Generic;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Members;

namespace FrameAnalysisProgram.ANALYSIS_CORE
{
    /// <summary>
    /// Recovers the distribution of section forces (N, V, M) and the deflected shape along
    /// each member, sampled at station points. Sits in the recovery pipeline alongside
    /// <see cref="ElementForceRecovery"/> and <see cref="ReactionRecovery"/>; its output is
    /// the single source of truth consumed by both the diagram/deflected-shape renderer and
    /// (via the envelope) the design demands.
    ///
    /// Section forces come from statics on the free body from the start node to the cut,
    /// using the recovered member-end forces (which already include the span-load fixed-end
    /// effects) plus the actual span loads up to the cut. The deflected shape uses the
    /// Hermite cubic interpolation of the member's nodal displacements and rotations.
    /// </summary>
    public class SectionForceRecovery
    {
        private readonly int _subdivisionsPerSpan;

        /// <param name="subdivisionsPerSpan">
        /// Number of intervals each smooth span (between the ends and any point loads) is
        /// divided into for drawing. Cosmetic only — the shear-zero point and point-load
        /// steps are always sampled, so the extrema are exact regardless.
        /// </param>
        public SectionForceRecovery(int subdivisionsPerSpan = 8)
        {
            _subdivisionsPerSpan = Math.Max(1, subdivisionsPerSpan);
        }

        public List<MemberStationResult> ComputeStations(
            StructureModel model,
            double[,] nodalDisplacements,
            IReadOnlyList<ElementEndForceResult> endForces)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (nodalDisplacements == null) throw new ArgumentNullException(nameof(nodalDisplacements));
            if (endForces == null) throw new ArgumentNullException(nameof(endForces));

            var byElementId = new Dictionary<int, double[]>(endForces.Count);
            foreach (ElementEndForceResult ef in endForces)
                byElementId[ef.Element.Id] = ef.LocalEndForces;

            var results = new List<MemberStationResult>(model.Elements.Count);
            foreach (StructuralElement2D element in model.Elements)
            {
                if (!byElementId.TryGetValue(element.Id, out double[] q))
                    continue;

                results.Add(element is FrameElement2D frame
                    ? ComputeFrameStations(model, frame, nodalDisplacements, q)
                    : ComputeTrussStations(element, nodalDisplacements, q));
            }

            return results;
        }

        // -------------------------------------------------------------------------
        // Frame element: full N/V/M distribution + Hermite deflected shape.
        // -------------------------------------------------------------------------

        private MemberStationResult ComputeFrameStations(
            StructureModel model, FrameElement2D element, double[,] nodalDisplacements, double[] q)
        {
            double L = element.Length;
            double c = element.CosX;
            double s = element.SinX;

            double fx1 = q[0], fy1 = q[1], mz1 = q[2];

            // Span loads on this member, resolved into local axial / transverse components.
            double udlAxial = 0.0, udlTransverse = 0.0;     // intensities (force / length)
            var pointLoads = new List<(double A, double Px, double Py)>();
            foreach (var ml in model.MemberLoads)
            {
                if (!ReferenceEquals(ml.Element, element))
                    continue;

                switch (ml)
                {
                    case UniformDistributedLoad udl:
                        (double gx, double gy) = GlobalComponents(udl.Direction, udl.MagnitudePerLength);
                        udlAxial += c * gx + s * gy;
                        udlTransverse += -s * gx + c * gy;
                        break;
                    case PointLoad pl:
                        (double px, double py) = GlobalComponents(pl.Direction, pl.Magnitude);
                        pointLoads.Add((pl.DistanceFromStart, c * px + s * py, -s * px + c * py));
                        break;
                    // TemperatureLoad and others contribute through the member-end forces only.
                }
            }

            // Local end displacements and rotations for the Hermite shape.
            (double u1, double v1) = LocalTranslation(nodalDisplacements, element.StartNode.Id, c, s);
            (double u2, double v2) = LocalTranslation(nodalDisplacements, element.EndNode.Id, c, s);
            double r1 = nodalDisplacements[element.StartNode.Id - 1, 2];
            double r2 = nodalDisplacements[element.EndNode.Id - 1, 2];

            var samples = BuildSamples(L, udlTransverse, fy1, pointLoads);

            int n = samples.Count;
            var xs = new double[n];
            var axial = new double[n];
            var shear = new double[n];
            var moment = new double[n];
            var dU = new double[n];
            var dV = new double[n];

            for (int i = 0; i < n; i++)
            {
                (double x, bool inclusive) = samples[i];

                double cumAxial = udlAxial * x;
                double cumTrans = udlTransverse * x;
                double momTrans = udlTransverse * x * x / 2.0;   // ∫ q (x - sigma) dsigma

                foreach (var (a, px, py) in pointLoads)
                {
                    bool counted = inclusive ? a <= x + Eps : a < x - Eps;
                    if (!counted) continue;
                    cumAxial += px;
                    cumTrans += py;
                    momTrans += py * (x - a);
                }

                xs[i] = x;
                axial[i] = -fx1 - cumAxial;
                shear[i] = fy1 + cumTrans;
                moment[i] = -mz1 + fy1 * x + momTrans;

                double t = L > 0.0 ? x / L : 0.0;
                dU[i] = (1.0 - t) * u1 + t * u2;
                dV[i] = Hermite(t, L, v1, r1, v2, r2);
            }

            return new MemberStationResult(element.Id, L, xs, axial, shear, moment, dU, dV);
        }

        // -------------------------------------------------------------------------
        // Truss element: constant axial, no shear/moment, linear deflection.
        // -------------------------------------------------------------------------

        private static MemberStationResult ComputeTrussStations(
            StructuralElement2D element, double[,] nodalDisplacements, double[] q)
        {
            double L = element.Length;
            double c = element.CosX;
            double s = element.SinX;

            double n = -q[0]; // tension positive

            (double u1, double v1) = LocalTranslation(nodalDisplacements, element.StartNode.Id, c, s);
            (double u2, double v2) = LocalTranslation(nodalDisplacements, element.EndNode.Id, c, s);

            return new MemberStationResult(
                element.Id, L,
                new[] { 0.0, L },
                new[] { n, n },
                new[] { 0.0, 0.0 },
                new[] { 0.0, 0.0 },
                new[] { u1, u2 },
                new[] { v1, v2 });
        }

        // -------------------------------------------------------------------------
        // Station layout
        // -------------------------------------------------------------------------

        /// <summary>
        /// Builds the sorted sample list as (position, inclusive) pairs. <c>inclusive</c>
        /// controls whether a point load sitting exactly at the position is counted, so a
        /// point load yields a left limit (exclusive) immediately followed by a right limit
        /// (inclusive) — drawing the shear step vertically. The shear-zero point of each
        /// span is inserted so the moment extremum is sampled exactly.
        /// </summary>
        private List<(double X, bool Inclusive)> BuildSamples(
            double L, double udlTransverse, double fy1, List<(double A, double Px, double Py)> pointLoads)
        {
            var breaks = new SortedSet<double> { 0.0, L };
            foreach (var (a, _, _) in pointLoads)
                if (a > Eps && a < L - Eps)
                    breaks.Add(a);

            var bp = new List<double>(breaks);
            var samples = new List<(double X, bool Inclusive)>();

            // Running transverse load carried into each span (right limit at the span start).
            double pyOffset = 0.0;
            for (int seg = 0; seg < bp.Count - 1; seg++)
            {
                double x0 = bp[seg], x1 = bp[seg + 1];

                // Point load sitting at the start of this span enters the right-limit total.
                foreach (var (a, _, py) in pointLoads)
                    if (Math.Abs(a - x0) <= Eps) pyOffset += py;

                for (int k = 0; k <= _subdivisionsPerSpan; k++)
                {
                    // Pin the endpoints to the exact breakpoint values so a point load's
                    // left limit (x1 of this span) and right limit (x0 of the next) land on
                    // the same X and sort left-before-right; rounding can't reorder them.
                    double x = k == 0 ? x0
                             : k == _subdivisionsPerSpan ? x1
                             : x0 + (x1 - x0) * k / _subdivisionsPerSpan;
                    // k==0 is the right limit at x0 (load there already counted); k==span is
                    // the left limit at x1 (load there not yet counted).
                    bool inclusive = k < _subdivisionsPerSpan;
                    samples.Add((x, inclusive));
                }

                // Shear-zero within the span (V = fy1 + udlTransverse*x + pyOffset): the
                // exact moment peak. Only meaningful when a transverse UDL is present.
                if (Math.Abs(udlTransverse) > Eps)
                {
                    double xZero = -(fy1 + pyOffset) / udlTransverse;
                    if (xZero > x0 + Eps && xZero < x1 - Eps)
                        samples.Add((xZero, true));
                }
            }

            samples.Sort((p, q) =>
            {
                int byX = p.X.CompareTo(q.X);
                return byX != 0 ? byX : p.Inclusive.CompareTo(q.Inclusive); // false (left) before true (right)
            });
            return samples;
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        private const double Eps = 1e-9;

        private static (double U, double V) LocalTranslation(double[,] nodalDisplacements, int nodeId, double c, double s)
        {
            double ux = nodalDisplacements[nodeId - 1, 0];
            double uy = nodalDisplacements[nodeId - 1, 1];
            return (c * ux + s * uy, -s * ux + c * uy);
        }

        /// <summary>Hermite cubic transverse deflection at natural position t in [0,1].</summary>
        private static double Hermite(double t, double L, double v1, double r1, double v2, double r2)
        {
            double t2 = t * t, t3 = t2 * t;
            double h1 = 1.0 - 3.0 * t2 + 2.0 * t3;
            double h2 = L * (t - 2.0 * t2 + t3);
            double h3 = 3.0 * t2 - 2.0 * t3;
            double h4 = L * (-t2 + t3);
            return h1 * v1 + h2 * r1 + h3 * v2 + h4 * r2;
        }

        private static (double X, double Y) GlobalComponents(LoadDirection direction, double magnitude) => direction switch
        {
            LoadDirection.X => (magnitude, 0.0),
            LoadDirection.Y => (0.0, magnitude),
            _ => (0.0, 0.0),
        };
    }
}
