using CommunityToolkit.Mvvm.ComponentModel;
using MemberDesigner.TimberMaterialData;
using static MemberDesigner.Designers.Enums;

namespace FrameAnalysis.UI.Core.Documents.Rows;

/// <summary>
/// A material row. The analysis only needs <see cref="ElasticModulus"/> (maps to
/// StructureInputData.MaterialTable); the remaining fields carry the timber design values.
///
/// Picking a grade auto-fills the design values from <see cref="TimberMaterialDatabase"/>
/// (EN strength class for EC5/TR, NDS species+grade for US) and syncs <see cref="ElasticModulus"/>
/// to the mean modulus so analysis and design stay consistent. Set <see cref="ManualOverride"/>
/// to type values by hand instead. All strengths/moduli are MPa; density is kg/m³.
/// </summary>
public partial class MaterialRowVm : ObservableObject, IIdentifiedRow
{
    [ObservableProperty] private int id;
    [ObservableProperty] private string name = string.Empty;

    /// <summary>Young's modulus E [force / length^2] used by the analysis.</summary>
    [ObservableProperty] private double elasticModulus;

    // --- Design metadata ---
    [ObservableProperty] private eTimberMaterialType materialType = eTimberMaterialType.SolidTimber;

    /// <summary>EN 338 / EN 14080 strength class (EC5 / TR). Null until chosen.</summary>
    [ObservableProperty] private eStrengthClass? strengthClass;

    /// <summary>NDS species + grade (US). Null until chosen.</summary>
    [ObservableProperty] private eNdsSpeciesGrade? speciesGrade;

    /// <summary>When true, the design values below are entered by hand and not overwritten by a grade pick.</summary>
    [ObservableProperty] private bool manualOverride;

    // --- Resolved design values (code-neutral; filled from the grade or typed by hand) ---
    [ObservableProperty] private double bendingStrength;        // f_m,k / Fb
    [ObservableProperty] private double tensionStrength;        // f_t,0,k / Ft
    [ObservableProperty] private double tensionPerpStrength;    // f_t,90,k
    [ObservableProperty] private double compressionStrength;    // f_c,0,k / Fc
    [ObservableProperty] private double compressionPerpStrength;// f_c,90,k / Fc90
    [ObservableProperty] private double shearStrength;          // f_v,k / Fv
    [ObservableProperty] private double modulusMean;            // E_0,mean / E
    [ObservableProperty] private double modulusBuckling;        // E_0,05 / Emin
    [ObservableProperty] private double density;                // ρ_k

    partial void OnStrengthClassChanged(eStrengthClass? value)
    {
        if (value is null || ManualOverride) return;
        ApplyEn(TimberMaterialDatabase.GetEn(value.Value));
        MaterialType = value.Value >= eStrengthClass.GL20h
            ? eTimberMaterialType.GluedLaminatedTimber
            : eTimberMaterialType.SolidTimber;
    }

    partial void OnSpeciesGradeChanged(eNdsSpeciesGrade? value)
    {
        if (value is null || ManualOverride) return;
        ApplyNds(TimberMaterialDatabase.GetNds(value.Value));
        MaterialType = eTimberMaterialType.SolidTimber;
    }

    partial void OnManualOverrideChanged(bool value)
    {
        // Leaving override mode re-applies the current grade so the row reflects the table again.
        if (value) return;
        if (StrengthClass is not null) ApplyEn(TimberMaterialDatabase.GetEn(StrengthClass.Value));
        else if (SpeciesGrade is not null) ApplyNds(TimberMaterialDatabase.GetNds(SpeciesGrade.Value));
    }

    private void ApplyEn(EnStrengthProperties p)
    {
        BendingStrength = p.Fmk;
        TensionStrength = p.Ft0k;
        TensionPerpStrength = p.Ft90k;
        CompressionStrength = p.Fc0k;
        CompressionPerpStrength = p.Fc90k;
        ShearStrength = p.Fvk;
        ModulusMean = p.E0Mean;
        ModulusBuckling = p.E005;
        Density = p.RhoK;
        ElasticModulus = p.E0Mean;
    }

    private void ApplyNds(NdsReferenceValues p)
    {
        BendingStrength = p.Fb;
        TensionStrength = p.Ft;
        TensionPerpStrength = 0; // NDS has no published reference tension-perp value
        CompressionStrength = p.Fc;
        CompressionPerpStrength = p.Fc90;
        ShearStrength = p.Fv;
        ModulusMean = p.E;
        ModulusBuckling = p.Emin;
        Density = 0;
        ElasticModulus = p.E;
    }

    public override string ToString() =>
        string.IsNullOrWhiteSpace(Name) ? $"Material {Id}" : Name;
}
