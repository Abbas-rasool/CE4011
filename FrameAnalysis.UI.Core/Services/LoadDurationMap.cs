using System;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysis.UI.Core.Mapping;
using StructuralLoads;
using static MemberDesigner.Designers.Enums;

namespace FrameAnalysis.UI.Core.Services;

/// <summary>
/// Resolves a combination's per-code load-duration adjustment. EC5/TR: the governing
/// <see cref="eLoadDurationClass"/> (the action with the <b>shortest</b> duration, EC5 3.1.3(2),
/// → kmod). US/NDS: the load-duration factor C_D (ASD, Table 2.3.2) or the time-effect factor λ
/// (LRFD, Appendix N) for the shortest-duration action. In every case the shortest-duration
/// (highest-value) participating action governs.
/// </summary>
public static class LoadDurationMap
{
    /// <summary>The duration adjustment for a combination under the active design code.</summary>
    public static DurationFactors For(LoadCombinationRowVm combo, eTimberCode code, eLoadCombinationType usMethod)
    {
        if (code == eTimberCode.US)
        {
            return usMethod == eLoadCombinationType.LRFD
                ? new DurationFactors(eLoadDurationClass.PermanentAction, 1.0, GoverningTimeEffectFactor(combo))
                : new DurationFactors(eLoadDurationClass.PermanentAction, GoverningLoadDurationFactor(combo), 1.0);
        }
        return new DurationFactors(GoverningDuration(combo), 1.0, 1.0); // EC5 / TR (kmod path)
    }

    // --- NDS load-duration factor C_D (ASD, Table 2.3.2) ---
    public static double NdsLoadDurationFactor(eLoadNature nature) => nature switch
    {
        eLoadNature.Dead => 0.9,
        eLoadNature.Live => 1.0,
        eLoadNature.RoofLive => 1.25,
        eLoadNature.Snow => 1.15,
        eLoadNature.Wind => 1.6,
        eLoadNature.Rain => 1.0,
        eLoadNature.Seismic => 1.6,
        eLoadNature.Thermal => 1.0,
        _ => 1.0
    };

    public static double GoverningLoadDurationFactor(LoadCombinationRowVm combo)
    {
        ArgumentNullException.ThrowIfNull(combo);
        double cd = 0.9; // permanent floor
        foreach (eLoadNature nature in Enum.GetValues<eLoadNature>())
            if (combo.FactorFor(nature) != 0.0)
                cd = Math.Max(cd, NdsLoadDurationFactor(nature));
        return cd;
    }

    // --- NDS time-effect factor λ (LRFD, Appendix N) ---
    public static double NdsTimeEffectFactor(eLoadNature nature) => nature switch
    {
        eLoadNature.Dead => 0.6,
        eLoadNature.Live => 0.8,
        eLoadNature.RoofLive => 0.8,
        eLoadNature.Snow => 0.8,
        eLoadNature.Wind => 1.0,
        eLoadNature.Rain => 0.8,
        eLoadNature.Seismic => 1.0,
        eLoadNature.Thermal => 1.0,
        _ => 0.8
    };

    public static double GoverningTimeEffectFactor(LoadCombinationRowVm combo)
    {
        ArgumentNullException.ThrowIfNull(combo);
        double lambda = 0.6; // permanent floor
        foreach (eLoadNature nature in Enum.GetValues<eLoadNature>())
            if (combo.FactorFor(nature) != 0.0)
                lambda = Math.Max(lambda, NdsTimeEffectFactor(nature));
        return lambda;
    }

    public static eLoadDurationClass DurationOf(eLoadNature nature) => nature switch
    {
        eLoadNature.Dead => eLoadDurationClass.PermanentAction,
        eLoadNature.Live => eLoadDurationClass.MediumTermAction,
        eLoadNature.RoofLive => eLoadDurationClass.ShortTermAction,
        eLoadNature.Snow => eLoadDurationClass.ShortTermAction,
        eLoadNature.Wind => eLoadDurationClass.ShortTermAction,
        eLoadNature.Rain => eLoadDurationClass.ShortTermAction,
        eLoadNature.Seismic => eLoadDurationClass.InstantaneousAction,
        eLoadNature.Thermal => eLoadDurationClass.ShortTermAction,
        _ => eLoadDurationClass.MediumTermAction
    };

    /// <summary>The shortest-duration action participating in the combination (defaults to
    /// Permanent for an empty / dead-only combination).</summary>
    public static eLoadDurationClass GoverningDuration(LoadCombinationRowVm combo)
    {
        ArgumentNullException.ThrowIfNull(combo);

        eLoadDurationClass governing = eLoadDurationClass.PermanentAction;
        foreach (eLoadNature nature in Enum.GetValues<eLoadNature>())
        {
            if (combo.FactorFor(nature) == 0.0) continue;
            eLoadDurationClass d = DurationOf(nature);
            if (d > governing) governing = d; // higher enum = shorter duration = governs
        }
        return governing;
    }
}
