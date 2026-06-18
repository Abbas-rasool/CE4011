using FrameAnalysis.UI.Core.Rendering;
using Xunit;

namespace FEMTestProject
{
    public class ViewportTests
    {
        [Fact]
        public void RoundTrip_WorldScreenWorld_IsIdentity()
        {
            var vp = new Viewport();
            vp.FitToBounds(new SceneBounds(0, 0, 4, 3, false), 800, 600);

            (double sx, double sy) = vp.WorldToScreen(2.0, 1.5);
            (double wx, double wy) = vp.ScreenToWorld(sx, sy);

            Assert.Equal(2.0, wx, 9);
            Assert.Equal(1.5, wy, 9);
        }

        [Fact]
        public void FitToBounds_CentersModelAndFlipsY()
        {
            var vp = new Viewport();
            var bounds = new SceneBounds(0, 0, 4, 3, false);
            vp.FitToBounds(bounds, 800, 600);

            // Model center maps to viewport center.
            (double cx, double cy) = vp.WorldToScreen(bounds.CenterX, bounds.CenterY);
            Assert.Equal(400.0, cx, 6);
            Assert.Equal(300.0, cy, 6);

            // World Y is up: a higher world point has a smaller screen Y.
            Assert.True(vp.WorldToScreen(0, 1).Y < vp.WorldToScreen(0, 0).Y);
        }

        [Fact]
        public void ZoomAt_KeepsWorldPointUnderCursorFixed()
        {
            var vp = new Viewport();
            vp.FitToBounds(new SceneBounds(0, 0, 4, 3, false), 800, 600);

            (double wxBefore, double wyBefore) = vp.ScreenToWorld(120, 90);
            vp.ZoomAt(2.5, 120, 90);
            (double wxAfter, double wyAfter) = vp.ScreenToWorld(120, 90);

            Assert.Equal(wxBefore, wxAfter, 9);
            Assert.Equal(wyBefore, wyAfter, 9);
        }

        [Fact]
        public void FitToBounds_EmptyModel_FallsBackToUnitScale()
        {
            var vp = new Viewport();
            vp.FitToBounds(SceneBounds.Empty, 800, 600);
            Assert.Equal(1.0, vp.Scale, 9);
        }

        [Fact]
        public void FitToBounds_LeavesMarginAroundModel()
        {
            var vp = new Viewport();
            vp.FitToBounds(new SceneBounds(0, 0, 4, 3, false), 800, 600);

            (double X, double Y) bottomLeft = vp.WorldToScreen(0, 0);
            (double X, double Y) topRight = vp.WorldToScreen(4, 3);

            // The model sits comfortably inside the viewport, not flush against the edges,
            // so support/load glyphs drawn beyond the nodes stay visible.
            Assert.True(bottomLeft.X > 20 && bottomLeft.Y < 580);
            Assert.True(topRight.X < 780 && topRight.Y > 20);
        }
    }
}
