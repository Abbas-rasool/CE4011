using System.Threading.Tasks;
using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysis.UI.Core.Mapping;
using FrameAnalysis.UI.Core.Services;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.ANALYSIS_CORE.Validation;
using FrameAnalysisProgram.INPUT_OUTPUT;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using MemberDesigner.Designers;
using MemberDesigner.TimberMaterialData;
using Matrix_Library.SOLVERS;
using Xunit;
using static MemberDesigner.Designers.Enums;

namespace FEMTestProject
{
    /// <summary>
    /// Covers the design wiring: the material database, the demand mapping from analysis
    /// results, and the end-to-end design service for each code. The document is built in the
    /// UI's units (coordinates/spans in m, section dims in mm, forces in kN, strengths in MPa);
    /// the mappers convert to the design backend's { N, mm, MPa }, which the assertions check.
    /// </summary>
    public class DesignTests
    {
        private const double PsiToMpa = 0.00689476;

        // --- Material database ---

        [Fact]
        public void Database_EnC24_ReturnsPublishedValues()
        {
            EnStrengthProperties c24 = TimberMaterialDatabase.GetEn(eStrengthClass.C24);

            Assert.Equal(24f, c24.Fmk);
            Assert.Equal(4.0f, c24.Fvk);
            Assert.Equal(21f, c24.Fc0k);
            Assert.Equal(11000f, c24.E0Mean);
            Assert.Equal(7400f, c24.E005);
            Assert.Equal(350f, c24.RhoK);
        }

        [Fact]
        public void Database_NdsDouglasFirNo2_ConvertsPsiToMpa()
        {
            NdsReferenceValues dfl = TimberMaterialDatabase.GetNds(eNdsSpeciesGrade.DouglasFirLarch_No2);

            Assert.Equal(900 * PsiToMpa, dfl.Fb, 2);
            Assert.Equal(1_600_000 * PsiToMpa, dfl.E, 0);
        }

        [Fact]
        public void Material_PickingStrengthClass_AutoFillsAndSyncsModulus()
        {
            var mat = new MaterialRowVm { StrengthClass = eStrengthClass.C24 };

            Assert.Equal(24.0, mat.BendingStrength);
            Assert.Equal(11000.0, mat.ModulusMean);
            Assert.Equal(11000.0, mat.ElasticModulus); // analysis modulus stays in sync
        }

        // --- Demand mapping ---

        [Fact]
        public void Mapper_CantileverTipLoad_MapsMomentShearAndGeometry()
        {
            ProjectDocument doc = BuildTimberCantilever(eTimberCode.EC5, enClass: eStrengthClass.C24, transverseLoadN: 1000);
            FrameAnalysisResult result = Analyze(doc);

            TimberMemberDesignContext ctx =
                DesignInputMapper.BuildContext(doc.MemberDesigns[0], 1, doc.Design, result, doc.Units);

            // Cantilever L=1000 mm, tip load 1000 N: M_fixed = P*L, V = P.
            Assert.Equal(1_000_000.0, ctx.MomentMajor, 0);
            Assert.Equal(1000.0, ctx.Shear, 0);
            Assert.Equal(0f, ctx.MomentMinor);
            Assert.True(ctx.AxialTension < 1f && ctx.AxialCompression < 1f);

            // Geometry: major axis = depth (200), minor = width (100).
            Assert.Equal(200f, ctx.H1);
            Assert.Equal(100f, ctx.H2);
            Assert.Equal(20000f, ctx.GrossArea);
            Assert.Equal(24f, ctx.BendingStrength); // resolved from C24
        }

        [Fact]
        public void Mapper_AxialTensionLoad_RoutesToTensionDemand()
        {
            ProjectDocument doc = BuildAxialBar(eStrengthClass.C24, axialLoadN: 2000);
            FrameAnalysisResult result = Analyze(doc);

            TimberMemberDesignContext ctx =
                DesignInputMapper.BuildContext(doc.MemberDesigns[0], 1, doc.Design, result, doc.Units);

            Assert.True(ctx.AxialTension > 1000f, $"Expected tension demand, got T={ctx.AxialTension}, C={ctx.AxialCompression}");
            Assert.Equal(0f, ctx.AxialCompression);
        }

