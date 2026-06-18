using System.Threading.Tasks;
using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Rendering;
using FrameAnalysis.UI.Core.Services;
using Xunit;

namespace FEMTestProject
{
    public class SceneBuilderTests
    {
        [Fact]
        public void Build_PortalFrame_ProducesStaticGeometry()
        {
            ProjectDocument doc = UiTestDocuments.BuildPortalFrameDocument();

            Scene scene = SceneBuilder.Build(doc);

            Assert.Equal(4, scene.Nodes.Count);
            Assert.Equal(3, scene.Members.Count);
            Assert.Equal(2, scene.Supports.Count);
            Assert.Single(scene.NodalLoads);
            Assert.Single(scene.DistributedLoads);

            // Bounds span the 4x3 portal.
            Assert.False(scene.Bounds.IsEmpty);
            Assert.Equal(0.0, scene.Bounds.MinX);
            Assert.Equal(4.0, scene.Bounds.MaxX);
            Assert.Equal(3.0, scene.Bounds.MaxY);

            // Distributed load resolves to a downward unit direction (global Y).
            SceneDistributedLoad udl = scene.DistributedLoads[0];
            Assert.Equal(0.0, udl.DirX);
            Assert.Equal(1.0, udl.DirY);
            Assert.Equal(-10.0, udl.Magnitude);

            // No result supplied -> result overlays are empty.
            Assert.Empty(scene.DeflectedMembers);
            Assert.Empty(scene.Reactions);
        }

        [Fact]
        public void Build_SkipsElementWithMissingNode()
        {
            ProjectDocument doc = UiTestDocuments.BuildPortalFrameDocument();
            doc.Elements[0].EndNode = null; // incomplete row while editing

            Scene scene = SceneBuilder.Build(doc);

            Assert.Equal(2, scene.Members.Count); // skipped, not thrown
        }

        [Fact]
        public async Task Build_WithResult_AddsDeflectedShapeAndReactions()
        {
            ProjectDocument doc = UiTestDocuments.BuildPortalFrameDocument();
            AnalysisOutcome outcome = await AnalysisService.CreateDefault().RunAsync(doc);
            Assert.NotNull(outcome.Result);

            Scene scene = SceneBuilder.Build(doc, outcome.Result, deflectionScale: 100.0);

            Assert.Equal(3, scene.DeflectedMembers.Count);
            Assert.All(scene.DeflectedMembers, m => Assert.Equal(2, m.Points.Count));
            Assert.Equal(2, scene.Reactions.Count); // two fixed bases
        }
    }
}
