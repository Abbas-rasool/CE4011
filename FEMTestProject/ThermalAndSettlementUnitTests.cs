using System;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Geometry;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Members;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Properties;
using Matrix_Library.MAIN_TYPES;
using Xunit;

namespace FEMTestProject
{
    /// <summary>
    /// Unit tests for the two new capabilities, each exercising a single class in
    /// isolation (no full solve):
    ///   - TemperatureLoad: thermal fixed-end forces.
    ///   - SettlementLoadBuilder: the -Kfr*Ur equivalent load for a prescribed settlement.
    ///
    /// Tolerance: results are closed-form algebra (no iterative solve), so a tight
    /// relative tolerance of 1e-9 is used.
    /// </summary>
    public class ThermalAndSettlementUnitTests
    {
        private const double RelTol = 1e-9;

        private const double E = 200e9;
        private const double A = 0.02;   // 0.2 x 0.1
        private const double I = 0.0001;

        private static (Material mat, SectionProperty sec) Properties()
            => (new Material(1, E), new SectionProperty(1, 0.2, 0.1, I));

        // --- UNIT TEST 1 (thermal): TemperatureLoad fixed-end forces ---
        // Uniform change -> axial force E*A*alpha*dTu; gradient -> end moments
        // E*I*alpha*dTg/h. No shear terms.
        [Fact]
        public void TemperatureLoad_LocalFixedEndForces_MatchUniformAndGradientEffects()
        {
            const double dTu = 40.0, dTg = 25.0, alpha = 1.2e-5, depth = 0.3;

            var (mat, sec) = Properties();
            var n1 = new Node(1, 0.0, 0.0);
            var n2 = new Node(2, 4.0, 0.0);
            var element = new FrameElement2D(1, n1, n2, mat, sec);

            var thermal = new TemperatureLoad(element, dTu, dTg, alpha, depth);

            double[] f = thermal.GetLocalFixedEndForces();

            double expectedAxial = E * A * alpha * dTu;       // N
            double expectedMoment = E * I * alpha * dTg / depth; // N*m

            Assert.Equal(6, f.Length);
            AssertClose(expectedAxial, f[0], RelTol);   // N1
            AssertClose(0.0, f[1], RelTol);             // V1
            AssertClose(expectedMoment, f[2], RelTol);  // M1
            AssertClose(-expectedAxial, f[3], RelTol);  // N2
            AssertClose(0.0, f[4], RelTol);             // V2
            AssertClose(-expectedMoment, f[5], RelTol); // M2
        }

        // --- UNIT TEST 2 (settlement): SettlementLoadBuilder equivalent load ---
        // For a horizontal element fixed at node 1 with a vertical base settlement
        // delta, the equivalent loads on node 2's free DOFs are -Ke[a, v1]*delta:
        //   Uy2 :  +12*E*I*delta / L^3
        //   Rz2 :  -6*E*I*delta / L^2
        //   Ux2 :   0
        [Fact]
        public void SettlementLoadBuilder_FixedBaseVerticalSettlement_ProducesExpectedEquivalentLoads()
        {
            const double L = 4.0, delta = 0.01;

            var (mat, sec) = Properties();
            var n1 = new Node(1, 0.0, 0.0);
            var n2 = new Node(2, L, 0.0);

            var model = new StructureModel();
            model.Nodes.Add(n1);
            model.Nodes.Add(n2);
            model.Elements.Add(new FrameElement2D(1, n1, n2, mat, sec));

            // Node 1 fully fixed and settling vertically by -delta; node 2 free.
            model.Supports.Add(new SupportCondition(n1, true, true, true, 0.0, delta, 0.0));

            DofMap dofMap = new DofNumberingService().BuildEquationNumbers(model);
            CustomVector loadVector = new CustomVector(dofMap.NumberOfEquations);

            // Act: build only the settlement equivalent load (no solve).
            new SettlementLoadBuilder().Apply(model, dofMap, loadVector);

            int ux2 = dofMap.GetEquation(2, DofType.Ux) - 1;
            int uy2 = dofMap.GetEquation(2, DofType.Uy) - 1;
            int rz2 = dofMap.GetEquation(2, DofType.Rz) - 1;

            AssertClose(0.0, loadVector.Get(ux2), RelTol);
            AssertClose(12.0 * E * I * delta / (L * L * L), loadVector.Get(uy2), RelTol);
            AssertClose(-6.0 * E * I * delta / (L * L), loadVector.Get(rz2), RelTol);
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