        [Fact]
        public void Mapper_UdlBeam_UsesMidspanMomentEnvelope_NotEndForces()
        {
            const double L = 4.0, w = 5.0; // span (m), UDL intensity (kN/m, downward)
            ProjectDocument doc = BuildSimplySupportedUdlBeam(L, w);
            FrameAnalysisResult result = AnalyzeWithStations(doc);

            TimberMemberDesignContext ctx =
                DesignInputMapper.BuildContext(doc.MemberDesigns[0], 1, doc.Design, result, doc.Units);

            // The simply-supported end moments are ~0; the station envelope captures the
            // mid-span peak wL^2/8 (= 10 kN·m → 1e7 N·mm).
            double expectedNmm = w * L * L / 8.0 * 1e6;
            Assert.True(System.Math.Abs(ctx.MomentMajor - expectedNmm) <= 1e-2 * expectedNmm,
                $"Expected mid-span envelope ~{expectedNmm:G6} N·mm, got {ctx.MomentMajor:G6}.");
        }

        // --- End-to-end design service per code ---

        [Theory]
        [InlineData(eTimberCode.EC5)]
        [InlineData(eTimberCode.TR)]
        public async Task DesignService_EnCode_ProducesFiniteUtilization(eTimberCode code)
        {
            ProjectDocument doc = BuildTimberCantilever(code, enClass: eStrengthClass.C24, transverseLoadN: 3000);
            DesignOutcome outcome = await RunDesign(doc);

            Assert.False(outcome.Fatal);
            MemberDesignResult member = Assert.Single(outcome.Results);
            Assert.NotEmpty(member.Checks);
            Assert.True(member.GoverningUtilization > 0);
            Assert.True(double.IsFinite(member.GoverningUtilization));
        }

        [Fact]
        public async Task DesignService_UsCode_ProducesFiniteUtilization()
        {
            ProjectDocument doc = BuildTimberCantilever(eTimberCode.US, usGrade: eNdsSpeciesGrade.DouglasFirLarch_No2, transverseLoadN: 3000);
            DesignOutcome outcome = await RunDesign(doc);

            Assert.False(outcome.Fatal);
            MemberDesignResult member = Assert.Single(outcome.Results);
            Assert.NotEmpty(member.Checks);
            Assert.True(double.IsFinite(member.GoverningUtilization));
        }

        [Fact]
        public async Task DesignService_HigherLoad_RaisesUtilization()
        {
            double low = (await RunDesign(BuildTimberCantilever(eTimberCode.EC5, enClass: eStrengthClass.C24, transverseLoadN: 1000)))
                .Results[0].GoverningUtilization;
            double high = (await RunDesign(BuildTimberCantilever(eTimberCode.EC5, enClass: eStrengthClass.C24, transverseLoadN: 4000)))
                .Results[0].GoverningUtilization;

            Assert.True(high > low, $"Expected utilization to grow with load: low={low}, high={high}");
        }

        [Fact]
        public async Task DesignService_NoGradeSelected_FlagsMissingDesignValues()
        {
            ProjectDocument doc = BuildTimberCantilever(eTimberCode.EC5, transverseLoadN: 1000); // no grade
            DesignOutcome outcome = await RunDesign(doc);

            Assert.Contains(outcome.Messages, m => m.Message.Contains("missing design values"));
            Assert.Empty(outcome.Results); // ungraded member is skipped, not designed with zeros
        }

        [Fact]
        public async Task DesignService_WithoutAnalysis_IsFatal()
        {
            ProjectDocument doc = BuildTimberCantilever(eTimberCode.EC5, enClass: eStrengthClass.C24, transverseLoadN: 1000);

            DesignOutcome outcome = await new DesignService().RunAsync(doc, analysisResult: null);

            Assert.True(outcome.Fatal);
            Assert.Empty(outcome.Results);
        }

        // --- Helpers ---

        private static Task<DesignOutcome> RunDesign(ProjectDocument doc)
        {
            FrameAnalysisResult result = Analyze(doc);
            return new DesignService().RunAsync(doc, result);
        }

