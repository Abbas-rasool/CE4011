namespace FrameAnalysis.UI.Core.Rendering;

/// <summary>
/// The deflected shape of a member as a polyline in world coordinates (already amplified
/// by the deflection scale). Currently two points (linear); Phase 5 will sample the true
/// cubic shape, adding intermediate points without changing this contract.
/// </summary>
public sealed record SceneDeflectedMember(int Id, IReadOnlyList<ScenePoint> Points);

/// <summary>A support reaction drawn at a node: force components and a moment.</summary>
public sealed record SceneReaction(double X, double Y, double Fx, double Fy, double Mz);
