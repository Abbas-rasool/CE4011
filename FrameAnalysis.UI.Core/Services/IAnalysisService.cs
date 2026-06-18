using FrameAnalysis.UI.Core.Documents;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.ANALYSIS_CORE.Validation;
using StructuralLoads;

namespace FrameAnalysis.UI.Core.Services;

/// <summary>
/// The outcome of an analysis run. On success <see cref="Result"/> is set and
/// <see cref="Fatal"/> is false (any <see cref="Messages"/> are warnings / info, e.g. a
/// static-indeterminacy note). On failure <see cref="Result"/> is null, <see cref="Fatal"/>
/// is true, and <see cref="Messages"/> explain why — the service never throws to the UI.
/// </summary>
public sealed record AnalysisOutcome(
    FrameAnalysisResult? Result,
    IReadOnlyList<ValidationMessage> Messages,
    bool Fatal);

/// <summary>Runs a structural analysis off the UI thread without ever throwing.</summary>
public interface IAnalysisService
{
    Task<AnalysisOutcome> RunAsync(ProjectDocument document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves the model once per supplied load nature (and once for settlements, if any), each
    /// in isolation, returning the basis for superposing combination demands. The structure is
    /// the same for every case, so stability is load-independent. Throws on an unsolvable model.
    /// </summary>
    Task<SuperpositionBasis> RunPerNatureAsync(
        ProjectDocument document, IReadOnlySet<eLoadNature> natures, CancellationToken cancellationToken = default);
}
