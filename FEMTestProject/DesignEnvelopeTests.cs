using System.Linq;
using System.Threading.Tasks;
using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysis.UI.Core.Services;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads;
using StructuralLoads;
using Xunit;
using static MemberDesigner.Designers.Enums;

namespace FEMTestProject
{
    public class DesignEnvelopeTests
    {
        [Fact]
        public void GoverningDuration_ShortestParticipatingActionGoverns()
        {
            Assert.Equal(eLoadDurationClass.PermanentAction,
                LoadDurationMap.GoverningDuration(new LoadCombinationRowVm { Dead = 1.35 }));
            Assert.Equal(eLoadDurationClass.MediumTermAction,
                LoadDurationMap.GoverningDuration(new LoadCombinationRowVm { Dead = 1.35, Live = 1.5 }));
            Assert.Equal(eLoadDurationClass.ShortTermAction,
                LoadDurationMap.GoverningDuration(new LoadCombinationRowVm { Dead = 1.2, Wind = 1.5 }));
            Assert.Equal(eLoadDurationClass.InstantaneousAction,
                LoadDurationMap.GoverningDuration(new LoadCombinationRowVm { Dead = 1.0, Seismic = 1.0 }));
        }

        [Fact]
        public async Task Envelope_GovernsByWorstCombination()
        {
            ProjectDocument doc = BasePortal();
            doc.DistributedLoads.Add(new DistributedLoadRowVm
            { Element = doc.Elements[1], MagnitudePerLength = -5, Direction = LoadDirection.Y, Nature = eLoadNature.Dead });
            doc.DistributedLoads.Add(new DistributedLoadRowVm
            { Element = doc.Elements[1], MagnitudePerLength = -4, Direction = LoadDirection.Y, Nature = eLoadNature.Live });
            doc.LoadCombinations.Add(new LoadCombinationRowVm { Name = "light", LimitState = eLimitState.Ultimate, Dead = 1.0 });
            doc.LoadCombinations.Add(new LoadCombinationRowVm { Name = "heavy", LimitState = eLimitState.Ultimate, Dead = 1.4, Live = 1.6 });

            DesignOutcome outcome = await RunEnvelope(doc);

            Assert.False(outcome.Fatal);
            MemberDesignResult beam = outcome.Results.First(r => r.ElementId == 2);
            Assert.True(beam.GoverningUtilization > 0 && double.IsFinite(beam.GoverningUtilization));
            Assert.Equal("heavy", beam.GoverningCombination); // the heavier combo governs the rafter
        }

        [Fact]
        public async Task Envelope_ShorterDurationGivesLowerUtilization()
        {
            // Identical physical load, tagged Dead (permanent) vs Wind (short-term). Same member
            // forces, but the shorter duration earns a higher kmod → lower utilization.
            double deadUtil = await RafterUtilization(eLoadNature.Dead);
            double windUtil = await RafterUtilization(eLoadNature.Wind);

            Assert.True(windUtil < deadUtil, $"wind (short) util {windUtil} should be < dead (permanent) util {deadUtil}");
        }

        private static async Task<double> RafterUtilization(eLoadNature nature)
        {
            ProjectDocument doc = BasePortal();
            doc.DistributedLoads.Add(new DistributedLoadRowVm
            { Element = doc.Elements[1], MagnitudePerLength = -6, Direction = LoadDirection.Y, Nature = nature });
            doc.LoadCombinations.Add(new LoadCombinationRowVm
            { Name = nature.ToString(), LimitState = eLimitState.Ultimate, Dead = nature == eLoadNature.Dead ? 1.0 : 0.0, Wind = nature == eLoadNature.Wind ? 1.0 : 0.0 });

            DesignOutcome outcome = await RunEnvelope(doc);
            return outcome.Results.First(r => r.ElementId == 2).GoverningUtilization;
        }

        [Fact]
        public void NdsDurationFactors_ShortestActionGoverns()
        {
            // C_D (ASD)
            Assert.Equal(0.9, LoadDurationMap.GoverningLoadDurationFactor(new LoadCombinationRowVm { Dead = 1.0 }));
            Assert.Equal(1.6, LoadDurationMap.GoverningLoadDurationFactor(new LoadCombinationRowVm { Dead = 1.0, Wind = 0.6 }));
            Assert.Equal(1.15, LoadDurationMap.GoverningLoadDurationFactor(new LoadCombinationRowVm { Dead = 1.0, Snow = 0.7 }));
            // λ (LRFD)
            Assert.Equal(0.6, LoadDurationMap.GoverningTimeEffectFactor(new LoadCombinationRowVm { Dead = 1.4 }));
            Assert.Equal(1.0, LoadDurationMap.GoverningTimeEffectFactor(new LoadCombinationRowVm { Dead = 1.2, Wind = 1.0 }));
        }

