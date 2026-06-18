namespace FrameAnalysis.UI.Core.Rendering;

/// <summary>A point in world coordinates.</summary>
public readonly record struct ScenePoint(double X, double Y);

/// <summary>Axis-aligned model extents in world coordinates, used to fit the view.</summary>
public readonly record struct SceneBounds(double MinX, double MinY, double MaxX, double MaxY, bool IsEmpty)
{
    public static SceneBounds Empty => new(0, 0, 0, 0, true);

    public double Width => MaxX - MinX;
    public double Height => MaxY - MinY;
    public double CenterX => (MinX + MaxX) / 2.0;
    public double CenterY => (MinY + MaxY) / 2.0;
}

/// <summary>A node glyph at a world position. <c>Id</c> is the 1-based node id.</summary>
public sealed record SceneNode(int Id, double X, double Y);

/// <summary>A straight member between two world points. <c>Id</c> is the 1-based element id.</summary>
public sealed record SceneMember(int Id, double StartX, double StartY, double EndX, double EndY, bool IsTruss);

/// <summary>
/// A support at a node, with the restrained global DOFs. The renderer chooses the glyph
/// (pin / roller / fixed) from the flags — the scene stays presentation-agnostic.
/// </summary>
public sealed record SceneSupport(double X, double Y, bool RestrainX, bool RestrainY, bool RestrainRz);
