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

    /// <summary>I for a solid rectangle bending in-plane: b·d³/12.</summary>
    private double RectangularInertia => Width * Depth * Depth * Depth / 12.0;

    /// <summary>True once the user has typed an I that doesn't match the rectangular formula
    /// (e.g. a non-rectangular section). While set, dimension edits no longer overwrite I.</summary>
    private bool customInertia;

    /// <summary>Guards the programmatic I-set in <see cref="AutoFillInertia"/> from being mistaken
    /// for a user override.</summary>
    private bool autoFilling;

    /// <summary>Lets the owning document batch-rescale Width/Depth/I (e.g. on a unit switch)
    /// without the dimension changes re-deriving I mid-update.</summary>
    internal bool SuppressInertiaAutoFill { get; set; }

    partial void OnWidthChanged(double value) => AutoFillInertia();
    partial void OnDepthChanged(double value) => AutoFillInertia();

    partial void OnMomentOfInertiaChanged(double value)
    {
        if (autoFilling || SuppressInertiaAutoFill) return;

        // A directly-entered I that doesn't match the rectangular formula is a custom value;
        // remember it so later Width/Depth edits don't clobber it.
        customInertia = !IsRectangular(value);
    }

    /// <summary>Re-derives I from the current Width/Depth, unless the user has locked in a custom
    /// value or the document is suppressing auto-fill for a batch rescale. Keeps I editable —
    /// it only fills the rectangular default.</summary>
    private void AutoFillInertia()
    {
        if (customInertia || SuppressInertiaAutoFill) return;

        autoFilling = true;
        MomentOfInertia = RectangularInertia;
        autoFilling = false;
    }

    private bool IsRectangular(double i) =>
        System.Math.Abs(i - RectangularInertia) <= 1e-6 * System.Math.Max(1.0, System.Math.Abs(RectangularInertia));

    public override string ToString() =>
        string.IsNullOrWhiteSpace(Name) ? $"Section {Id}" : Name;
}
