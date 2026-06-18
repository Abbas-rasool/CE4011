namespace FrameAnalysis.UI.Core.Units;

/// <summary>
/// Single source of truth for the project's unit conventions.
///
/// <para><b>Canonical internal analysis units = { kN, m, kN/m² }</b> (a coherent set: EA/L and
/// EI/L³ both reduce to kN). The FEM solver and the render scene work entirely in these, so
/// reactions come out in kN / kN·m, displacements in m, and member end forces in kN / kN·m.</para>
///
/// <para>The UI <b>stores and edits</b> values in display units (Phase 1 fixed defaults):</para>
/// <list type="bullet">
///   <item>length (coordinates, spans, settlements, distances) — <b>m</b></item>
///   <item>section width / depth — <b>mm</b></item>
///   <item>moment of inertia — <b>mm⁴</b></item>
///   <item>force — <b>kN</b>; moment — <b>kN·m</b>; distributed load — <b>kN/m</b></item>
///   <item>modulus / strength (stress) — <b>MPa</b>; density — <b>kg/m³</b></item>
/// </list>
///
/// <para>Conversions live only at the two mapping boundaries:
/// <see cref="Mapping.ModelInputMapper"/> converts display → canonical for the solver, and
/// <see cref="Mapping.DesignInputMapper"/> converts canonical/display → the timber design
/// backend's { N, mm, MPa }. (Phase 2 will make the display units user-selectable; keeping the
/// factors here is what makes that a localized change.)</para>
/// </summary>
public static class UnitSystem
{
    // --- display → canonical (solver: kN, m, kN/m²) ---

    /// <summary>MPa → kN/m². 1 MPa = 1e6 N/m² = 1e3 kN/m².</summary>
    public const double MPaToKNm2 = 1_000.0;

    /// <summary>mm → m (section width/depth).</summary>
    public const double MmToM = 1.0e-3;

    /// <summary>mm⁴ → m⁴ (moment of inertia).</summary>
    public const double Mm4ToM4 = 1.0e-12;

    // --- canonical/display → timber design backend (N, mm, MPa) ---

    /// <summary>kN → N (axial / shear demands).</summary>
    public const double KNToN = 1_000.0;

    /// <summary>kN·m → N·mm (moment demands).</summary>
    public const double KNmToNmm = 1.0e6;

    /// <summary>m → mm (effective lengths).</summary>
    public const double MToMm = 1_000.0;
}
