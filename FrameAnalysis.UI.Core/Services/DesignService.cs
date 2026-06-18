using System;
using System.Collections.Generic;
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
            List<MemberDesignResult> results = await Task.Run(() => RunChecks(jobs, cancellationToken), cancellationToken);
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
                TimberMemberDesignContext ctx = DesignInputMapper.BuildContext(memberRow, elementNumber, document.Design, result);

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
        CancellationToken cancellationToken)
    {
        var results = new List<MemberDesignResult>();

        foreach ((int elementNumber, TimberMemberDesignContext ctx) in jobs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // A fresh provider/factory/designer per member (the context differs each time).
            var checkTypeProvider = new TimberCheckTypeProvider();
            var provider = new TimberDesignCheckProvider(checkTypeProvider, ctx.Code);
            var factory = new TimberDesignCheckInputFactory(checkTypeProvider, ctx);
            var designer = new ATimberDesigner(provider, factory);

            List<TimberDesignCheckData> data = await designer.CheckDesignAsync(cancellationToken).ConfigureAwait(false);
            results.Add(ToMemberResult(elementNumber, data));
        }

        return results;
    }

    private static MemberDesignResult ToMemberResult(int elementNumber, List<TimberDesignCheckData> data)
    {
        var checks = new List<CheckResult>();

        foreach (TimberDesignCheckData d in data)
        {
            if (d is null || d.CheckType == eTimberDesignCheckType.Parameters)
                continue; // Parameters is a dependency, not a capacity check — don't display it.

            checks.Add(new CheckResult(
                d.CheckType,
                SafeTitle(d),
                SafeRatio(d),
                d.DesignStatus,
                SafeSummary(d)));
        }

        return new MemberDesignResult(elementNumber, checks);
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

    private static double SafeRatio(TimberDesignCheckData d)
    {
        try
        {
            double r = d.GetUtilizationRatio();
            return double.IsFinite(r) ? r : 0;
        }
        catch { return 0; }
    }

    private static DesignOutcome Failure(string message)
        => new(Array.Empty<MemberDesignResult>(), new[] { ValidationMessage.Error(message) }, Fatal: true);
}
