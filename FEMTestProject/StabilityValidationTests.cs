using System;
using System.Linq;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.ANALYSIS_CORE.Validation;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Geometry;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Properties;
using Matrix_Library.SOLVERS;
using Xunit;

namespace FEMTestProject
{
    /// <summary>
    /// Tests for the stability / model-validation feature: the program must not
    /// crash, and must give a clear error/warning explaining the likely cause.
    /// </summary>
    public class StabilityValidationTests
    {
        private static (Material mat, SectionProperty sec) Props()
            => (new Material(1, 200e9), new SectionProperty(1, 0.3, 0.3, 0.000675));

        // --- UNIT TEST: ConnectivityRule flags a disconnected model (case b) ---
        [Fact]
        public void ConnectivityRule_TwoSeparateElements_ReportsDisconnectedError()
        {
            var (mat, sec) = Props();
            var model = new StructureModel();
            var n1 = new Node(1, 0, 0);
            var n2 = new Node(2, 0, 4);
            var n3 = new Node(3, 6, 0);
            var n4 = new Node(4, 6, 4);
            model.Nodes.AddRange(new[] { n1, n2, n3, n4 });
            model.Elements.Add(new FrameElement2D(1, n1, n2, mat, sec));
            model.Elements.Add(new FrameElement2D(2, n3, n4, mat, sec)); // no element joins the two

            var messages = new ConnectivityRule().Validate(model).ToList();

            Assert.Contains(messages, m =>
                m.Severity == ValidationSeverity.Error && m.Message.Contains("disconnected"));
        }

        // --- UNIT TEST: GlobalRestraintRule flags a missing horizontal restraint (case a) ---
        [Fact]
        public void GlobalRestraintRule_OnlyVerticalRollers_ReportsSwayError()
        {
            var (mat, sec) = Props();
            var model = new StructureModel();
            var n1 = new Node(1, 0, 0);
            var n2 = new Node(2, 0, 4);
            model.Nodes.AddRange(new[] { n1, n2 });
            model.Elements.Add(new FrameElement2D(1, n1, n2, mat, sec));
            model.Supports.Add(new SupportCondition(n1, false, true, false)); // roller: Uy only

            var messages = new GlobalRestraintRule().Validate(model).ToList();

            Assert.Contains(messages, m =>
                m.Severity == ValidationSeverity.Error && m.Message.Contains("X-translation"));
        }

        // --- INTEGRATION TEST: analyzer refuses a mechanism with a clear message (case d) ---
        [Fact]
        public void Analyze_TwoBarTrussOnPinAndRoller_ThrowsStructuralAnalysisException()
        {
            var (mat, sec) = Props();
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

            var ex = Assert.Throws<StructuralAnalysisException>(() => BuildAnalyzer().Analyze(model));
            Assert.NotEmpty(ex.Messages);
            Assert.Contains(ex.Messages, m => m.Severity == ValidationSeverity.Error);
        }

        // --- INTEGRATION TEST: an indeterminate but stable frame is analyzed, not rejected (case c) ---
        [Fact]
        public void Analyze_FixedBasePortal_SucceedsAndReportsIndeterminate()
        {
            var (mat, sec) = Props();
            var model = new StructureModel();
            var n1 = new Node(1, 0, 0);
            var n2 = new Node(2, 0, 4);
            var n3 = new Node(3, 6, 4);
            var n4 = new Node(4, 6, 0);
            model.Nodes.AddRange(new[] { n1, n2, n3, n4 });
            model.Elements.Add(new FrameElement2D(1, n1, n2, mat, sec));
            model.Elements.Add(new FrameElement2D(2, n2, n3, mat, sec));
            model.Elements.Add(new FrameElement2D(3, n4, n3, mat, sec));
            model.Supports.Add(new SupportCondition(n1, true, true, true));
            model.Supports.Add(new SupportCondition(n4, true, true, true));
            model.Loads.Add(new NodalLoad(n2, 10000, 0, 0));

            FrameAnalysisResult result = BuildAnalyzer().Analyze(model); // must not throw

            Assert.Contains(result.ValidationMessages, m =>
                m.Severity == ValidationSeverity.Info && m.Message.Contains("indeterminate"));
        }

        private static FrameAnalyzer BuildAnalyzer()
        {
            var displacementMapper = new DisplacementMapper();
            return new FrameAnalyzer(
                new DofNumberingService(),
                new GlobalStiffnessAssembler(),
                new LoadVectorBuilder(),
                new SettlementLoadBuilder(),
                new CSparseCholeskySolver(),
                displacementMapper,
                new ElementForceRecovery(displacementMapper),
                new ReactionRecovery(),
                ModelValidator.CreateDefault(),
                new StiffnessSingularityDetector());
        }
    }
}
