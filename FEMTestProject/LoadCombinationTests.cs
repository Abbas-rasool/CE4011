using System.Collections.Generic;
using System.Linq;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysis.UI.Core.Services;
using FrameAnalysis.UI.Core.ViewModels;
using StructuralLoads;
using Xunit;
using static MemberDesigner.Designers.Enums;

namespace FEMTestProject
{
    public class LoadCombinationTests
    {
        [Fact]
        public void PresentNatures_ReflectsTheLoadsInUse()
        {
            var doc = UiTestDocuments.BuildPortalFrameDocument(); // dead loads only

            var natures = LoadCombinationService.PresentNatures(doc);
            Assert.Contains(eLoadNature.Dead, natures);
            Assert.DoesNotContain(eLoadNature.Wind, natures);

            doc.NodalLoads.Add(new NodalLoadRowVm { Node = doc.Nodes[0], Fx = 5, Nature = eLoadNature.Wind });
            Assert.Contains(eLoadNature.Wind, LoadCombinationService.PresentNatures(doc));
        }

        [Theory]
        [InlineData(eTimberCode.EC5, eLoadCode.EN1990)]
        [InlineData(eTimberCode.TR, eLoadCode.TBDY)]
        [InlineData(eTimberCode.US, eLoadCode.ASCE7)]
        public void ToLoadCode_MapsMaterialCodeToLoadStandard(eTimberCode code, eLoadCode expected)
            => Assert.Equal(expected, LoadCombinationService.ToLoadCode(code));

        [Fact]
        public void FromCombination_CopiesNameLimitStateAndFactors()
        {
            var combo = new LoadCombination("EN 6.10", eLoadCode.EN1990, eLimitState.Ultimate,
                new Dictionary<eLoadNature, double> { [eLoadNature.Dead] = 1.35, [eLoadNature.Live] = 1.5 });

            var row = LoadCombinationRowVm.FromCombination(combo);

            Assert.Equal("EN 6.10", row.Name);
            Assert.True(row.IsUltimate);
            Assert.Equal(1.35, row.Dead);
            Assert.Equal(1.5, row.Live);
            Assert.Equal(0.0, row.Wind);
            Assert.Equal(1.35, row.FactorFor(eLoadNature.Dead));
            Assert.Equal(1.5, row.FactorFor(eLoadNature.Live));
        }

        [Fact]
        public void IsUltimate_TrueForStrengthStatesOnly()
        {
            Assert.True(new LoadCombinationRowVm { LimitState = eLimitState.Ultimate }.IsUltimate);
            Assert.True(new LoadCombinationRowVm { LimitState = eLimitState.UltimateSeismic }.IsUltimate);
            Assert.False(new LoadCombinationRowVm { LimitState = eLimitState.ServiceabilityCharacteristic }.IsUltimate);
        }

        [Fact]
        public void Generate_ProducesUltimateCombosForThePresentNatures()
        {
            var doc = UiTestDocuments.BuildPortalFrameDocument(); // EC5 default, dead present

            var combos = new LoadCombinationService().Generate(doc);

            Assert.NotEmpty(combos);
            Assert.Contains(combos, c => c.LimitState == eLimitState.Ultimate && c.FactorFor(eLoadNature.Dead) > 1.0);
        }

        [Fact]
        public void GenerateCommand_PopulatesAndReplacesTheEditableCollection()
        {
            var vm = new MainViewModel(AnalysisService.CreateDefault(), UiTestDocuments.BuildPortalFrameDocument());

            vm.GenerateLoadCombinationsCommand.Execute(null);

            Assert.NotEmpty(vm.Document.LoadCombinations);
            Assert.Contains(vm.Document.LoadCombinations, r => r.IsUltimate && r.Dead > 1.0);

            int count = vm.Document.LoadCombinations.Count;
            vm.GenerateLoadCombinationsCommand.Execute(null); // regenerate
            Assert.Equal(count, vm.Document.LoadCombinations.Count); // replaced, not appended
        }
    }
}
