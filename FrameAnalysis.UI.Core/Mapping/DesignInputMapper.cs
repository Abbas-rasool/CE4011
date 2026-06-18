using System;
using System.Linq;
using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysis.UI.Core.Units;
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
        FrameAnalysisResult result,
        UnitSettings units)
    {
        ArgumentNullException.ThrowIfNull(result);
        TimberMemberDesignContext context = BuildBase(memberRow, settings, units);
        ApplyDemands(context, result, elementNumber);
        return context;
    }

    /// <summary>
    /// Builds a context from a pre-combined demand vector (local end forces in canonical kN / kN·m)
    /// and the combination's governing load-duration class — used by the per-combination design
    /// envelope. The duration overrides the project default so the EC5/TR kmod is per-combination,
    /// unless the user has manually fixed the factors.
    /// </summary>
    public static TimberMemberDesignContext BuildContext(
        MemberDesignRowVm memberRow,
        DesignSettings settings,
        UnitSettings units,
        double[] localEndForces,
        DurationFactors durationFactors)
    {
        ArgumentNullException.ThrowIfNull(localEndForces);
        TimberMemberDesignContext context = BuildBase(memberRow, settings, units);
        if (!settings.FactorsModified)
        {
            // EC5/TR read the duration class (→ kmod); US reads C_D (ASD) / λ (LRFD).
            context.LoadDurationClass = durationFactors.DurationClass;
            context.LoadDurationFactor = (float)durationFactors.LoadDurationFactor;
            context.TimeEffectFactor = (float)durationFactors.TimeEffectFactor;
        }
        ApplyDemands(context, localEndForces);
        return context;
    }

    private static TimberMemberDesignContext BuildBase(MemberDesignRowVm memberRow, DesignSettings settings, UnitSettings units)
    {
        ArgumentNullException.ThrowIfNull(memberRow);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(units);

        ElementRowVm element = memberRow.Element
            ?? throw new InvalidOperationException("Member design row has no element assigned.");

        MaterialRowVm material = element.Material
            ?? throw new InvalidOperationException($"{element} has no material assigned.");

        SectionRowVm section = element.Section
            ?? throw new InvalidOperationException($"{element} has no section assigned.");

        var context = new TimberMemberDesignContext { Code = settings.Code };

        ApplyMaterial(context, material, settings.Code);
        ApplyGeometry(context, section, memberRow.NetAreaFactor, units);
        ApplyMember(context, memberRow);
        ApplySettings(context, settings);

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

    private static void ApplyGeometry(TimberMemberDesignContext c, SectionRowVm s, double netAreaFactor, UnitSettings units)
    {
        // Major axis = depth, minor axis = width (rectangular section). Section dimensions are
        // stored in the user's chosen unit; the design backend expects mm.
        double toMm = units.SectionToM * UnitSystem.MToMm; // chosen section unit → m → mm
        float depth = (float)(s.Depth * toMm);
        float width = (float)(s.Width * toMm);
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
        // Effective lengths are entered in m; the design backend works in mm.
        c.EffectiveLengthMajor = (float)(r.EffectiveLengthMajor * UnitSystem.MToMm);
        c.EffectiveLengthMinor = (float)(r.EffectiveLengthMinor * UnitSystem.MToMm);
        c.EffectiveBeamLength = (float)(r.EffectiveBeamLength * UnitSystem.MToMm);
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
        c.DesignMethod = s.UsDesignMethod; // US: ASD vs LRFD
    }

    private static void ApplyDemands(TimberMemberDesignContext c, FrameAnalysisResult result, int elementNumber)
    {
        // Prefer the per-member station envelope: the exact worst section force anywhere
        // along the span (e.g. the mid-span moment under a UDL, not just the end values).
        // Fall back to the member-end forces when no station data is available.
        MemberStationResult? stations = result.MemberStations.FirstOrDefault(s => s.ElementId == elementNumber);
        if (stations is not null && stations.X.Count > 0)
        {
            ApplyDemands(c, stations);
            return;
        }

        ElementEndForceResult? forces = result.ElementEndForces.FirstOrDefault(e => e.Element.Id == elementNumber);
        if (forces is null) return; // no demands (e.g. element not in the solved model)
        ApplyDemands(c, forces.LocalEndForces);
    }

    /// <summary>Sets the demand fields from the per-member station envelope (canonical
    /// kN / kN·m), converting to the design backend's N / N·mm.</summary>
    private static void ApplyDemands(TimberMemberDesignContext c, MemberStationResult s)
    {
        double maxN = double.NegativeInfinity, minN = double.PositiveInfinity;
        foreach (double n in s.Axial)
        {
            if (n > maxN) maxN = n;
            if (n < minN) minN = n;
        }

        // Axial is tension-positive; route the worst of each sign to its demand.
        c.AxialTension = (float)(Math.Max(maxN, 0.0) * UnitSystem.KNToN);
        c.AxialCompression = (float)(Math.Max(-minN, 0.0) * UnitSystem.KNToN);
        c.Shear = (float)(s.MaxAbsShear * UnitSystem.KNToN);
        c.MomentMajor = (float)(s.MaxAbsMoment * UnitSystem.KNmToNmm);
        c.MomentMinor = 0f; // out-of-plane is zero in the 2D solver
    }

    /// <summary>Sets the demand fields from a local end-force vector [Fx1,Fy1,Mz1,Fx2,Fy2,Mz2]
    /// (canonical kN / kN·m), converting to the design backend's N / N·mm.</summary>
    private static void ApplyDemands(TimberMemberDesignContext c, double[] f)
    {
        if (f.Length == 0) return;
        bool isFrame = f.Length == 6;

        double fx1 = f[0];
        double fy1 = f[1];
        double mz1 = isFrame ? f[2] : 0.0;
        double fy2 = isFrame ? f[4] : f[3];
        double mz2 = isFrame ? f[5] : 0.0;

        // Axial: tension positive = -Fx1 (= Fx2 for a member with no axial span load).
        double axialTension = -fx1;

        c.AxialTension = (float)(Math.Max(axialTension, 0.0) * UnitSystem.KNToN);
        c.AxialCompression = (float)(Math.Max(-axialTension, 0.0) * UnitSystem.KNToN);
        c.Shear = (float)(Math.Max(Math.Abs(fy1), Math.Abs(fy2)) * UnitSystem.KNToN);
        c.MomentMajor = (float)(Math.Max(Math.Abs(mz1), Math.Abs(mz2)) * UnitSystem.KNmToNmm);
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
