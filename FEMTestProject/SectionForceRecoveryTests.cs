using System;
using System.Linq;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.ANALYSIS_CORE.Validation;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Geometry;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Members;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Properties;
using Matrix_Library.SOLVERS;
using Xunit;

namespace FEMTestProject
{
    /// <summary>
    /// Closed-form checks for <see cref="SectionForceRecovery"/>: the sampled N/V/M
    /// distributions and the Hermite deflected shape against hand calculations.
    /// </summary>
    public class SectionForceRecoveryTests
    {
        private const double E = 200e9, I = 1e-4;
        private const double Tol = 1e-6;

        [Fact]
        public void Cantilever_TipPointLoad_ShearConstant_MomentLinear_DeflectionCubic()
        {
            const double L = 3.0, P = 10000.0;

            var model = BeamModel(L, fixStart: true, out FrameElement2D element);
            model.Loads.Add(new NodalLoad(model.Nodes[1], 0.0, -P, 0.0));

            MemberStationResult m = StationsFor(model, element.Id);

            // Hogging cantilever: M = -P(L - x); shear constant = P; no axial.
            AssertClose(-P * L, m.Moment[0], Tol);                 // fixed end
            AssertClose(0.0, m.Moment[m.Moment.Count - 1], Tol);   // free tip
            AssertClose(P * L, m.MaxAbsMoment, Tol);
            foreach (double v in m.Shear) AssertClose(P, v, Tol);
            foreach (double n in m.Axial) AssertClose(0.0, n, Tol);

            // Hermite is exact for the cubic shape under a tip point load.
            double vMid = TransverseAt(m, L / 2.0);
            AssertClose(-5.0 * P * L * L * L / (48.0 * E * I), vMid, Tol);
        }

        [Fact]
        public void SimplySupported_Udl_ParabolicMoment_LinearShear()
        {
            const double L = 4.0, w = 5000.0; // downward intensity

            var model = SimplySupportedBeam(L, out FrameElement2D element);
            model.MemberLoads.Add(new UniformDistributedLoad(element, -w, LoadDirection.Y));

            MemberStationResult m = StationsFor(model, element.Id);

            AssertClose(w * L * L / 8.0, m.MaxAbsMoment, Tol);   // mid-span sagging peak
            AssertClose(w * L / 2.0, m.Shear[0], Tol);           // +wL/2 at the start
            AssertClose(-w * L / 2.0, m.Shear[m.Shear.Count - 1], Tol); // -wL/2 at the end
            AssertClose(w * L / 2.0, m.MaxAbsShear, Tol);

            // The exact moment peak is sampled (shear-zero station inserted at mid-span).
            double mMid = MomentAt(m, L / 2.0);
            AssertClose(w * L * L / 8.0, mMid, Tol);
        }

        [Fact]
        public void SimplySupported_MidspanPointLoad_ShearSteps_MomentPeak()
        {
            const double L = 4.0, P = 8000.0;

            var model = SimplySupportedBeam(L, out FrameElement2D element);
            model.MemberLoads.Add(new PointLoad(element, L / 2.0, -P, LoadDirection.Y));

            MemberStationResult m = StationsFor(model, element.Id);

            AssertClose(P * L / 4.0, m.MaxAbsMoment, Tol);   // PL/4 under the load
            AssertClose(P * L / 4.0, MomentAt(m, L / 2.0), Tol);

            // Shear steps from +P/2 to -P/2 across the load: both limits are present.
            double[] atMid = m.X
                .Select((x, k) => (x, k))
                .Where(p => Math.Abs(p.x - L / 2.0) <= 1e-9)
                .Select(p => m.Shear[p.k])
                .ToArray();
            Assert.Contains(atMid, v => Math.Abs(v - P / 2.0) <= 1e-3);
            Assert.Contains(atMid, v => Math.Abs(v + P / 2.0) <= 1e-3);
        }

        // -------------------------------------------------------------------------
        // Model builders + helpers
        // -------------------------------------------------------------------------

        private static StructureModel BeamModel(double L, bool fixStart, out FrameElement2D element)
        {
            var mat = new Material(1, E);
            var sec = new SectionProperty(1, 0.2, 0.1, I);
            var n1 = new Node(1, 0.0, 0.0);
            var n2 = new Node(2, L, 0.0);

            var model = new StructureModel();
            model.Nodes.Add(n1);
            model.Nodes.Add(n2);
            element = new FrameElement2D(1, n1, n2, mat, sec);
            model.Elements.Add(element);
            if (fixStart)
                model.Supports.Add(new SupportCondition(n1, true, true, true));
            return model;
        }

        private static StructureModel SimplySupportedBeam(double L, out FrameElement2D element)
        {
            var model = BeamModel(L, fixStart: false, out element);
            model.Supports.Add(new SupportCondition(model.Nodes[0], true, true, false));  // pin
            model.Supports.Add(new SupportCondition(model.Nodes[1], false, true, false)); // vertical roller
            return model;
        }

        private static MemberStationResult StationsFor(StructureModel model, int elementId)
        {
            FrameAnalysisResult result = Analyze(model);
            return result.MemberStations.Single(s => s.ElementId == elementId);
        }

        private static double MomentAt(MemberStationResult m, double x) => SampleAt(m, x, m.Moment);
        private static double TransverseAt(MemberStationResult m, double x) => SampleAt(m, x, m.DeflectionTransverse);

        private static double SampleAt(MemberStationResult m, double x, System.Collections.Generic.IReadOnlyList<double> values)
        {
            int best = 0;
            double bestDist = double.MaxValue;
            for (int i = 0; i < m.X.Count; i++)
            {
                double d = Math.Abs(m.X[i] - x);
                if (d < bestDist) { bestDist = d; best = i; }
            }
            return values[best];
        }

        private static FrameAnalysisResult Analyze(StructureModel model)
        {
            var displacementMapper = new DisplacementMapper();
            var analyzer = new FrameAnalyzer(
                new DofNumberingService(),
                new GlobalStiffnessAssembler(),
                new LoadVectorBuilder(),
                new SettlementLoadBuilder(),
                new CSparseCholeskySolver(),
                displacementMapper,
                new ElementForceRecovery(displacementMapper),
                new ReactionRecovery(),
                ModelValidator.CreateDefault(),
                new StiffnessSingularityDetector(),
                new SectionForceRecovery());
            return analyzer.Analyze(model);
        }

        private static void AssertClose(double expected, double actual, double relativeTolerance)
        {
            double allowed = relativeTolerance * Math.Max(1.0, Math.Abs(expected));
            Assert.True(
                Math.Abs(expected - actual) <= allowed,
                $"Expected {expected:G10}, got {actual:G10} (tolerance {allowed:G3}).");
        }
    }
}
