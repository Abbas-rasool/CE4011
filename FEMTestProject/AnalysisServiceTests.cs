using System.Threading.Tasks;
using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Services;
using FrameAnalysisProgram.ANALYSIS_CORE.Validation;
using Xunit;

namespace FEMTestProject
{
    public class AnalysisServiceTests
    {
        [Fact]
        public async Task RunAsync_StablePortalFrame_Succeeds()
        {
            ProjectDocument doc = UiTestDocuments.BuildPortalFrameDocument();

            AnalysisOutcome outcome = await AnalysisService.CreateDefault().RunAsync(doc);

            Assert.False(outcome.Fatal);
            Assert.NotNull(outcome.Result);
            Assert.NotEmpty(outcome.Result!.Reactions);
        }

        [Fact]
        public async Task RunAsync_UnstableModel_ReturnsFatalWithoutThrowing()
        {
            ProjectDocument doc = UiTestDocuments.BuildUnstableNoSupportsDocument();

            AnalysisOutcome outcome = await AnalysisService.CreateDefault().RunAsync(doc);

            Assert.True(outcome.Fatal);
            Assert.Null(outcome.Result);
            Assert.Contains(outcome.Messages, m => m.Severity == ValidationSeverity.Error);
        }

        [Fact]
        public async Task RunAsync_UnassignedReference_ReturnsFatalMappingError()
        {
            ProjectDocument doc = UiTestDocuments.BuildPortalFrameDocument();
            doc.Elements[0].StartNode = null; // user hasn't finished the row

            AnalysisOutcome outcome = await AnalysisService.CreateDefault().RunAsync(doc);

            Assert.True(outcome.Fatal);
            Assert.Null(outcome.Result);
            Assert.NotEmpty(outcome.Messages);
        }
    }
}
