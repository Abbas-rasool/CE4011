using System;
using System.Linq;
using FrameAnalysisProgram.ANALYSIS_CORE;
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
    /// End-to-end regression tests covering a frame, a truss, and a combined
    /// frame-truss structure. Each runs the full analysis pipeline and compares
    /// results against closed-form values and/or global equilibrium.
    ///
    /// Tolerances (clearly stated):
    /// - RelTol     = 1e-6  : relative tolerance for displacement / force values
    ///                        (direct solver; a single Euler-Bernoulli element
    ///                        reproduces the analytical solution exactly).
    /// - EquilTolN  = 1e-3  : absolute tolerance (N, N*m) on the residual of
    ///                        global equilibrium (reactions + applied loads = 0).
    /// </summary>
    public class RegressionTests
    {
        private const double RelTol = 1e-6;
        private const double EquilTolN = 1e-3;

        // --- REGRESSION TEST 1: FRAME (cantilever, closed-form) ---
        [Fact]
        public void Frame_CantileverTipPointLoad_MatchesEulerBernoulliClosedForm()
        {
            const double E = 200e9, I = 0.0001, L = 3.0, P = 10000.0;

            var mat = new Material(1, E);
            var sec = new SectionProperty(1, 0.2, 0.1, I); // A = 0.2 x 0.1 = 0.02
            var n1 = new Node(1, 0.0, 0.0);
            var n2 = new Node(2, L, 0.0);

            var model = new StructureModel();
            model.Nodes.Add(n1);
            model.Nodes.Add(n2);
            model.Elements.Add(new FrameElement2D(1, n1, n2, mat, sec));
            model.Supports.Add(new SupportCondition(n1, true, true, true)); // fixed
            model.Loads.Add(new NodalLoad(n2, 0.0, -P, 0.0));               // downward tip load

            FrameAnalysisResult result = Analyze(model);

            // Closed-form cantilever (downward load => negative deflection/rotation).
            double expectedTipDeflection = -P * L * L * L / (3.0 * E * I);
            double expectedTipRotation = -P * L * L / (2.0 * E * I);

            AssertClose(expectedTipDeflection, result.NodalDisplacements[1, 1], RelTol); // node 2 Uy
            AssertClose(expectedTipRotation, result.NodalDisplacements[1, 2], RelTol);   // node 2 Rz

            NodalReaction support = result.Reactions.Single(r => r.NodeId == 1);
            AssertClose(P, support.Fy, RelTol);        // vertical reaction balances the load
            AssertClose(P * L, support.Mz, RelTol);    // fixed-end moment = P*L

            AssertGlobalEquilibrium(model, result);
        }

        // --- REGRESSION TEST 2: TRUSS (two-bar, statically determinate) ---
        [Fact]
        public void Truss_TwoBarApexLoad_MatchesStaticEquilibriumClosedForm()
        {
            const double E = 200e9, P = 20000.0;

            var mat = new Material(1, E);
            var sec = new SectionProperty(1, 0.1, 0.1, 0.0001); // A = 0.01
            var n1 = new Node(1, 0.0, 0.0);
            var n2 = new Node(2, 4.0, 0.0);
            var n3 = new Node(3, 2.0, 3.0);

            var model = new StructureModel();
            model.Nodes.Add(n1);
            model.Nodes.Add(n2);
            model.Nodes.Add(n3);
            model.Elements.Add(new TrussElement2D(1, n1, n3, mat, sec));
            model.Elements.Add(new TrussElement2D(2, n2, n3, mat, sec));
            model.Supports.Add(new SupportCondition(n1, true, true, false)); // pin
            model.Supports.Add(new SupportCondition(n2, true, true, false)); // pin
            model.Loads.Add(new NodalLoad(n3, 0.0, -P, 0.0));

            FrameAnalysisResult result = Analyze(model);

            // Each bar length sqrt(13); by symmetry each carries vertical P/2.
            double barLength = Math.Sqrt(2.0 * 2.0 + 3.0 * 3.0);
            double expectedAxial = P * barLength / 6.0; // = (P/2)/(3/L) ~ 12018.5 N (compression)

            // Truss local end forces are [Fx1, Fy1, Fx2, Fy2]; |Fx1| is the axial force.
            double[] bar1 = result.ElementEndForces.Single(e => e.Element.Id == 1).LocalEndForces;
            Assert.Equal(4, bar1.Length);
            AssertClose(expectedAxial, Math.Abs(bar1[0]), RelTol);

            NodalReaction r1 = result.Reactions.Single(r => r.NodeId == 1);
            NodalReaction r2 = result.Reactions.Single(r => r.NodeId == 2);
            AssertClose(P / 2.0, r1.Fy, RelTol);
            AssertClose(P / 2.0, r2.Fy, RelTol);

            AssertGlobalEquilibrium(model, result);
        }

        // --- REGRESSION TEST 3: FRAME-TRUSS (portal frame with a truss brace) ---
        [Fact]
        public void FrameTruss_BracedPortal_SolvesAndSatisfiesEquilibrium()
        {
            const double E = 200e9, P = 50000.0;

            var mat = new Material(1, E);
            var sec = new SectionProperty(1, 0.2, 0.1, 0.0001); // A = 0.02

            var n1 = new Node(1, 0.0, 0.0);
            var n2 = new Node(2, 0.0, 3.0);
            var n3 = new Node(3, 4.0, 3.0);
            var n4 = new Node(4, 4.0, 0.0);

            var model = new StructureModel();
            model.Nodes.Add(n1);
            model.Nodes.Add(n2);
            model.Nodes.Add(n3);
            model.Nodes.Add(n4);

            // Portal frame: two columns and a beam (frame elements)...
            model.Elements.Add(new FrameElement2D(1, n1, n2, mat, sec)); // left column
            model.Elements.Add(new FrameElement2D(2, n2, n3, mat, sec)); // beam
            model.Elements.Add(new FrameElement2D(3, n4, n3, mat, sec)); // right column
            // ...stiffened by a diagonal truss brace.
            model.Elements.Add(new TrussElement2D(4, n1, n3, mat, sec));  // brace

            model.Supports.Add(new SupportCondition(n1, true, true, true));
            model.Supports.Add(new SupportCondition(n4, true, true, true));
            model.Loads.Add(new NodalLoad(n2, P, 0.0, 0.0)); // lateral load

            FrameAnalysisResult result = Analyze(model);

            // Physics-based check that holds regardless of implementation details.
            AssertGlobalEquilibrium(model, result);

            // Regression baseline: lateral sway at node 2 (locked to current solver
            // output; relative tolerance RelTol). Captured from a verified run.
            double sway = result.NodalDisplacements[1, 0]; // node 2 Ux
            AssertClose(0.0001645549288, sway, RelTol);

            // The diagonal brace must carry axial load only (4-DOF truss output).
            double[] brace = result.ElementEndForces.Single(e => e.Element.Id == 4).LocalEndForces;
            Assert.Equal(4, brace.Length);
            Assert.True(Math.Abs(brace[0]) > 1.0, "Brace should carry a non-trivial axial force.");
        }

        // --- REGRESSION TEST 4: THERMAL (axially restrained bar, uniform heating) ---
        [Fact]
        public void Thermal_AxiallyRestrainedBar_UniformHeating_DevelopsExpectedAxialForce()
        {
            const double E = 200e9, A = 0.02, alpha = 1.2e-5, dT = 50.0, L = 4.0;

            var mat = new Material(1, E);
            var sec = new SectionProperty(1, 0.2, 0.1, 0.0001); // A = 0.02
            var n1 = new Node(1, 0.0, 0.0);
            var n2 = new Node(2, L, 0.0);

            var model = new StructureModel();
            model.Nodes.Add(n1);
            model.Nodes.Add(n2);

            var element = new FrameElement2D(1, n1, n2, mat, sec);
            model.Elements.Add(element);

            // Both ends axially restrained (n1 fully fixed; n2 restrains Ux only).
            model.Supports.Add(new SupportCondition(n1, true, true, true));
            model.Supports.Add(new SupportCondition(n2, true, false, false));

            // Uniform temperature rise on the member (no gradient).
            model.MemberLoads.Add(new TemperatureLoad(element, dT, 0.0, alpha, 0.1));

            FrameAnalysisResult result = Analyze(model);

            // Fully restrained axial bar: N = E * A * alpha * dT.
            double expectedAxial = E * A * alpha * dT;
            double[] f = result.ElementEndForces.Single(e => e.Element.Id == 1).LocalEndForces;
            AssertClose(expectedAxial, Math.Abs(f[0]), RelTol);

            // Thermal effect is self-equilibrated: reactions sum to zero.
            AssertGlobalEquilibrium(model, result);
        }

        // --- REGRESSION TEST 5: SETTLEMENT (propped cantilever, prop settles) ---
        [Fact]
        public void Settlement_ProppedCantilever_PropSettles_MatchesTipStiffness()
        {
            const double E = 200e9, I = 0.0001, L = 3.0, delta = 0.005; // 5 mm

            var mat = new Material(1, E);
            var sec = new SectionProperty(1, 0.2, 0.1, I);
            var n1 = new Node(1, 0.0, 0.0);
            var n2 = new Node(2, L, 0.0);

            var model = new StructureModel();
            model.Nodes.Add(n1);
            model.Nodes.Add(n2);
            model.Elements.Add(new FrameElement2D(1, n1, n2, mat, sec));

            model.Supports.Add(new SupportCondition(n1, true, true, true));                  // fixed
            model.Supports.Add(new SupportCondition(n2, false, true, false, 0.0, -delta, 0.0)); // roller, settles down

            FrameAnalysisResult result = Analyze(model);

            // The prescribed settlement is reproduced exactly at the restrained DOF.
            AssertClose(-delta, result.NodalDisplacements[1, 1], RelTol); // node 2 Uy

            // Forcing a cantilever tip by delta requires the tip stiffness 3EI/L^3.
            double expectedPropReaction = 3.0 * E * I * delta / (L * L * L);
            NodalReaction r2 = result.Reactions.Single(r => r.NodeId == 2);
            AssertClose(expectedPropReaction, Math.Abs(r2.Fy), RelTol);

            AssertGlobalEquilibrium(model, result);
        }

        // -------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------

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
                new ReactionRecovery());

            return analyzer.Analyze(model);
        }

        /// <summary>
        /// Verifies sum(reactions) + sum(applied nodal loads) = 0 for forces and
        /// moments (taken about the global origin), within EquilTolN.
        /// Valid for models without member (span) loads.
        /// </summary>
        private static void AssertGlobalEquilibrium(StructureModel model, FrameAnalysisResult result)
        {
            double sumFx = 0.0, sumFy = 0.0, sumM = 0.0;

            foreach (INodalLoadLike load in EnumerateAppliedLoads(model))
            {
                sumFx += load.Fx;
                sumFy += load.Fy;
                sumM += load.X * load.Fy - load.Y * load.Fx + load.Mz;
            }

            foreach (NodalReaction r in result.Reactions)
            {
                Node node = model.Nodes.Single(n => n.Id == r.NodeId);
                sumFx += r.Fx;
                sumFy += r.Fy;
                sumM += node.X * r.Fy - node.Y * r.Fx + r.Mz;
            }

            Assert.True(Math.Abs(sumFx) <= EquilTolN, $"Fx not balanced: residual {sumFx:G3} N");
            Assert.True(Math.Abs(sumFy) <= EquilTolN, $"Fy not balanced: residual {sumFy:G3} N");
            Assert.True(Math.Abs(sumM) <= EquilTolN, $"Moment not balanced: residual {sumM:G3} N*m");
        }

        private readonly struct INodalLoadLike
        {
            public double X { get; init; }
            public double Y { get; init; }
            public double Fx { get; init; }
            public double Fy { get; init; }
            public double Mz { get; init; }
        }

        private static System.Collections.Generic.IEnumerable<INodalLoadLike> EnumerateAppliedLoads(StructureModel model)
        {
            foreach (var load in model.Loads)
                yield return new INodalLoadLike
                {
                    X = load.Node.X,
                    Y = load.Node.Y,
                    Fx = load.Fx,
                    Fy = load.Fy,
                    Mz = load.Mz
                };
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
