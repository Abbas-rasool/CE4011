using System.Threading.Tasks;
using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysis.UI.Core.Mapping;
using FrameAnalysis.UI.Core.Services;
using FrameAnalysis.UI.Core.Units;
using FrameAnalysis.UI.Core.ViewModels;
using FrameAnalysisProgram.INPUT_OUTPUT;
using Xunit;

namespace FEMTestProject
{
    /// <summary>
    /// Phase 2 unit switcher: changing a display unit rescales the stored values so the
    /// canonical model fed to the solver is unchanged, and it is treated as a display change
    /// (not a model edit), so analysis results are not invalidated.
    /// </summary>
    public class UnitSwitchingTests
    {
        [Fact]
        public void SectionUnitSwitch_RescalesStoredValuesAndPreservesCanonicalModel()
        {
            ProjectDocument doc = UiTestDocuments.BuildPortalFrameDocument(); // sections stored in mm
            StructureInputData before = ModelInputMapper.ToInputData(doc);

            doc.Units.Section = SectionLengthUnit.Centimetre;

            // Stored display values rescale mm → cm (100 mm = 10 cm; 1e8 mm⁴ = 1e4 cm⁴).
            Assert.Equal(10.0, doc.Sections[0].Width, 9);
            Assert.Equal(10.0, doc.Sections[0].Depth, 9);
            Assert.Equal(1.0e4, doc.Sections[0].MomentOfInertia, 6);

            // Canonical (metre) values fed to the solver are unchanged — same physical model.
            StructureInputData after = ModelInputMapper.ToInputData(doc);
            Assert.Equal(before.SectionTable[0, 0], after.SectionTable[0, 0], 12);
            Assert.Equal(before.SectionTable[0, 1], after.SectionTable[0, 1], 12);
            Assert.Equal(before.SectionTable[0, 2], after.SectionTable[0, 2], 15);
        }

        [Fact]
        public void SettlementUnitSwitch_PreservesCanonicalSettlement()
        {
            ProjectDocument doc = UiTestDocuments.BuildPortalFrameDocument();
            doc.Settlements.Add(new SettlementRowVm { Node = doc.Nodes[0], DeltaUy = 5.0 }); // 5 mm (default)

            StructureInputData before = ModelInputMapper.ToInputData(doc);
            Assert.NotNull(before.SettlementTable);
            double canonicalBefore = before.SettlementTable![0, 2]; // dUy in m

            doc.Units.Settlement = SettlementUnit.Metre;

            Assert.Equal(0.005, doc.Settlements[0].DeltaUy, 9); // 5 mm → 0.005 m in storage

            StructureInputData after = ModelInputMapper.ToInputData(doc);
            Assert.Equal(canonicalBefore, after.SettlementTable![0, 2], 12);
        }

        [Fact]
        public void UnitSwitch_DoesNotRaiseDocumentChanged()
        {
            ProjectDocument doc = UiTestDocuments.BuildPortalFrameDocument();
            bool changed = false;
            doc.Changed += (_, _) => changed = true;

            doc.Units.Section = SectionLengthUnit.Metre;

            Assert.False(changed); // a display change, not a model edit
        }

        [Fact]
        public async Task UnitSwitch_AfterAnalysis_DoesNotInvalidateResults()
        {
            var vm = new MainViewModel(AnalysisService.CreateDefault(), UiTestDocuments.BuildPortalFrameDocument());
            await vm.RunAnalysisCommand.ExecuteAsync(null);
            Assert.True(vm.HasResult);

            vm.Document.Units.Section = SectionLengthUnit.Centimetre;

            Assert.True(vm.HasResult);
            Assert.False(vm.ResultsAreStale);
        }
    }
}
