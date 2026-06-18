namespace FrameAnalysis.UI.Core.Rendering;

/// <summary>
/// An immutable, presentation-oriented snapshot of the structure to draw. Holds only plain
/// geometry — no solver types, no document view-models, no WPF. <see cref="SceneBuilder"/>
/// produces it; <see cref="IStructuralRenderer"/> consumes it. The result overlays
/// (<see cref="DeflectedMembers"/>, <see cref="Reactions"/>) are empty unless an analysis
/// result was supplied to the builder.
/// </summary>
public sealed class Scene
{
    public IReadOnlyList<SceneNode> Nodes { get; init; } = Array.Empty<SceneNode>();
    public IReadOnlyList<SceneMember> Members { get; init; } = Array.Empty<SceneMember>();
    public IReadOnlyList<SceneSupport> Supports { get; init; } = Array.Empty<SceneSupport>();

    public IReadOnlyList<SceneNodalLoad> NodalLoads { get; init; } = Array.Empty<SceneNodalLoad>();
    public IReadOnlyList<SceneDistributedLoad> DistributedLoads { get; init; } = Array.Empty<SceneDistributedLoad>();
    public IReadOnlyList<ScenePointLoad> PointLoads { get; init; } = Array.Empty<ScenePointLoad>();

    public IReadOnlyList<SceneDeflectedMember> DeflectedMembers { get; init; } = Array.Empty<SceneDeflectedMember>();
    public IReadOnlyList<SceneReaction> Reactions { get; init; } = Array.Empty<SceneReaction>();

    /// <summary>Model extents in world coordinates (from node positions), for view fitting.</summary>
    public SceneBounds Bounds { get; init; } = SceneBounds.Empty;

    public static Scene Empty { get; } = new();
}
