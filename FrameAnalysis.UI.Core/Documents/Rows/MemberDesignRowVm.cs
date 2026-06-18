using CommunityToolkit.Mvvm.ComponentModel;
using static MemberDesigner.Designers.Enums;

namespace FrameAnalysis.UI.Core.Documents.Rows;

/// <summary>
/// Per-member design parameters. References the <see cref="ElementRowVm"/> it designs by
/// object (kept in sync with the element collection). Material strength comes from the
/// element's material; section geometry from the element's section; demands are auto-fed from
/// the analysis result — so this row only carries the things the analysis can't supply
/// (effective lengths, bracing, support type). Effective lengths are entered in m (the UI's
/// length unit) and converted to mm for the design backend by DesignInputMapper.
/// </summary>
public partial class MemberDesignRowVm : ObservableObject
{
    public MemberDesignRowVm() { }

    public MemberDesignRowVm(ElementRowVm element) => this.element = element;

    [ObservableProperty] private ElementRowVm? element;

    /// <summary>Unsupported length for buckling about the major axis.</summary>
    [ObservableProperty] private double effectiveLengthMajor;

    /// <summary>Unsupported length for buckling about the minor axis.</summary>
    [ObservableProperty] private double effectiveLengthMinor;

    /// <summary>Effective beam length for lateral-torsional stability.</summary>
    [ObservableProperty] private double effectiveBeamLength;

    /// <summary>Net area as a fraction of gross (1.0 = no holes/notches deducted).</summary>
    [ObservableProperty] private double netAreaFactor = 1.0;

    /// <summary>Bearing support type (EC5 / TR compression perpendicular).</summary>
    [ObservableProperty] private eSupportTypeEU supportType = eSupportTypeEU.Continuous;

    /// <summary>Compression edge laterally restrained → C_L / k_crit = 1.</summary>
    [ObservableProperty] private bool isLaterallySupported = true;

    /// <summary>Repetitive member (NDS C_r / closely-spaced framing).</summary>
    [ObservableProperty] private bool isRepetitiveMember;

    public override string ToString() =>
        Element is null ? "Member —" : Element.ToString();
}