        [Fact]
        public async Task UsAsd_ShorterDurationGivesLowerUtilization()
        {
            // Identical physical load tagged Dead (C_D = 0.9) vs Wind (C_D = 1.6). Same forces;
            // the higher C_D raises the allowable stress → lower utilization.
            double deadUtil = await UsRafterUtilization(eLoadNature.Dead);
            double windUtil = await UsRafterUtilization(eLoadNature.Wind);

            Assert.True(windUtil < deadUtil, $"wind (C_D=1.6) util {windUtil} should be < dead (C_D=0.9) util {deadUtil}");
        }

        [Theory]
        [InlineData(eLoadCombinationType.ASD)]
        [InlineData(eLoadCombinationType.LRFD)]
        public async Task UsChecks_CompressionAndCombinedNowEvaluate(eLoadCombinationType method)
        {
            // Compression & CombinedBendingAxial are now implemented for US (NDS): finite ratios,
            // no longer skipped or reported as a spurious 0% Fail, for both ASD and LRFD.
            ProjectDocument doc = UsPortal();
            doc.Design.UsDesignMethod = method;
            doc.DistributedLoads.Add(new DistributedLoadRowVm
            { Element = doc.Elements[1], MagnitudePerLength = -6, Direction = LoadDirection.Y, Nature = eLoadNature.Dead });
            doc.LoadCombinations.Add(new LoadCombinationRowVm { Name = "C1", LimitState = eLimitState.Ultimate, Dead = method == eLoadCombinationType.LRFD ? 1.4 : 1.0 });

            DesignOutcome outcome = await RunEnvelope(doc);

            MemberDesignResult beam = outcome.Results.First(r => r.ElementId == 2);
            Assert.Contains(beam.Checks, c => c.CheckType == eTimberDesignCheckType.Compression);
            Assert.Contains(beam.Checks, c => c.CheckType == eTimberDesignCheckType.CombinedBendingAxial);
            Assert.All(beam.Checks, c => Assert.True(double.IsFinite(c.Utilization) && c.Utilization >= 0));
            Assert.Equal(eDesignStatus.Pass, beam.GoverningStatus); // modest load → passes, no phantom Fail
            Assert.DoesNotContain(outcome.Messages, m => m.Message.Contains("Not evaluated"));
        }

        private static async Task<double> UsRafterUtilization(eLoadNature nature)
        {
            ProjectDocument doc = UsPortal();
            doc.DistributedLoads.Add(new DistributedLoadRowVm
            { Element = doc.Elements[1], MagnitudePerLength = -6, Direction = LoadDirection.Y, Nature = nature });
            doc.LoadCombinations.Add(new LoadCombinationRowVm
            {
                Name = nature.ToString(),
                LimitState = eLimitState.Ultimate,
                Dead = nature == eLoadNature.Dead ? 1.0 : 0.0,
                Wind = nature == eLoadNature.Wind ? 1.0 : 0.0
            });

            DesignOutcome outcome = await RunEnvelope(doc);
            return outcome.Results.First(r => r.ElementId == 2).GoverningUtilization;
        }

        private static ProjectDocument UsPortal()
        {
            ProjectDocument doc = BasePortal();
            doc.Design.Code = eTimberCode.US;
            doc.Design.UsDesignMethod = eLoadCombinationType.ASD;
            doc.Materials[0].SpeciesGrade = eNdsSpeciesGrade.DouglasFirLarch_No2; // NDS values + modulus
            return doc;
        }

