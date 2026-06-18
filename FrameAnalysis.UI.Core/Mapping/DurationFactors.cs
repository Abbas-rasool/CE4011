using static MemberDesigner.Designers.Enums;

namespace FrameAnalysis.UI.Core.Mapping;

/// <summary>
/// The per-combination load-duration adjustment applied to a design context. EC5/TR use
/// <see cref="DurationClass"/> (which drives kmod); US/NDS uses <see cref="LoadDurationFactor"/>
/// (C_D, for ASD) or <see cref="TimeEffectFactor"/> (λ, for LRFD). Fields not relevant to the
/// active code stay at their neutral values (1.0 / Permanent).
/// </summary>
public readonly record struct DurationFactors(
    eLoadDurationClass DurationClass,
    double LoadDurationFactor,
    double TimeEffectFactor)
{
    public static readonly DurationFactors Default =
        new(eLoadDurationClass.PermanentAction, 1.0, 1.0);
}
