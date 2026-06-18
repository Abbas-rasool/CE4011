using System.Collections.Generic;
using FrameAnalysis.UI.Core.Documents;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.ANALYSIS_CORE.Validation;

namespace FrameAnalysis.UI.Core.Services;

/// <summary>
/// The outcome of a design run. <see cref="Results"/> holds one entry per designed member;
/// <see cref="Messages"/> carries readiness warnings / errors (missing grade, unassigned
/// section, "run analysis first", per-member failures). The service never throws to the UI.
/// </summary>
public sealed record DesignOutcome(
    IReadOnlyList<MemberDesignResult> Results,
    IReadOnlyList<ValidationMessage> Messages,
    bool Fatal);

/// <summary>
/// Runs the timber member design off the UI thread without ever throwing. Requires a current
/// analysis result to supply member demands.
/// </summary>
public interface IDesignService
{
    Task<DesignOutcome> RunAsync(
        ProjectDocument document,
        FrameAnalysisResult? analysisResult,
        CancellationToken cancellationToken = default);
}
