namespace FrameAnalysis.UI.Core.Rendering;

/// <summary>
/// The deflected shape of a member as a polyline in world coordinates (already amplified
/// by the deflection scale). Sampled from the member's station points using the Hermite
/// cubic shape, so the polyline carries the true bending curvature, not just the displaced
/// end nodes.
/// </summary>
public sealed record SceneDeflectedMember(int Id, IReadOnlyList<ScenePoint> Points);

/// <summary>A support reaction drawn at a node: force components and a moment.</summary>
public sealed record SceneReaction(double X, double Y, double Fx, double Fy, double Mz);
