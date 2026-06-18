using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FrameAnalysis.UI.Core.Units;

public enum SectionLengthUnit { Millimetre, Centimetre, Metre }
public enum SettlementUnit { Millimetre, Metre }

/// <summary>
/// User-selectable display units for the quantities where engineers genuinely differ — section
/// sizes and imposed settlements. The chosen unit is also the unit the document <b>stores</b>
/// values in, so a switch rescales the stored numbers (handled by <see cref="Documents.ProjectDocument"/>)
/// rather than needing per-cell value converters. The mappers read the <c>*ToM</c> factors to
/// convert to the canonical metre system; see <see cref="UnitSystem"/>.
///
/// Other quantities are fixed at the project conventions: coordinates/spans m, force kN,
/// moment kN·m, distributed kN/m, stress/modulus MPa, density kg/m³.
/// </summary>
public sealed partial class UnitSettings : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SectionToM))]
    [NotifyPropertyChangedFor(nameof(InertiaToM4))]
    [NotifyPropertyChangedFor(nameof(SectionUnitLabel))]
    [NotifyPropertyChangedFor(nameof(SectionHeader))]
    private SectionLengthUnit section = SectionLengthUnit.Millimetre;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SettlementToM))]
    [NotifyPropertyChangedFor(nameof(SettlementUnitLabel))]
    [NotifyPropertyChangedFor(nameof(SettlementHeader))]
    private SettlementUnit settlement = SettlementUnit.Millimetre;

    // --- display unit → canonical metre system ---
    public double SectionToM => SectionToMFactor(Section);
    public double InertiaToM4 => Math.Pow(SectionToM, 4);
    public double SettlementToM => SettlementToMFactor(Settlement);

    // --- labels for the (bindable) group-box headers ---
    public string SectionUnitLabel => Section switch
    {
        SectionLengthUnit.Millimetre => "mm",
        SectionLengthUnit.Centimetre => "cm",
        _ => "m"
    };

    public string SettlementUnitLabel => Settlement == SettlementUnit.Millimetre ? "mm" : "m";

    public string SectionHeader =>
        $"Sections — Width/Depth in {SectionUnitLabel}, I in {SectionUnitLabel}⁴, A in {SectionUnitLabel}²";

    public string SettlementHeader =>
        $"Settlements — dUx/dUy in {SettlementUnitLabel}, dRz in rad";

    public static double SectionToMFactor(SectionLengthUnit u) => u switch
    {
        SectionLengthUnit.Millimetre => 1.0e-3,
        SectionLengthUnit.Centimetre => 1.0e-2,
        _ => 1.0
    };

    public static double SettlementToMFactor(SettlementUnit u) =>
        u == SettlementUnit.Millimetre ? 1.0e-3 : 1.0;
}
