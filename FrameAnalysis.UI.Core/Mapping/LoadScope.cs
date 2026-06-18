using StructuralLoads;

namespace FrameAnalysis.UI.Core.Mapping;

/// <summary>
/// Selects which loads <see cref="ModelInputMapper"/> includes when building an input, so the
/// superposition basis can solve one load case at a time. <see cref="All"/> is the full model
/// (default behaviour); <see cref="Nature"/> isolates a single nature's actions (no settlements);
/// <see cref="SettlementsOnly"/> isolates the prescribed settlements. Geometry, material, section,
/// and supports are always included — only the load tables vary.
/// </summary>
public sealed record LoadScope(eLoadNature? OnlyNature, bool IncludeActions, bool IncludeSettlements)
{
    public static readonly LoadScope All = new(null, true, true);
    public static readonly LoadScope SettlementsOnly = new(null, false, true);
    public static LoadScope Nature(eLoadNature nature) => new(nature, true, false);

    /// <summary>True when an action of <paramref name="nature"/> is included by this scope.
    /// (Thermal loads count as <see cref="eLoadNature.Thermal"/>.)</summary>
    public bool IncludesAction(eLoadNature nature) =>
        IncludeActions && (OnlyNature is null || OnlyNature.Value == nature);
}