        /// <summary>Horizontal cantilever (fixed at n1, free at n2) with a transverse tip load.</summary>
        private static ProjectDocument BuildTimberCantilever(
            eTimberCode code,
            eStrengthClass? enClass = null,
            eNdsSpeciesGrade? usGrade = null,
            double transverseLoadN = 1000)
        {
            var doc = new ProjectDocument();
            doc.Design.Code = code;

            var n1 = new NodeRowVm { X = 0, Y = 0 };
            var n2 = new NodeRowVm { X = 1.0, Y = 0 }; // 1 m cantilever (coords in m)
            doc.Nodes.Add(n1);
            doc.Nodes.Add(n2);

            var mat = new MaterialRowVm();
            if (enClass is not null) mat.StrengthClass = enClass;
            if (usGrade is not null) mat.SpeciesGrade = usGrade;
            if (mat.ElasticModulus <= 0) mat.ElasticModulus = 11000; // ensure a solvable model when ungraded
            doc.Materials.Add(mat);

            var sec = new SectionRowVm { Width = 100, Depth = 200, MomentOfInertia = 100 * 200 * 200 * 200 / 12.0 };
            doc.Sections.Add(sec);

            doc.Elements.Add(new ElementRowVm { StartNode = n1, EndNode = n2, Material = mat, Section = sec });
            doc.Supports.Add(new SupportRowVm { Node = n1, RestrainX = true, RestrainY = true, RestrainRz = true });
            doc.NodalLoads.Add(new NodalLoadRowVm { Node = n2, Fy = -transverseLoadN / 1000.0 }); // N → kN

            ConfigureMember(doc);
            return doc;
        }

        /// <summary>Horizontal bar (fixed at n1) with an axial tip load in +X (tension).</summary>
        private static ProjectDocument BuildAxialBar(eStrengthClass enClass, double axialLoadN)
        {
            var doc = new ProjectDocument();
            doc.Design.Code = eTimberCode.EC5;

            var n1 = new NodeRowVm { X = 0, Y = 0 };
            var n2 = new NodeRowVm { X = 1.0, Y = 0 }; // 1 m cantilever (coords in m)
            doc.Nodes.Add(n1);
            doc.Nodes.Add(n2);

            var mat = new MaterialRowVm { StrengthClass = enClass };
            doc.Materials.Add(mat);

            var sec = new SectionRowVm { Width = 100, Depth = 200, MomentOfInertia = 100 * 200 * 200 * 200 / 12.0 };
            doc.Sections.Add(sec);

            doc.Elements.Add(new ElementRowVm { StartNode = n1, EndNode = n2, Material = mat, Section = sec });
            doc.Supports.Add(new SupportRowVm { Node = n1, RestrainX = true, RestrainY = true, RestrainRz = true });
            doc.NodalLoads.Add(new NodalLoadRowVm { Node = n2, Fx = axialLoadN / 1000.0 }); // N → kN

            ConfigureMember(doc);
            return doc;
        }

        /// <summary>Simply-supported beam (pin + roller) carrying a downward UDL.</summary>
        private static ProjectDocument BuildSimplySupportedUdlBeam(double lengthM, double udlKnPerM)
        {
            var doc = new ProjectDocument();
            doc.Design.Code = eTimberCode.EC5;

            var n1 = new NodeRowVm { X = 0, Y = 0 };
            var n2 = new NodeRowVm { X = lengthM, Y = 0 };
            doc.Nodes.Add(n1);
            doc.Nodes.Add(n2);

            var mat = new MaterialRowVm { StrengthClass = eStrengthClass.C24 };
            doc.Materials.Add(mat);

            var sec = new SectionRowVm { Width = 100, Depth = 200, MomentOfInertia = 100 * 200 * 200 * 200 / 12.0 };
            doc.Sections.Add(sec);

            var element = new ElementRowVm { StartNode = n1, EndNode = n2, Material = mat, Section = sec };
            doc.Elements.Add(element);
            doc.Supports.Add(new SupportRowVm { Node = n1, RestrainX = true, RestrainY = true, RestrainRz = false });  // pin
            doc.Supports.Add(new SupportRowVm { Node = n2, RestrainX = false, RestrainY = true, RestrainRz = false }); // roller
            doc.DistributedLoads.Add(new DistributedLoadRowVm { Element = element, MagnitudePerLength = -udlKnPerM });

            ConfigureMember(doc);
            return doc;
        }

        private static FrameAnalysisResult AnalyzeWithStations(ProjectDocument doc)
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

        private static void ConfigureMember(ProjectDocument doc)
        {
            MemberDesignRowVm md = doc.MemberDesigns[0];
            md.EffectiveLengthMajor = 1.0; // m (= 1000 mm in the design backend)
            md.EffectiveLengthMinor = 1.0;
            md.EffectiveBeamLength = 1.0;
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
                new StiffnessSingularityDetector());

            return analyzer.Analyze(model);
        }
    }
}
