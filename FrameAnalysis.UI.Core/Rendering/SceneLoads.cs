namespace FrameAnalysis.UI.Core.Rendering;

/// <summary>A joint load: force components and a moment at a node position.</summary>
public sealed record SceneNodalLoad(double X, double Y, double Fx, double Fy, double Mz);

/// <summary>
/// A uniform load along a member. (DirX, DirY) is the unit direction the load acts in;
/// Magnitude is per unit length (signed).
/// </summary>
public sealed record SceneDistributedLoad(
    double StartX, double StartY, double EndX, double EndY,
    double DirX, double DirY, double Magnitude);

/// <summary>A concentrated load at a point on a member span, acting along (DirX, DirY).</summary>
public sealed record ScenePointLoad(double X, double Y, double DirX, double DirY, double Magnitude);
