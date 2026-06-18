using System.Linq;
using System.Threading.Tasks;
using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysis.UI.Core.Rendering;
using FrameAnalysis.UI.Core.Services;
using FrameAnalysis.UI.Core.ViewModels;
using Xunit;

namespace FEMTestProject
{
    public class MainViewModelTests
    {
        [Fact]
        public void Constructor_BuildsInitialSceneFromDocument()
        {
            var vm = new MainViewModel(AnalysisService.CreateDefault(), UiTestDocuments.BuildPortalFrameDocument());

            Assert.Equal(3, vm.CurrentScene.Members.Count);
            Assert.False(vm.HasResult);
            Assert.Null(vm.LastOutcome);
        }

        [Fact]
        public void DocumentEdit_RebuildsSceneLive()
        {
            ProjectDocument doc = UiTestDocuments.BuildPortalFrameDocument();
            var vm = new MainViewModel(AnalysisService.CreateDefault(), doc);

            doc.Nodes.Add(new NodeRowVm { X = 10, Y = 10 });

            Assert.Equal(5, vm.CurrentScene.Nodes.Count);
        }

        [Fact]
        public async Task RunAnalysis_PopulatesResultAndDeflectedOverlay()
        {
            var vm = new MainViewModel(AnalysisService.CreateDefault(), UiTestDocuments.BuildPortalFrameDocument());

            await vm.RunAnalysisCommand.ExecuteAsync(null);

            Assert.NotNull(vm.LastOutcome);
            Assert.NotNull(vm.LastOutcome!.Result);
            Assert.True(vm.HasResult);
            Assert.False(vm.ResultsAreStale);
            Assert.False(vm.IsBusy);
            Assert.NotEmpty(vm.CurrentScene.DeflectedMembers);
            Assert.NotEmpty(vm.CurrentScene.Reactions);
        }

        [Fact]
        public async Task EditAfterRun_MarksStaleAndDropsOverlayButKeepsNumbers()
        {
            ProjectDocument doc = UiTestDocuments.BuildPortalFrameDocument();
            var vm = new MainViewModel(AnalysisService.CreateDefault(), doc);
            await vm.RunAnalysisCommand.ExecuteAsync(null);
            Assert.True(vm.HasResult);

            doc.Nodes[1].X += 0.5; // edit a node after solving

            Assert.True(vm.ResultsAreStale);
            Assert.False(vm.HasResult);
            Assert.NotNull(vm.LastOutcome);                 // numbers retained for the results panel
            Assert.Empty(vm.CurrentScene.DeflectedMembers); // overlay dropped (would be wrong now)
        }

        [Fact]
        public async Task DeflectionScaleChange_RescalesDeflectedShape()
        {
            var vm = new MainViewModel(AnalysisService.CreateDefault(), UiTestDocuments.BuildPortalFrameDocument());
            await vm.RunAnalysisCommand.ExecuteAsync(null);

            var atScaleOne = vm.CurrentScene.DeflectedMembers.SelectMany(m => m.Points).ToList();
            vm.DeflectionScale = 1000.0;
            var atScaleThousand = vm.CurrentScene.DeflectedMembers.SelectMany(m => m.Points).ToList();

            Assert.False(atScaleOne.SequenceEqual(atScaleThousand));
        }

        [Fact]
        public async Task OpenMemberResult_AfterRun_RaisesRequestWithSelectedMemberStations()
        {
            var vm = new MainViewModel(AnalysisService.CreateDefault(), UiTestDocuments.BuildPortalFrameDocument());
            await vm.RunAnalysisCommand.ExecuteAsync(null);

            FrameAnalysisProgram.ANALYSIS_CORE.MemberStationResult? requested = null;
            vm.MemberResultRequested += (stations, _) => requested = stations;

            vm.SelectedElementId = 2;             // the beam (set by the list/canvas selection)
            vm.OpenMemberResultCommand.Execute(null);

            Assert.NotNull(requested);
            Assert.Equal(2, requested!.ElementId);
        }

        [Fact]
        public void OpenMemberResult_WithoutResult_DoesNothing()
        {
            var vm = new MainViewModel(AnalysisService.CreateDefault(), UiTestDocuments.BuildPortalFrameDocument());
            bool raised = false;
            vm.MemberResultRequested += (_, _) => raised = true;

            vm.SelectedElementId = 2;
            vm.OpenMemberResultCommand.Execute(null); // no analysis run yet

            Assert.False(raised);
        }

        [Fact]
        public void ShowSheet_ChangesCurrentSheet()
        {
            var vm = new MainViewModel(AnalysisService.CreateDefault());
            Assert.Equal(Sheet.Nodes, vm.CurrentSheet); // default

            vm.ShowSheetCommand.Execute(Sheet.Elements);

            Assert.Equal(Sheet.Elements, vm.CurrentSheet);
        }

        [Fact]
        public void AddRow_AppendsToCurrentSheetAndRebuildsScene()
        {
            var vm = new MainViewModel(AnalysisService.CreateDefault(), UiTestDocuments.BuildPortalFrameDocument());

            vm.ShowSheetCommand.Execute(Sheet.Elements);
            int before = vm.Document.Elements.Count;
            int membersBefore = vm.CurrentScene.Members.Count;

            vm.AddRowCommand.Execute(null);

            Assert.Equal(before + 1, vm.Document.Elements.Count);
            // The new element has no nodes yet, so it isn't drawn — but wiring it up will be
            // tracked live (the collection add already triggered a rebuild without throwing).
            Assert.Equal(membersBefore, vm.CurrentScene.Members.Count);

            // Completing the element makes it appear on the canvas.
            ElementRowVm added = vm.Document.Elements[^1];
            added.StartNode = vm.Document.Nodes[0];
            added.EndNode = vm.Document.Nodes[2];
            Assert.Equal(membersBefore + 1, vm.CurrentScene.Members.Count);
        }

        [Fact]
        public void AddRow_DisabledOnNonDataSheets()
        {
            var vm = new MainViewModel(AnalysisService.CreateDefault());

            vm.ShowSheetCommand.Execute(Sheet.Results);
            Assert.False(vm.AddRowCommand.CanExecute(null));

            vm.ShowSheetCommand.Execute(Sheet.Nodes);
            Assert.True(vm.AddRowCommand.CanExecute(null));
        }

        [Fact]
        public async Task RunAnalysis_UnstableModel_FatalWithNoOverlay()
        {
            var vm = new MainViewModel(AnalysisService.CreateDefault(), UiTestDocuments.BuildUnstableNoSupportsDocument());

            await vm.RunAnalysisCommand.ExecuteAsync(null);

            Assert.NotNull(vm.LastOutcome);
            Assert.True(vm.LastOutcome!.Fatal);
            Assert.Null(vm.LastOutcome.Result);
            Assert.False(vm.HasResult);
            Assert.Empty(vm.CurrentScene.DeflectedMembers);
        }
    }
}
