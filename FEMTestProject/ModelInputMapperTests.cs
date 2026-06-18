using System;
using System.Linq;
using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysis.UI.Core.Mapping;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.ANALYSIS_CORE.Validation;
using FrameAnalysisProgram.INPUT_OUTPUT;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads;
using Matrix_Library.SOLVERS;
using Xunit;

namespace FEMTestProject
{
    /// <summary>
    /// Verifies ModelInputMapper.ToInputData flattens the object-reference document into
    /// the solver's positional tables correctly, that the result is consumable end-to-end
    /// by StructureModelBuilder + FrameAnalyzer, and that reordering rows is safe.
    /// </summary>
    public class ModelInputMapperTests
    {
        [Fact]
        public void ToInputData_PortalFrame_MapsColumnsAndOmitsEmptyOptionalTables()
        {
            ProjectDocument doc = UiTestDocuments.BuildPortalFrameDocument();

            StructureInputData input = ModelInputMapper.ToInputData(doc);

            // Node table: [X, Y], node 2 (index 1) is at (0, 3).
            Assert.Equal(0.0, input.NodeTable[1, 0]);
            Assert.Equal(3.0, input.NodeTable[1, 1]);

            // Material / section columns, converted to canonical units: E from MPa to kN/m²
            // (×1000); Width/Depth from mm to m (×1e-3); I from mm⁴ to m⁴ (×1e-12).
            Assert.Equal(2.0e8, input.MaterialTable[0, 0]);
            Assert.Equal(0.1, input.SectionTable[0, 0], 9);    // Width  (100 mm)
            Assert.Equal(0.1, input.SectionTable[0, 1], 9);    // Depth  (100 mm, table "Length")
            Assert.Equal(0.0001, input.SectionTable[0, 2], 12); // I     (1e8 mm⁴)

            // Element table: [Start, End, Mat, Sec, Type, Release]. Element 3 is n4 -> n3.
            Assert.Equal(4, input.ElementTable[2, 0]);
            Assert.Equal(3, input.ElementTable[2, 1]);
            Assert.Equal(0, input.ElementTable[2, 4]); // Frame
            Assert.Equal(0, input.ElementTable[2, 5]); // Release None

            // Joint load: [NodeId, Fx, Fy, Mz] at node 2.
            Assert.Equal(2.0, input.LoadTable[0, 0]);
            Assert.Equal(50.0, input.LoadTable[0, 1]);

            // Distributed load on element 2, -10 in global Y (direction code 1).
            Assert.NotNull(input.DistributedLoadTable);
            Assert.Equal(2.0, input.DistributedLoadTable![0, 0]);
            Assert.Equal(-10.0, input.DistributedLoadTable[0, 1]);
            Assert.Equal(1.0, input.DistributedLoadTable[0, 2]);

            // Unused optional tables stay null rather than empty arrays.
            Assert.Null(input.PointLoadTable);
            Assert.Null(input.TemperatureLoadTable);
            Assert.Null(input.SettlementTable);
        }

        [Fact]
        public void ToInputData_PortalFrame_BuildsAndAnalyzes()
        {
            ProjectDocument doc = UiTestDocuments.BuildPortalFrameDocument();

            FrameAnalysisResult result = Analyze(ModelInputMapper.ToInputData(doc));

            Assert.NotEmpty(result.Reactions);
            // Horizontal equilibrium: reactions balance the 50-unit applied load.
            Assert.Equal(-50.0, result.Reactions.Sum(r => r.Fx), 3);
        }

        [Fact]
        public void ToInputData_AfterNodeReorder_PreservesConnectivity()
        {
            ProjectDocument doc = UiTestDocuments.BuildPortalFrameDocument();
            double before = Analyze(ModelInputMapper.ToInputData(doc)).Reactions.Sum(r => r.Fx);

            // Move node 1 to the end: ids change, but element/support/load references are
            // held by object, so the physical structure must be identical.
            doc.Nodes.Move(0, doc.Nodes.Count - 1);
            double after = Analyze(ModelInputMapper.ToInputData(doc)).Reactions.Sum(r => r.Fx);

            Assert.Equal(before, after, 6);
            Assert.Equal(-50.0, after, 3);
        }

        [Fact]
        public void ToInputData_ElementWithMissingNode_Throws()
        {
            ProjectDocument doc = UiTestDocuments.BuildPortalFrameDocument();
            doc.Elements[0].StartNode = null;

            Assert.Throws<InvalidOperationException>(() => ModelInputMapper.ToInputData(doc));
        }

        // --- Helpers ---

        private static FrameAnalysisResult Analyze(StructureInputData input)
        {
            StructureModel model = new StructureModelBuilder().Build(input);

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
                new StiffnessSingularityDetector());

            return analyzer.Analyze(model);
        }
    }
}
