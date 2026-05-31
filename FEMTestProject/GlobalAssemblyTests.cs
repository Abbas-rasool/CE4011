using System;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Geometry;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Properties;
using Matrix_Library.MAIN_TYPES;
using Xunit;

namespace FEMTestProject
{
    /// <summary>
    /// Interface tests for the global stiffness assembly stage. These exercise the
    /// contract between the elements and <see cref="GlobalStiffnessAssembler"/>:
    /// that element contributions are summed at shared DOFs and that restrained
    /// DOFs are excluded from the assembled system.
    ///
    /// Tolerance: assembled stiffness entries are compared to closed-form values
    /// using a relative tolerance of 1e-9 (direct algebraic assembly, no solve).
    /// </summary>
    public class GlobalAssemblyTests
    {
        private const double RelTol = 1e-9;

        private const double E = 200e9;     // Pa
        private const double A = 0.02;      // m^2  (= 0.2 x 0.1)
        private const double I = 0.0001;    // m^4
        private const double L = 4.0;       // m

        private static (Material mat, SectionProperty sec) Properties()
            => (new Material(1, E), new SectionProperty(1, 0.2, 0.1, I));

        // --- INTERFACE TEST 1: contributions at a shared node are summed ---
        [Fact]
        public void Assemble_TwoCollinearFrameElements_SharedNodeStiffnessIsSummed()
        {
            // Arrange: three collinear nodes joined by two identical horizontal
            // frame elements. (No supports: we only assemble, never solve.)
            var (mat, sec) = Properties();
            var n1 = new Node(1, 0.0, 0.0);
            var n2 = new Node(2, 4.0, 0.0);
            var n3 = new Node(3, 8.0, 0.0);

            var model = new StructureModel();
            model.Nodes.Add(n1);
            model.Nodes.Add(n2);
            model.Nodes.Add(n3);
            model.Elements.Add(new FrameElement2D(1, n1, n2, mat, sec));
            model.Elements.Add(new FrameElement2D(2, n2, n3, mat, sec));

            DofMap dofMap = new DofNumberingService().BuildEquationNumbers(model);

            // Act
            SparseMatrix k = new GlobalStiffnessAssembler().Assemble(model, dofMap);

            // Assert: the shared middle node carries the axial stiffness of BOTH
            // elements (EA/L each), while an end node carries only one element's.
            int endUx = dofMap.GetEquation(1, DofType.Ux) - 1;
            int sharedUx = dofMap.GetEquation(2, DofType.Ux) - 1;

            AssertClose(E * A / L, k.Get(endUx, endUx), RelTol);
            AssertClose(2.0 * E * A / L, k.Get(sharedUx, sharedUx), RelTol);
        }

        // --- INTERFACE TEST 2: restrained DOFs are excluded from the system ---
        [Fact]
        public void Assemble_SingleElementWithFixedNode_KeepsOnlyActiveDofBlock()
        {
            // Arrange: one horizontal frame element, fully fixed at the start node.
            var (mat, sec) = Properties();
            var n1 = new Node(1, 0.0, 0.0);
            var n2 = new Node(2, 4.0, 0.0);

            var model = new StructureModel();
            model.Nodes.Add(n1);
            model.Nodes.Add(n2);
            model.Elements.Add(new FrameElement2D(1, n1, n2, mat, sec));
            model.Supports.Add(new SupportCondition(n1, true, true, true)); // fix node 1

            DofMap dofMap = new DofNumberingService().BuildEquationNumbers(model);

            // Act
            SparseMatrix k = new GlobalStiffnessAssembler().Assemble(model, dofMap);

            // Assert: only node 2's three DOFs remain → 3x3 system, equal to the
            // free–free diagonal block of the element's global stiffness matrix.
            Assert.Equal(3, k.RowCount);

            int ux = dofMap.GetEquation(2, DofType.Ux) - 1;
            int uy = dofMap.GetEquation(2, DofType.Uy) - 1;
            int rz = dofMap.GetEquation(2, DofType.Rz) - 1;

            AssertClose(E * A / L, k.Get(ux, ux), RelTol);                 // axial
            AssertClose(12.0 * E * I / (L * L * L), k.Get(uy, uy), RelTol); // shear
            AssertClose(4.0 * E * I / L, k.Get(rz, rz), RelTol);           // bending
            AssertClose(-6.0 * E * I / (L * L), k.Get(rz, uy), RelTol);    // shear-bending coupling
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
