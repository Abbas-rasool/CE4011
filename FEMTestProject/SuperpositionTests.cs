using System.Threading.Tasks;
using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysis.UI.Core.Services;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads;
using StructuralLoads;
using Xunit;

namespace FEMTestProject
{
    /// <summary>
    /// Validates the Phase 2 superposition basis: solving once per load nature and factor-combining
    /// gives exactly the same member demands as a direct solve with the loads pre-scaled by the
    /// combination factors (linear, first-order analysis).
    /// </summary>
    public class SuperpositionTests
    {
        [Fact]
        public async Task Superposition_EqualsDirectResolveWithScaledLoads()
        {
            var combo = new LoadCombinationRowVm
            {
                LimitState = eLimitState.Ultimate,
                Dead = 1.35,
                Live = 1.5,
                Wind = 0.9
            };

            // Per-nature basis from the unscaled model.
            ProjectDocument basisDoc = BuildMultiNatureFrame(1.0, 1.0, 1.0);
            var natures = LoadCombinationService.PresentNatures(basisDoc);
            SuperpositionBasis basis = await AnalysisService.CreateDefault().RunPerNatureAsync(basisDoc, natures);

            // Direct solve of the same combination (loads pre-scaled by the factors).
            ProjectDocument scaledDoc = BuildMultiNatureFrame(combo.Dead, combo.Live, combo.Wind);
            AnalysisOutcome direct = await AnalysisService.CreateDefault().RunAsync(scaledDoc);
            Assert.NotNull(direct.Result);

            foreach (var ef in direct.Result!.ElementEndForces)
            {
                double[] superposed = basis.CombinedEndForces(ef.Element.Id, combo);
                Assert.Equal(ef.LocalEndForces.Length, superposed.Length);
                for (int i = 0; i < ef.LocalEndForces.Length; i++)
                    Assert.Equal(ef.LocalEndForces[i], superposed[i], 6);
            }
        }

        [Fact]
        public async Task Superposition_IncludesSettlementAtFactorOne()
        {
            ProjectDocument doc = BuildDeadFrameWithSettlement();

            SuperpositionBasis basis = await AnalysisService.CreateDefault()
                .RunPerNatureAsync(doc, LoadCombinationService.PresentNatures(doc));
            var combo = new LoadCombinationRowVm { LimitState = eLimitState.Ultimate, Dead = 1.0 };

            // Full direct solve = dead (×1.0) + settlement (always ×1.0).
            AnalysisOutcome direct = await AnalysisService.CreateDefault().RunAsync(doc);
            Assert.NotNull(direct.Result);

            foreach (var ef in direct.Result!.ElementEndForces)
            {
                double[] superposed = basis.CombinedEndForces(ef.Element.Id, combo);
                for (int i = 0; i < ef.LocalEndForces.Length; i++)
                    Assert.Equal(ef.LocalEndForces[i], superposed[i], 6);
            }
        }

        private static ProjectDocument BuildMultiNatureFrame(double deadMult, double liveMult, double windMult)
        {
            ProjectDocument doc = BasePortal();
            doc.DistributedLoads.Add(new DistributedLoadRowVm
            { Element = doc.Elements[1], MagnitudePerLength = -5.0 * deadMult, Direction = LoadDirection.Y, Nature = eLoadNature.Dead });
            doc.DistributedLoads.Add(new DistributedLoadRowVm
            { Element = doc.Elements[1], MagnitudePerLength = -3.0 * liveMult, Direction = LoadDirection.Y, Nature = eLoadNature.Live });
            doc.NodalLoads.Add(new NodalLoadRowVm { Node = doc.Nodes[1], Fx = 4.0 * windMult, Nature = eLoadNature.Wind });
            return doc;
        }

        private static ProjectDocument BuildDeadFrameWithSettlement()
        {
            ProjectDocument doc = BasePortal();
            doc.DistributedLoads.Add(new DistributedLoadRowVm
            { Element = doc.Elements[1], MagnitudePerLength = -5.0, Direction = LoadDirection.Y, Nature = eLoadNature.Dead });
            doc.Settlements.Add(new SettlementRowVm { Node = doc.Nodes[0], DeltaUy = -5.0 }); // 5 mm support drop
            return doc;
        }

        private static ProjectDocument BasePortal()
        {
            var doc = new ProjectDocument();
            var n1 = new NodeRowVm { X = 0, Y = 0 };
            var n2 = new NodeRowVm { X = 0, Y = 3 };
            var n3 = new NodeRowVm { X = 4, Y = 3 };
            var n4 = new NodeRowVm { X = 4, Y = 0 };
            doc.Nodes.Add(n1); doc.Nodes.Add(n2); doc.Nodes.Add(n3); doc.Nodes.Add(n4);

            var mat = new MaterialRowVm { ElasticModulus = 11000 };
            doc.Materials.Add(mat);
            var sec = new SectionRowVm { Width = 150, Depth = 300, MomentOfInertia = 150.0 * 300.0 * 300.0 * 300.0 / 12.0 };
            doc.Sections.Add(sec);

            doc.Elements.Add(new ElementRowVm { StartNode = n1, EndNode = n2, Material = mat, Section = sec });
            doc.Elements.Add(new ElementRowVm { StartNode = n2, EndNode = n3, Material = mat, Section = sec });
            doc.Elements.Add(new ElementRowVm { StartNode = n4, EndNode = n3, Material = mat, Section = sec });

            doc.Supports.Add(new SupportRowVm { Node = n1, RestrainX = true, RestrainY = true, RestrainRz = true });
            doc.Supports.Add(new SupportRowVm { Node = n4, RestrainX = true, RestrainY = true, RestrainRz = true });
            return doc;
        }
    }
}
