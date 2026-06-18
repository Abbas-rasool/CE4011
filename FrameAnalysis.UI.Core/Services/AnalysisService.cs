using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Mapping;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.ANALYSIS_CORE.Validation;
using FrameAnalysisProgram.INPUT_OUTPUT;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using Matrix_Library.SOLVERS;
using StructuralLoads;

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

    public Task<SuperpositionBasis> RunPerNatureAsync(
        ProjectDocument document, IReadOnlySet<eLoadNature> natures, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(natures);

        // Map each isolated load case on the caller thread (the document is not thread-safe).
        var natureInputs = new Dictionary<eLoadNature, StructureInputData>();
        foreach (eLoadNature n in natures)
            natureInputs[n] = ModelInputMapper.ToInputData(document, LoadScope.Nature(n));

        StructureInputData? settlementInput = document.Settlements.Count > 0
            ? ModelInputMapper.ToInputData(document, LoadScope.SettlementsOnly)
            : null;

        return Task.Run(() =>
        {
            var perNature = new Dictionary<eLoadNature, IReadOnlyDictionary<int, double[]>>();
            foreach (var (nature, input) in natureInputs)
                perNature[nature] = SolveEndForces(input);

            IReadOnlyDictionary<int, double[]>? settlement =
                settlementInput is not null ? SolveEndForces(settlementInput) : null;

            return new SuperpositionBasis(perNature, settlement);
        }, cancellationToken);
    }

    /// <summary>Solves one input and returns each element's local end-force vector by element id.</summary>
    private IReadOnlyDictionary<int, double[]> SolveEndForces(StructureInputData input)
    {
        StructureModel model = _modelBuilder.Build(input);
        FrameAnalyzer analyzer = _analyzerFactory();
        FrameAnalysisResult result = analyzer.Analyze(model);

        var map = new Dictionary<int, double[]>();
        foreach (var ef in result.ElementEndForces)
            map[ef.Element.Id] = (double[])ef.LocalEndForces.Clone();
        return map;
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
            new StiffnessSingularityDetector(),
            new SectionForceRecovery());
    }
}
