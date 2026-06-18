using System.Collections.Generic;
using System.Linq;
using static MemberDesigner.Designers.Enums;

namespace FrameAnalysis.UI.Core.Services;

/// <summary>One design check's outcome for a member (utilization ratio + pass/fail + text).</summary>
public sealed record CheckResult(
    eTimberDesignCheckType CheckType,
    string Title,
    double Utilization,
    eDesignStatus Status,
    string Summary);

/// <summary>
/// All design-check results for a single member (1-based <see cref="ElementId"/>), plus the
/// governing utilization and overall pass/fail rolled up across the checks.
/// </summary>
public sealed record MemberDesignResult(int ElementId, IReadOnlyList<CheckResult> Checks)
{
    /// <summary>Highest utilization ratio across all checks (the governing demand/capacity).</summary>
    public double GoverningUtilization => Checks.Count == 0 ? 0 : Checks.Max(c => c.Utilization);

    /// <summary>Worst status across checks: any Fail → Fail; else any Warning → Warning; else Pass.</summary>
    public eDesignStatus GoverningStatus =>
        Checks.Any(c => c.Status == eDesignStatus.Fail) ? eDesignStatus.Fail
        : Checks.Any(c => c.Status == eDesignStatus.Warning) ? eDesignStatus.Warning
        : eDesignStatus.Pass;

    /// <summary>Check type with the highest utilization (the governing check), if any.</summary>
    public eTimberDesignCheckType? GoverningCheck =>
        Checks.Count == 0 ? null : Checks.OrderByDescending(c => c.Utilization).First().CheckType;
}
