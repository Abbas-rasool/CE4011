using System;
using System.Linq;
using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysisProgram.ANALYSIS_CORE;
using MemberDesigner.Designers;
using static MemberDesigner.Designers.Enums;

namespace FrameAnalysis.UI.Core.Mapping;

/// <summary>
/// Builds a <see cref="TimberMemberDesignContext"/> for one member from the design document
/// (material design values + section geometry + per-member parameters + project settings) and
/// the latest analysis result (demands). This is the design-side analogue of
/// <see cref="ModelInputMapper"/>: it bridges the UI's object model to the design backend.
/// </summary>
public static class DesignInputMapper
{
    /// <summary>
    /// Builds the design context for <paramref name="memberRow"/>. <paramref name="elementNumber"/>
    /// is the element's 1-based position in the document (matches <c>ElementEndForceResult.Element.Id</c>).
    /// </summary>
    public static TimberMemberDesignContext BuildContext(
        MemberDesignRowVm memberRow,
        int elementNumber,
        DesignSettings settings,
        FrameAnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(memberRow);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(result);

        ElementRowVm element = memberRow.Element
            ?? throw new InvalidOperationException("Member design row has no element assigned.");

        MaterialRowVm material = element.Material
            ?? throw new InvalidOperationException($"{element} has no material assigned.");

        SectionRowVm section = element.Section
            ?? throw new InvalidOperationException($"{element} has no section assigned.");

        var context = new TimberMemberDesignContext { Code = settings.Code };

        ApplyMaterial(context, material, settings.Code);
        ApplyGeometry(context, section, memberRow.NetAreaFactor);
        ApplyMember(context, memberRow);
        ApplySettings(context, settings);
        ApplyDemands(context, result, elementNumber);

        return context;
    }

    private static void ApplyMaterial(TimberMemberDesignContext ctx, MaterialRowVm materialUI, eTimberCode code)
    {
        ctx.MaterialType = materialUI.MaterialType;
        ctx.BendingStrength = (float)materialUI.BendingStrength;
        ctx.TensionStrength = (float)materialUI.TensionStrength;
        ctx.TensionPerpStrength = (float)materialUI.TensionPerpStrength;
        ctx.CompressionStrength = (float)materialUI.CompressionStrength;
        ctx.CompressionPerpStrength = (float)materialUI.CompressionPerpStrength;
        ctx.ShearStrength = (float)materialUI.ShearStrength;
        ctx.ModulusMean = (float)materialUI.ModulusMean;
        ctx.ModulusBuckling = (float)materialUI.ModulusBuckling;
        ctx.Density = (float)materialUI.Density;

        if (code == eTimberCode.US && materialUI.SpeciesGrade is not null)
            ctx.TimberGrade = ToTimberGrade(materialUI.SpeciesGrade.Value);
    }

    private static void ApplyGeometry(TimberMemberDesignContext c, SectionRowVm s, double netAreaFactor)
    {
        // Major axis = depth, minor axis = width (rectangular section).
        float depth = (float)s.Depth;
        float width = (float)s.Width;
        float gross = width * depth;

        c.H1 = depth;
        c.H2 = width;
        c.GrossArea = gross;
        c.NetArea = gross * (float)netAreaFactor;
        c.SectionModulusMajor = width * depth * depth / 6f;
        c.SectionModulusMinor = depth * width * width / 6f;
        c.Inertia = width * depth * depth * depth / 12f;
        c.FirstMomentOfArea = width * depth * depth / 8f; // Q at neutral axis (rectangle)
    }

    private static void ApplyMember(TimberMemberDesignContext c, MemberDesignRowVm r)
    {
        c.EffectiveLengthMajor = (float)r.EffectiveLengthMajor;
        c.EffectiveLengthMinor = (float)r.EffectiveLengthMinor;
        c.EffectiveBeamLength = (float)r.EffectiveBeamLength;
        c.SupportType = r.SupportType;
        c.IsLaterallySupported = r.IsLaterallySupported;
        c.IsRepetitiveMember = r.IsRepetitiveMember;
    }

    private static void ApplySettings(TimberMemberDesignContext c, DesignSettings s)
    {
        c.ServiceClass = s.ServiceClass;
        c.MoistureCondition = s.MoistureCondition;
        c.LoadDurationClass = s.DefaultLoadDuration;
        c.FactorsModified = s.FactorsModified;
        c.PartialFactor = (float)s.PartialFactor;
        c.ModificationFactor = (float)s.ModificationFactor;
        c.LoadDurationFactor = (float)s.LoadDurationFactor;
        c.TimeEffectFactor = (float)s.TimeEffectFactor;
    }

    private static void ApplyDemands(TimberMemberDesignContext c, FrameAnalysisResult result, int elementNumber)
    {
        ElementEndForceResult? forces = result.ElementEndForces.FirstOrDefault(e => e.Element.Id == elementNumber);

        if (forces is null) return; // no demands (e.g. element not in the solved model)

        double[] f = forces.LocalEndForces;
        bool isFrame = f.Length == 6;

        double fx1 = f[0];
        double fy1 = f[1];
        double mz1 = isFrame ? f[2] : 0.0;
        double fy2 = isFrame ? f[4] : f[3];
        double mz2 = isFrame ? f[5] : 0.0;

        // Axial: tension positive = -Fx1 (= Fx2 for a member with no axial span load).
        double axialTension = -fx1;

        c.AxialTension = (float)Math.Max(axialTension, 0.0);
        c.AxialCompression = (float)Math.Max(-axialTension, 0.0);
        c.Shear = (float)Math.Max(Math.Abs(fy1), Math.Abs(fy2));
        c.MomentMajor = (float)Math.Max(Math.Abs(mz1), Math.Abs(mz2));
        c.MomentMinor = 0f; // out-of-plane is zero in the 2D solver
    }

    /// <summary>Maps an NDS species+grade to its commercial grade (for US adjustment factors).</summary>
    private static eTimberGrades ToTimberGrade(eNdsSpeciesGrade speciesGrade) => speciesGrade switch
    {
        eNdsSpeciesGrade.DouglasFirLarch_SS or eNdsSpeciesGrade.HemFir_SS or eNdsSpeciesGrade.SprucePineFir_SS or eNdsSpeciesGrade.SouthernPine_SS => eTimberGrades.SS,

        eNdsSpeciesGrade.DouglasFirLarch_No1 or eNdsSpeciesGrade.HemFir_No1 or eNdsSpeciesGrade.SprucePineFir_No1 or eNdsSpeciesGrade.SouthernPine_No1 => eTimberGrades.No1,

        _ => eTimberGrades.No2,
    };
}
