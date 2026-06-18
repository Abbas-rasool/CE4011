using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Mapping;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.ANALYSIS_CORE.Validation;
using FrameAnalysisProgram.INPUT_OUTPUT;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using Matrix_Library.SOLVERS;

namespace FrameAnalysis.UI.Core.Services;

/// <summary>
/// Async, non-crashing wrapper over the analysis pipeline. It maps the document to the
/// solver DTO on the calling thread (so the background task never touches the mutable,
/// non-thread-safe document), then builds and analyzes on a background thread. Fatal
/// problems — <see cref="StructuralAnalysisException"/>, mapping errors (unassigned
/// references), and builder validation errors — are returned as a failed
/// <see cref="AnalysisOutcome"/> instead of an exception.
///
/// A fresh <see cref="FrameAnalyzer"/> is created per run (the linear solver is stateful),
/// so overlapping runs never share factorization state.
/// </summary>
public sealed class AnalysisService : IAnalysisService
{
    private readonly Func<FrameAnalyzer> _analyzerFactory;
    private readonly StructureModelBuilder _modelBuilder;

    public AnalysisService(Func<FrameAnalyzer> analyzerFactory, StructureModelBuilder? modelBuilder = null)
    {
        _analyzerFactory = analyzerFactory ?? throw new ArgumentNullException(nameof(analyzerFactory));
        _modelBuilder = modelBuilder ?? new StructureModelBuilder();
    }

    /// <summary>Creates a service wired with the standard analyzer (CSparse Cholesky solver).</summary>
    public static AnalysisService CreateDefault() => new(CreateDefaultAnalyzer);

    public Task<AnalysisOutcome> RunAsync(ProjectDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        // Snapshot the document to an immutable DTO here, on the caller's thread, before
        // handing off to the background — the document's collections are not thread-safe.
        StructureInputData input;
        try
        {
            input = ModelInputMapper.ToInputData(document);
        }
        catch (Exception ex)
        {
            return Task.FromResult(Failure(ex.Message));
        }

        return Task.Run(() => Run(input), cancellationToken);
    }

    private AnalysisOutcome Run(StructureInputData input)
    {
        try
        {
            StructureModel model = _modelBuilder.Build(input);
            FrameAnalyzer analyzer = _analyzerFactory();
            FrameAnalysisResult result = analyzer.Analyze(model);
            return new AnalysisOutcome(result, result.ValidationMessages, Fatal: false);
        }
        catch (StructuralAnalysisException ex)
        {
            return new AnalysisOutcome(null, ex.Messages, Fatal: true);
        }
        catch (Exception ex)
        {
            // Builder validation (undefined ids, bad release codes, etc.) and anything else.
            return Failure(ex.Message);
        }
    }

    private static AnalysisOutcome Failure(string message)
        => new(null, new[] { ValidationMessage.Error(message) }, Fatal: true);

    private static FrameAnalyzer CreateDefaultAnalyzer()
    {
        var displacementMapper = new DisplacementMapper();

        return new FrameAnalyzer(
            new DofNumberingService(),
            new GlobalStiffnessAssembler(),
            new LoadVectorBuilder(),
            new SettlementLoadBuilder(),
            new CSparseCholeskySolver(),
            displacementMapper,
            new ElementForceRecovery(displacementMapper),
            new ReactionRecovery(),
            ModelValidator.CreateDefault(),
            new StiffnessSingularityDetector());
    }
}
