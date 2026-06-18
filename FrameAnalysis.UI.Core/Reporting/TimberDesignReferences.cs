using System.Collections.Generic;
using static MemberDesigner.Designers.Enums;

namespace FrameAnalysis.UI.Core.Reporting;

/// <summary>
/// Static catalog of the symbolic design formula and the standard clause reference behind each
/// timber member check, per code. This supplies only the things a check's <c>GetSummary()</c>
/// does not: the symbolic utilization equation and where to find it in the standard. The
/// numeric values come from the design run (CheckResult.Summary, i.e. the checks' GetSummary).
/// </summary>
public static class TimberDesignReferences
{
    /// <summary>One catalog entry: the symbolic utilization formula and its standard clause.</summary>
    public readonly record struct Entry(string Formula, string Reference);

    private static readonly Dictionary<(eTimberCode, eTimberDesignCheckType), Entry> Catalog = new()
    {
        // --- US — NDS 2018 ---
        [(eTimberCode.US, eTimberDesignCheckType.Tension)] = new(
            "ft = T / An  ≤  Ft′ = Ft·CD·CM·Ct·CF·Ci",
            "NDS 2018, §3.8.1"),
        [(eTimberCode.US, eTimberDesignCheckType.Compression)] = new(
            "fc = P / Ag  ≤  Fc′ = Fc·CD·CM·Ct·CF·Ci·CP",
            "NDS 2018, §3.6–3.7 (CP per §3.7.1)"),
        [(eTimberCode.US, eTimberDesignCheckType.Bending)] = new(
            "fb = M / S  ≤  Fb′ = Fb·CD·CM·Ct·CL·CF·Cr",
            "NDS 2018, §3.3.1"),
        [(eTimberCode.US, eTimberDesignCheckType.Shear)] = new(
            "fv = 1.5·V / A  ≤  Fv′ = Fv·CD·CM·Ct",
            "NDS 2018, §3.4.2"),
        [(eTimberCode.US, eTimberDesignCheckType.CombinedBendingAxial)] = new(
            "(fc / Fc′)² + fb / [Fb′·(1 − fc / FcE)]  ≤  1.0",
            "NDS 2018, §3.9.2 (Eq. 3.9-3)"),

        // --- EC5 — EN 1995-1-1:2004 ---
        [(eTimberCode.EC5, eTimberDesignCheckType.Tension)] = new(
            "σt,0,d / ft,0,d  ≤  1.0 ,   ft,0,d = kmod·ft,0,k / γM",
            "EN 1995-1-1, §6.1.2 (Eq. 6.1)"),
        [(eTimberCode.EC5, eTimberDesignCheckType.Compression)] = new(
            "σc,0,d / (kc·fc,0,d)  ≤  1.0 ,   fc,0,d = kmod·fc,0,k / γM",
            "EN 1995-1-1, §6.3.2 (Eq. 6.23/6.24)"),
        [(eTimberCode.EC5, eTimberDesignCheckType.Bending)] = new(
            "σm,y,d / fm,y,d + km·σm,z,d / fm,z,d  ≤  1.0",
            "EN 1995-1-1, §6.1.6 (Eq. 6.11/6.12)"),
        [(eTimberCode.EC5, eTimberDesignCheckType.Shear)] = new(
            "τd / fv,d  ≤  1.0 ,   fv,d = kmod·fv,k / γM",
            "EN 1995-1-1, §6.1.7 (Eq. 6.13)"),
        [(eTimberCode.EC5, eTimberDesignCheckType.CombinedBendingAxial)] = new(
            "σc,0,d / (kc·fc,0,d) + σm,y,d / fm,y,d + km·σm,z,d / fm,z,d  ≤  1.0",
            "EN 1995-1-1, §6.3.2 (Eq. 6.23/6.24)"),

        // --- TR — TS 647 (limit-state form: ω safety factor, C-modification factors) ---
        [(eTimberCode.TR, eTimberDesignCheckType.Tension)] = new(
            "σt / ft,d  ≤  1.0 ,   ft,d = ft·CN·CB·CY / ω",
            "TS 647 — Timber Structures"),
        [(eTimberCode.TR, eTimberDesignCheckType.Compression)] = new(
            "σc / fc,d  ≤  1.0 ,   fc,d = Cyb·fc·CN·CB·CY / ω",
            "TS 647 — Timber Structures"),
        [(eTimberCode.TR, eTimberDesignCheckType.Bending)] = new(
            "σm / fm,d  ≤  1.0 ,   fm,d = Cyb·fm·CN·CB·CY / ω",
            "TS 647 — Timber Structures"),
        [(eTimberCode.TR, eTimberDesignCheckType.Shear)] = new(
            "τ / fv,d  ≤  1.0 ,   fv,d = fv·CN·CB·CY / ω",
            "TS 647 — Timber Structures"),
        [(eTimberCode.TR, eTimberDesignCheckType.CombinedBendingAxial)] = new(
            "σc / fc,d + σm / fm,d  ≤  1.0",
            "TS 647 — Timber Structures"),
    };

    /// <summary>The formula + reference for a check, or an empty entry if not catalogued.</summary>
    public static Entry Get(eTimberCode code, eTimberDesignCheckType checkType) =>
        Catalog.TryGetValue((code, checkType), out var entry) ? entry : new Entry("—", "");
}
