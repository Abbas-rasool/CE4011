using System;
using System.IO;
using System.Threading.Tasks;
using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysis.UI.Core.Mapping;
using FrameAnalysis.UI.Core.Reporting;
using FrameAnalysis.UI.Core.Services;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.ANALYSIS_CORE.Validation;
using FrameAnalysisProgram.INPUT_OUTPUT;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using MemberDesigner.Designers;
using Matrix_Library.SOLVERS;
using Xunit;
using static MemberDesigner.Designers.Enums;

namespace FEMTestProject
{
    /// <summary>
    /// End-to-end check of the PDF design-report generator: build a model, analyze, design, then
    /// generate the report for each code and confirm a real, non-empty PDF is produced. Also
    /// exercises QuestPDF's native (SkiaSharp) rendering path at runtime.
    /// </summary>
    public class DesignReportTests
    {
        [Theory]
        [InlineData(eTimberCode.EC5)]
        [InlineData(eTimberCode.US)]
        [InlineData(eTimberCode.TR)]
        public async Task Generate_ProducesNonEmptyPdf(eTimberCode code)
        {
            ProjectDocument doc = BuildCantilever(code);
            FrameAnalysisResult result = Analyze(doc);
            DesignOutcome outcome = await new DesignService().RunAsync(doc, result);

            Assert.False(outcome.Fatal);
            Assert.NotEmpty(outcome.Results);

            string path = Path.Combine(Path.GetTempPath(), $"design_report_{code}_{Guid.NewGuid():N}.pdf");
            try
            {
                DesignReportGenerator.Save(path, doc, outcome);

                Assert.True(File.Exists(path), "report file was not created");
                byte[] bytes = File.ReadAllBytes(path);
                Assert.True(bytes.Length > 1000, $"report looks too small ({bytes.Length} bytes)");

                // PDF magic number "%PDF"
                Assert.Equal((byte)'%', bytes[0]);
                Assert.Equal((byte)'P', bytes[1]);
                Assert.Equal((byte)'D', bytes[2]);
                Assert.Equal((byte)'F', bytes[3]);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Theory]
        [InlineData(eTimberCode.EC5)]
        [InlineData(eTimberCode.US)]
        [InlineData(eTimberCode.TR)]
        public async Task EveryProducedCheck_HasTitleAndSummary(eTimberCode code)
        {
            ProjectDocument doc = BuildCantilever(code);
            FrameAnalysisResult result = Analyze(doc);
            DesignOutcome outcome = await new DesignService().RunAsync(doc, result);

            MemberDesignResult member = Assert.Single(outcome.Results);
            Assert.NotEmpty(member.Checks);
            foreach (CheckResult check in member.Checks)
            {
                Assert.False(string.IsNullOrWhiteSpace(check.Title), $"{code}/{check.CheckType}: empty title");
                Assert.False(string.IsNullOrWhiteSpace(check.Summary), $"{code}/{check.CheckType}: empty summary");
            }
        }

        private static ProjectDocument BuildCantilever(eTimberCode code)
        {
            var doc = new ProjectDocument { ProjectName = "Report Test" };
            doc.Design.Code = code;

            var n1 = new NodeRowVm { X = 0, Y = 0 };
            var n2 = new NodeRowVm { X = 1.0, Y = 0 };
            doc.Nodes.Add(n1);
            doc.Nodes.Add(n2);

            var mat = new MaterialRowVm();
            if (code == eTimberCode.US) mat.SpeciesGrade = eNdsSpeciesGrade.DouglasFirLarch_No2;
            else mat.StrengthClass = eStrengthClass.C24;
            doc.Materials.Add(mat);

            var sec = new SectionRowVm { Width = 100, Depth = 200, MomentOfInertia = 100 * 200 * 200 * 200 / 12.0 };
            doc.Sections.Add(sec);

            doc.Elements.Add(new ElementRowVm { StartNode = n1, EndNode = n2, Material = mat, Section = sec });
            doc.Supports.Add(new SupportRowVm { Node = n1, RestrainX = true, RestrainY = true, RestrainRz = true });
            doc.NodalLoads.Add(new NodalLoadRowVm { Node = n2, Fy = -3.0 }); // 3 kN downward tip load

            MemberDesignRowVm md = doc.MemberDesigns[0];
            md.EffectiveLengthMajor = 1.0;
            md.EffectiveLengthMinor = 1.0;
            md.EffectiveBeamLength = 1.0;
            return doc;
        }

        private static FrameAnalysisResult Analyze(ProjectDocument doc)
        {
            StructureInputData input = ModelInputMapper.ToInputData(doc);
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
                new StiffnessSingularityDetector(),
                new SectionForceRecovery());

            return analyzer.Analyze(model);
        }
    }
}
