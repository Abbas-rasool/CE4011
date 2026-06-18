using CommunityToolkit.Mvvm.ComponentModel;

namespace FrameAnalysis.UI.Core.Documents.Rows;

/// <summary>
/// A section row. Maps to StructureInputData.SectionTable [Width, Length, MomentOfInertia].
/// (The table's "Length" column is the section depth here; Area is derived as Width × Depth,
/// mirroring StructureModelBuilder.) Width/Depth are entered in mm and I in mm⁴; ModelInputMapper
/// converts them to the canonical metre system for the solver.
/// </summary>
public partial class SectionRowVm : ObservableObject, IIdentifiedRow
{
    [ObservableProperty] private int id;
    [ObservableProperty] private string name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Area))]
    private double width;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Area))]
    private double depth;

    [ObservableProperty] private double momentOfInertia;

    /// <summary>Derived cross-sectional area (Width × Depth).</summary>
    public double Area => Width * Depth;

    public override string ToString() =>
        string.IsNullOrWhiteSpace(Name) ? $"Section {Id}" : Name;
}
