namespace FrameAnalysis.UI.Core.Rendering;

/// <summary>
/// World &lt;-&gt; screen transform with zoom and pan. Pure math (no WPF), so it is
/// unit-testable. World Y is up; screen Y is down. Screen units are pixels.
/// </summary>
public sealed class Viewport
{
    /// <summary>Pixels per world unit. Always positive.</summary>
    public double Scale { get; private set; } = 1.0;

    /// <summary>Screen position (px) that world (0,0) maps to.</summary>
    public double OriginScreenX { get; private set; }
    public double OriginScreenY { get; private set; }

    public (double X, double Y) WorldToScreen(double worldX, double worldY)
        => (OriginScreenX + worldX * Scale, OriginScreenY - worldY * Scale);

    public (double X, double Y) ScreenToWorld(double screenX, double screenY)
        => ((screenX - OriginScreenX) / Scale, (OriginScreenY - screenY) / Scale);

    /// <summary>
    /// Fit the given model bounds within a viewport of the given pixel size, leaving a
    /// uniform padding plus a world-space margin (a fraction of the model size) so that
    /// glyphs drawn beyond the nodes — supports, load arrows, deflected shape — stay visible.
    /// Centers the model and preserves aspect ratio. Falls back to 1:1 for an empty model.
    /// </summary>
    public void FitToBounds(SceneBounds bounds, double widthPx, double heightPx,
        double paddingPx = 28.0, double marginFraction = 0.15)
    {
        SceneBounds fit = ExpandBounds(bounds, marginFraction);

        double availW = Math.Max(1.0, widthPx - 2.0 * paddingPx);
        double availH = Math.Max(1.0, heightPx - 2.0 * paddingPx);

        double scaleX = fit.IsEmpty || fit.Width <= 0.0 ? double.PositiveInfinity : availW / fit.Width;
        double scaleY = fit.IsEmpty || fit.Height <= 0.0 ? double.PositiveInfinity : availH / fit.Height;

        double scale = Math.Min(scaleX, scaleY);
        if (double.IsInfinity(scale) || scale <= 0.0)
            scale = 1.0;

        Scale = scale;

        // The expanded bounds share the original centre, so the model stays centred.
        double cx = fit.IsEmpty ? 0.0 : fit.CenterX;
        double cy = fit.IsEmpty ? 0.0 : fit.CenterY;
        OriginScreenX = widthPx / 2.0 - cx * Scale;
        OriginScreenY = heightPx / 2.0 + cy * Scale;
    }

    /// <summary>
    /// Grows bounds outward by a fraction of their size. A degenerate dimension (a flat or
    /// vertical model) borrows the other dimension's size so it still gets breathing room and
    /// a finite fit scale instead of the 1:1 fallback.
    /// </summary>
    private static SceneBounds ExpandBounds(SceneBounds b, double fraction)
    {
        if (b.IsEmpty || fraction <= 0.0)
            return b;

        double marginX = b.Width * fraction;
        double marginY = b.Height * fraction;

        if (b.Width <= 0.0) marginX = (b.Height > 0.0 ? b.Height : 1.0) * fraction;
        if (b.Height <= 0.0) marginY = (b.Width > 0.0 ? b.Width : 1.0) * fraction;

        return new SceneBounds(b.MinX - marginX, b.MinY - marginY, b.MaxX + marginX, b.MaxY + marginY, false);
    }

    /// <summary>
    /// Zoom by a factor (&gt; 1 zooms in) about a screen point, keeping the world point under
    /// that screen point fixed.
    /// </summary>
    public void ZoomAt(double factor, double screenX, double screenY)
    {
        if (factor <= 0.0 || double.IsNaN(factor))
            return;

        (double worldX, double worldY) = ScreenToWorld(screenX, screenY);
        Scale *= factor;
        OriginScreenX = screenX - worldX * Scale;
        OriginScreenY = screenY + worldY * Scale;
    }

    /// <summary>Pan the view by a screen-space delta (px).</summary>
    public void PanBy(double deltaScreenX, double deltaScreenY)
    {
        OriginScreenX += deltaScreenX;
        OriginScreenY += deltaScreenY;
    }
}
