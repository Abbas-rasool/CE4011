using System;
using System.Collections.Generic;
using System.Linq;
using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysis.UI.Core.Mapping;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.ANALYSIS_CORE.Validation;
using MemberDesigner.Designers;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace FrameAnalysis.UI.Core.Services;

/// <summary>
/// Async, non-crashing wrapper over the timber design pipeline. It snapshots the document into
/// per-member <see cref="TimberMemberDesignContext"/> objects on the calling thread (the
/// document is not thread-safe), then runs <see cref="ATimberDesigner.CheckDesignAsync"/> per
/// member on a background thread. Readiness problems and per-member failures come back as
/// <see cref="ValidationMessage"/>s rather than exceptions.
/// </summary>
public sealed class DesignService : IDesignService
{
    public async Task<DesignOutcome> RunAsync(
        ProjectDocument document,
        FrameAnalysisResult? analysisResult,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (analysisResult is null)
            return Failure("Run analysis before running design — member demands come from the analysis result.");

        // Snapshot to immutable contexts here, on the caller's thread.
        var jobs = new List<(int elementNumber, TimberMemberDesignContext ctx)>();

        var messages = new List<ValidationMessage>();
        BuildJobs(document, analysisResult, jobs, messages);

        if (jobs.Count == 0)
        {
            messages.Add(ValidationMessage.Warning("No designable members — add elements with a material and section."));
            return new DesignOutcome(Array.Empty<MemberDesignResult>(), messages, Fatal: false);
        }

        try
        {
            var skipped = new HashSet<eTimberDesignCheckType>();
            List<MemberDesignResult> results = await Task.Run(() => RunChecks(jobs, skipped, cancellationToken), cancellationToken);
            AddSkippedMessage(messages, skipped);
            return new DesignOutcome(results, messages, Fatal: false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            messages.Add(ValidationMessage.Error($"Design run failed: {ex.Message}"));
            return new DesignOutcome(Array.Empty<MemberDesignResult>(), messages, Fatal: true);
        }
    }

    public async Task<DesignOutcome> RunEnvelopeAsync(
        ProjectDocument document,
        SuperpositionBasis basis,
        IReadOnlyList<LoadCombinationRowVm> ulsCombinations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(basis);
        ArgumentNullException.ThrowIfNull(ulsCombinations);

        if (ulsCombinations.Count == 0)
            return new DesignOutcome(Array.Empty<MemberDesignResult>(),
                new[] { ValidationMessage.Warning("No ultimate (ULS) load combinations — generate or add some on the Load Combinations sheet.") },
                Fatal: false);

        // Snapshot member × combination contexts on the caller thread (the document isn't thread-safe).
        var messages = new List<ValidationMessage>();
        var jobs = new List<MemberEnvelopeJob>();

        foreach (MemberDesignRowVm memberRow in document.MemberDesigns)
        {
            if (memberRow.Element is null) continue;
            int index = document.Elements.IndexOf(memberRow.Element);
            if (index < 0) continue;
            int elementNumber = index + 1;

            var comboContexts = new List<(string Name, TimberMemberDesignContext Ctx)>();
            bool failed = false;
            foreach (LoadCombinationRowVm combo in ulsCombinations)
            {
                double[] demand = basis.CombinedEndForces(elementNumber, combo);
                DurationFactors duration = LoadDurationMap.For(combo, document.Design.Code, document.Design.UsDesignMethod);
                try
                {
                    TimberMemberDesignContext ctx =
                        DesignInputMapper.BuildContext(memberRow, document.Design, document.Units, demand, duration);
                    comboContexts.Add((combo.Name, ctx));
                }
                catch (Exception ex)
                {
                    messages.Add(ValidationMessage.Error($"Member {elementNumber}: {ex.Message}"));
                    failed = true;
                    break;
                }
            }
            if (failed || comboContexts.Count == 0) continue;

            // Material strengths don't vary per combination — validate once.
            if (!HasRequiredDesignValues(comboContexts[0].Ctx))
            {
                string materialName = memberRow.Element?.Material?.ToString() ?? "?";
                messages.Add(ValidationMessage.Error(
                    $"Member {elementNumber}: material '{materialName}' is missing design values — pick a grade " +
                    "(or tick Override and enter them). Required for design."));
                continue;
            }

            jobs.Add(new MemberEnvelopeJob(elementNumber, comboContexts));
        }

        if (jobs.Count == 0)
        {
            if (messages.Count == 0)
                messages.Add(ValidationMessage.Warning("No designable members — add elements with a material and section."));
            return new DesignOutcome(Array.Empty<MemberDesignResult>(), messages, Fatal: false);
        }

        try
        {
            var skipped = new HashSet<eTimberDesignCheckType>();
            List<MemberDesignResult> results = await Task.Run(() => RunEnvelopeChecks(jobs, skipped, cancellationToken), cancellationToken);
            AddSkippedMessage(messages, skipped);
            return new DesignOutcome(results, messages, Fatal: false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            messages.Add(ValidationMessage.Error($"Design run failed: {ex.Message}"));
            return new DesignOutcome(Array.Empty<MemberDesignResult>(), messages, Fatal: true);
        }
    }

    private sealed record MemberEnvelopeJob(int ElementNumber, List<(string Name, TimberMemberDesignContext Ctx)> Combos);

    private static async Task<List<MemberDesignResult>> RunEnvelopeChecks(
        List<MemberEnvelopeJob> jobs, ISet<eTimberDesignCheckType> skipped, CancellationToken cancellationToken)
    {
        var results = new List<MemberDesignResult>();
        foreach (MemberEnvelopeJob job in jobs)
        {
            // Worst (max) utilization per check type across the combinations, with its combo.
            var worst = new Dictionary<eTimberDesignCheckType, CheckResult>();
            foreach ((string name, TimberMemberDesignContext ctx) in job.Combos)
            {
                cancellationToken.ThrowIfCancellationRequested();
                List<TimberDesignCheckData> data = await RunChecksForContext(ctx, cancellationToken).ConfigureAwait(false);
                foreach (CheckResult cr in ToCheckResults(data, name, skipped))
                {
                    if (!worst.TryGetValue(cr.CheckType, out CheckResult? existing) || cr.Utilization > existing.Utilization)
                        worst[cr.CheckType] = cr;
                }
            }
            results.Add(new MemberDesignResult(job.ElementNumber, worst.Values.OrderBy(c => c.CheckType).ToList()));
        }
        return results;
    }

    private static void BuildJobs(
        ProjectDocument document,
        FrameAnalysisResult result,
        List<(int, TimberMemberDesignContext)> jobs,
        List<ValidationMessage> messages)
    {
        foreach (MemberDesignRowVm memberRow in document.MemberDesigns)
        {
            if (memberRow.Element is null) continue;

            int index = document.Elements.IndexOf(memberRow.Element);
            if (index < 0) continue;
            int elementNumber = index + 1;

            try
            {
                TimberMemberDesignContext ctx = DesignInputMapper.BuildContext(memberRow, elementNumber, document.Design, result, document.Units);

                // Design strength values are mandatory for a design run — skip members whose
                // material isn't graded (or manually filled) rather than report bogus ratios.
                if (!HasRequiredDesignValues(ctx))
                {
                    string materialName = memberRow.Element?.Material?.ToString() ?? "?";
                    messages.Add(ValidationMessage.Error(
                        $"Member {elementNumber}: material '{materialName}' is missing design values — pick a grade " +
                        "(or tick Override and enter them). Required for design."));
                    continue;
                }

                jobs.Add((elementNumber, ctx));
            }
            catch (Exception ex)
            {
                messages.Add(ValidationMessage.Error($"Member {elementNumber}: {ex.Message}"));
            }
        }
    }

    /// <summary>The core strength values a member needs before design can run meaningfully.</summary>
    private static bool HasRequiredDesignValues(TimberMemberDesignContext c)
        => c.BendingStrength > 0 && c.TensionStrength > 0 && c.CompressionStrength > 0
           && c.ShearStrength > 0 && c.ModulusMean > 0;

    private static async Task<List<MemberDesignResult>> RunChecks(
        List<(int elementNumber, TimberMemberDesignContext ctx)> jobs,
        ISet<eTimberDesignCheckType> skipped,
        CancellationToken cancellationToken)
    {
        var results = new List<MemberDesignResult>();

        foreach ((int elementNumber, TimberMemberDesignContext ctx) in jobs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<TimberDesignCheckData> data = await RunChecksForContext(ctx, cancellationToken).ConfigureAwait(false);
            results.Add(new MemberDesignResult(elementNumber, ToCheckResults(data, comboName: "", skipped)));
        }

        return results;
    }

    /// <summary>Runs all design checks for one prepared context (fresh provider/factory/designer).</summary>
    private static async Task<List<TimberDesignCheckData>> RunChecksForContext(
        TimberMemberDesignContext ctx, CancellationToken cancellationToken)
    {
        var checkTypeProvider = new TimberCheckTypeProvider();
        var provider = new TimberDesignCheckProvider(checkTypeProvider, ctx.Code);
        var factory = new TimberDesignCheckInputFactory(checkTypeProvider, ctx);
        var designer = new ATimberDesigner(provider, factory);
        return await designer.CheckDesignAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Converts raw check data to display results, tagging each with the governing
    /// combination (empty for a single-state run). Parameters is a dependency, not a capacity
    /// check. A check whose utilization can't be evaluated — e.g. not implemented for the active
    /// design code (it throws), or non-finite — is recorded in <paramref name="skipped"/> and
    /// omitted, rather than shown as a misleading "0% Fail".</summary>
    private static List<CheckResult> ToCheckResults(
        List<TimberDesignCheckData> data, string comboName, ISet<eTimberDesignCheckType> skipped)
    {
        var results = new List<CheckResult>();
        foreach (TimberDesignCheckData d in data)
        {
            if (d is null || d.CheckType == eTimberDesignCheckType.Parameters)
                continue;
            if (TryGetRatio(d, out double ratio))
                results.Add(new CheckResult(d.CheckType, SafeTitle(d), ratio, d.DesignStatus, SafeSummary(d), comboName));
            else
                skipped.Add(d.CheckType);
        }
        return results;
    }

    // Some check-data classes leave secondary text methods unimplemented; never let that
    // break the results panel.
    private static string SafeTitle(TimberDesignCheckData d)
    {
        try { return d.GetTitle() ?? string.Empty; } catch { return d.CheckType.ToString(); }
    }

    private static string SafeSummary(TimberDesignCheckData d)
    {
        try { return d.GetSummary() ?? string.Empty; } catch { return string.Empty; }
    }

    /// <summary>True with a finite utilization ratio; false when the check can't be evaluated
    /// (not implemented for the active code → throws, or returned a non-finite value).</summary>
    private static bool TryGetRatio(TimberDesignCheckData d, out double ratio)
    {
        ratio = 0;
        try
        {
            double r = d.GetUtilizationRatio();
            if (!double.IsFinite(r)) return false;
            ratio = r;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Adds one note listing check types that couldn't be evaluated for the active code.</summary>
    private static void AddSkippedMessage(List<ValidationMessage> messages, ISet<eTimberDesignCheckType> skipped)
    {
        if (skipped.Count == 0) return;
        string names = string.Join(", ", skipped.OrderBy(s => s));
        messages.Add(ValidationMessage.Warning(
            $"Not evaluated for this design code (not implemented yet): {names}. These checks were excluded — verify them separately."));
    }

    private static DesignOutcome Failure(string message)
        => new(Array.Empty<MemberDesignResult>(), new[] { ValidationMessage.Error(message) }, Fatal: true);
}