        [Theory]
        [InlineData(eTimberCode.EC5, eLoadCombinationType.ASD)]
        [InlineData(eTimberCode.TR, eLoadCombinationType.ASD)]
        [InlineData(eTimberCode.US, eLoadCombinationType.ASD)]
        [InlineData(eTimberCode.US, eLoadCombinationType.LRFD)]
        public async Task AllFiveChecksEvaluateForEveryCode(eTimberCode code, eLoadCombinationType method)
        {
            // Every code must produce all five capacity checks with finite ratios — none skipped
            // (NotImplemented) or dropped (non-finite, e.g. the TS C_P NaN that hid TR's combined).
            ProjectDocument doc = code == eTimberCode.US ? UsPortal() : BasePortal();
            doc.Design.Code = code;
            doc.Design.UsDesignMethod = method;
            doc.DistributedLoads.Add(new DistributedLoadRowVm
            { Element = doc.Elements[1], MagnitudePerLength = -6, Direction = LoadDirection.Y, Nature = eLoadNature.Dead });
            doc.LoadCombinations.Add(new LoadCombinationRowVm
            { Name = "C", LimitState = eLimitState.Ultimate, Dead = method == eLoadCombinationType.LRFD ? 1.4 : 1.0 });

            DesignOutcome outcome = await RunEnvelope(doc);

            Assert.DoesNotContain(outcome.Messages, m => m.Message.Contains("Not evaluated"));
            Assert.NotEmpty(outcome.Results);
            foreach (MemberDesignResult member in outcome.Results)
            {
                var types = member.Checks.Select(c => c.CheckType).ToList();
                Assert.Contains(eTimberDesignCheckType.Compression, types);
                Assert.Contains(eTimberDesignCheckType.Tension, types);
                Assert.Contains(eTimberDesignCheckType.Bending, types);
                Assert.Contains(eTimberDesignCheckType.Shear, types);
                Assert.Contains(eTimberDesignCheckType.CombinedBendingAxial, types);
                Assert.All(member.Checks, c => Assert.True(double.IsFinite(c.Utilization) && c.Utilization >= 0));
            }
        }

        private static async Task<DesignOutcome> RunEnvelope(ProjectDocument doc)
        {
            var natures = LoadCombinationService.PresentNatures(doc);
            SuperpositionBasis basis = await AnalysisService.CreateDefault().RunPerNatureAsync(doc, natures);
            var uls = doc.LoadCombinations.Where(c => c.IsUltimate).ToList();
            return await new DesignService().RunEnvelopeAsync(doc, basis, uls);
        }

        private static ProjectDocument BasePortal()
        {
            var doc = new ProjectDocument(); // EC5 by default
            var n1 = new NodeRowVm { X = 0, Y = 0 };
            var n2 = new NodeRowVm { X = 0, Y = 3 };
            var n3 = new NodeRowVm { X = 4, Y = 3 };
            var n4 = new NodeRowVm { X = 4, Y = 0 };
            doc.Nodes.Add(n1); doc.Nodes.Add(n2); doc.Nodes.Add(n3); doc.Nodes.Add(n4);

            var mat = new MaterialRowVm { Name = "C24", StrengthClass = eStrengthClass.C24 };
            doc.Materials.Add(mat);
            var sec = new SectionRowVm { Width = 150, Depth = 300, MomentOfInertia = 150.0 * 300.0 * 300.0 * 300.0 / 12.0 };
            doc.Sections.Add(sec);

            doc.Elements.Add(new ElementRowVm { StartNode = n1, EndNode = n2, Material = mat, Section = sec });
            doc.Elements.Add(new ElementRowVm { StartNode = n2, EndNode = n3, Material = mat, Section = sec });
            doc.Elements.Add(new ElementRowVm { StartNode = n4, EndNode = n3, Material = mat, Section = sec });

            doc.Supports.Add(new SupportRowVm { Node = n1, RestrainX = true, RestrainY = true, RestrainRz = true });
            doc.Supports.Add(new SupportRowVm { Node = n4, RestrainX = true, RestrainY = true, RestrainRz = true });

            doc.MemberDesigns[0].EffectiveLengthMajor = doc.MemberDesigns[0].EffectiveLengthMinor = doc.MemberDesigns[0].EffectiveBeamLength = 3;
            doc.MemberDesigns[1].EffectiveLengthMajor = doc.MemberDesigns[1].EffectiveLengthMinor = doc.MemberDesigns[1].EffectiveBeamLength = 4;
            doc.MemberDesigns[2].EffectiveLengthMajor = doc.MemberDesigns[2].EffectiveLengthMinor = doc.MemberDesigns[2].EffectiveBeamLength = 3;
            return doc;
        }
    }
}
