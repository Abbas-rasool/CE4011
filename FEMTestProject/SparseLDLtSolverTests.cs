using System;
using System.Linq;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.ANALYSIS_CORE.Validation;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Geometry;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Properties;
using Matrix_Library.MAIN_TYPES;
using Matrix_Library.SOLVERS;
using Xunit;

namespace FEMTestProject
{
    /// <summary>
    /// Tests for the hand-written <see cref="SparseLDLtSolver"/>. The focus is the
    /// pivot-rejection contract: a stiffness matrix is only usable if it is
    /// symmetric positive-definite, so the solver must reject any non-positive
    /// pivot (zero = singular / mechanism, negative = not positive-definite),
    /// judged relative to the matrix scale. This is what lets the analyzer's
    /// StiffnessSingularityDetector fire and localize the instability.
    /// </summary>
    public class SparseLDLtSolverTests
    {
        // Builds a symmetric SparseMatrix (lower-triangle storage) from a full 2D array.
        private static SparseMatrix Symmetric(double[,] full)
        {
            int n = full.GetLength(0);
            var m = new SparseMatrix(n);
            for (int i = 0; i < n; i++)
                for (int j = 0; j <= i; j++)
                    m.Set(i, j, full[i, j]);
            return m;
        }

        private static CustomVector Vector(params double[] values)
        {
            var v = new CustomVector(values.Length);
            for (int i = 0; i < values.Length; i++)
                v[i] = values[i];
            return v;
        }

        [Fact]
        public void Solve_SpdSystem_ReturnsCorrectSolution()
        {
            // K = [[4,1],[1,3]], b = [1,2]  =>  x = [1/11, 7/11]
            var k = Symmetric(new double[,] { { 4, 1 }, { 1, 3 } });
            var solver = new SparseLDLtSolver();
            solver.Factorize(k);

            CustomVector x = solver.Solve(Vector(1, 2));

            Assert.Equal(1.0 / 11.0, x[0], 10);
            Assert.Equal(7.0 / 11.0, x[1], 10);
        }

        [Fact]
        public void Factorize_SingularMatrix_Throws()
        {
            // Rank-deficient: second pivot is exactly zero.
            var k = Symmetric(new double[,] { { 1, 1 }, { 1, 1 } });

            Assert.Throws<InvalidOperationException>(() => new SparseLDLtSolver().Factorize(k));
        }

        [Fact]
        public void Factorize_IndefiniteMatrix_Throws()
        {
            // Regression for the negative-pivot fix: D = [2, -3]. A large NEGATIVE
            // pivot must be rejected. The old Math.Abs() test let this through and
            // returned a garbage solution; the signed test now throws.
            var k = Symmetric(new double[,] { { 2, 0 }, { 0, -3 } });

            Assert.Throws<InvalidOperationException>(() => new SparseLDLtSolver().Factorize(k));
        }

        [Fact]
        public void Factorize_PivotSmallRelativeToScale_Throws()
        {
            // Regression for the scale-aware tolerance. Diagonal scale is ~1e8, and
            // the second pivot collapses to 1e-3 (singular relative to scale, though
            // far above the old absolute 1e-12 threshold).
            var k = Symmetric(new double[,]
            {
                { 1e8, 1e8 },
                { 1e8, 1e8 + 1e-3 },
            });

            Assert.Throws<InvalidOperationException>(() => new SparseLDLtSolver().Factorize(k));
        }

        [Fact]
        public void Solve_MatchesCSparse_OnSpdSystem()
        {
            // The two ILinearSolver implementations are interchangeable: on a stable
            // SPD system they must agree.
            var k = Symmetric(new double[,]
            {
                { 6, 2, 1 },
                { 2, 5, 2 },
                { 1, 2, 4 },
            });
            var b = Vector(7, 9, 6);

            var mine = new SparseLDLtSolver();
            mine.Factorize(Symmetric(new double[,] { { 6, 2, 1 }, { 2, 5, 2 }, { 1, 2, 4 } }));
            CustomVector xMine = mine.Solve(b);

            var theirs = new CSparseCholeskySolver();
            theirs.Factorize(k);
            CustomVector xTheirs = theirs.Solve(b);

            for (int i = 0; i < 3; i++)
                Assert.Equal(xTheirs[i], xMine[i], 9);
        }

        // --- INTEGRATION: the catch -> detector path also works with the custom solver ---
        [Fact]
        public void Analyze_WithCustomSolver_MechanismThrowsLocalizedError()
        {
            var mat = new Material(1, 200e9);
            var sec = new SectionProperty(1, 0.3, 0.3, 0.000675);

            var model = new StructureModel();
            var n1 = new Node(1, 0, 0);
            var n2 = new Node(2, 4, 0);
            var n3 = new Node(3, 2, 3);
            model.Nodes.AddRange(new[] { n1, n2, n3 });
            model.Elements.Add(new TrussElement2D(1, n1, n3, mat, sec));
            model.Elements.Add(new TrussElement2D(2, n2, n3, mat, sec));
            model.Supports.Add(new SupportCondition(n1, true, true, false)); // pin
            model.Supports.Add(new SupportCondition(n2, false, true, false)); // roller
            model.Loads.Add(new NodalLoad(n3, 0, -20000, 0));

            var displacementMapper = new DisplacementMapper();
            var analyzer = new FrameAnalyzer(
                new DofNumberingService(),
                new GlobalStiffnessAssembler(),
                new LoadVectorBuilder(),
                new SettlementLoadBuilder(),
                new SparseLDLtSolver(), // exercise the hand-written solver end-to-end
                displacementMapper,
                new ElementForceRecovery(displacementMapper),
                new ReactionRecovery(),
                ModelValidator.CreateDefault(),
                new StiffnessSingularityDetector());

            var ex = Assert.Throws<StructuralAnalysisException>(() => analyzer.Analyze(model));
            Assert.Contains(ex.Messages, m => m.Severity == ValidationSeverity.Error);
        }
    }
}
