namespace FrameAnalysis.UI.Core.ViewModels;

/// <summary>
/// Identifies which single input/results sheet is shown in the left pane. Drives the
/// menu-based navigation — one table visible at a time instead of all stacked together.
/// </summary>
public enum Sheet
{
    // Geometry
    Nodes,
    Elements,
    Materials,
    Sections,
    Supports,

    // Loads
    JointLoads,
    DistributedLoads,
    PointLoads,
    Settlements,
    ThermalLoads,

    // Loads — placeholders (backend not ready yet)
    LoadCases,
    LoadCombinations,

    // Design
    DesignSettings,
    MemberDesign,
    DesignResults,

    // Results
    Results
}
