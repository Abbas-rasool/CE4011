using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Documents.Rows;
using FrameAnalysis.UI.Core.Rendering;
using FrameAnalysis.UI.Core.Services;
using FrameAnalysisProgram.ANALYSIS_CORE;

namespace FrameAnalysis.UI.Core.ViewModels;

/// <summary>
/// The application's central coordinator. Owns the editable <see cref="Document"/>, keeps a
/// render-ready <see cref="CurrentScene"/> in sync with every edit (live geometry), runs the
/// analysis off the UI thread, and holds the shared selection. No WPF and no drawing live
/// here — the window binds to this; the renderer consumes <see cref="CurrentScene"/>.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly IAnalysisService _analysisService;
    private readonly IDesignService _designService;

    public MainViewModel(IAnalysisService analysisService, ProjectDocument? document = null, IDesignService? designService = null)
    {
        _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
        _designService = designService ?? new DesignService();
        Document = document ?? new ProjectDocument();
        Document.Changed += OnDocumentChanged;
        RebuildScene();
    }

    /// <summary>The single source of truth the grids bind to.</summary>
    public ProjectDocument Document { get; }

    /// <summary>The render-ready snapshot the renderer draws. Rebuilt on every edit.</summary>
    [ObservableProperty]
    private Scene currentScene = Scene.Empty;

    /// <summary>The element selected in the list or by clicking the canvas (hybrid selection).</summary>
    [ObservableProperty]
    private int? selectedElementId;

    /// <summary>Which input/results sheet the left pane is currently showing.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddRowCommand))]
    private Sheet currentSheet = Sheet.Nodes;

    /// <summary>The most recent analysis outcome (result + messages), or null if never run.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResult))]
    [NotifyCanExecuteChangedFor(nameof(RunDesignCommand))]
    private AnalysisOutcome? lastOutcome;

    /// <summary>The most recent design outcome (per-member results + messages), or null if never run.</summary>
    [ObservableProperty]
    private DesignOutcome? lastDesignOutcome;

    /// <summary>
    /// True once the document is edited after a run: the numbers in <see cref="LastOutcome"/>
    /// no longer match the model, so the deflected overlay is dropped while they are kept for
    /// the results panel until the next run.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunDesignCommand))]
    private bool resultsAreStale;

    /// <summary>True while an analysis is running.</summary>
    [ObservableProperty]
    private bool isBusy;

    /// <summary>Amplification applied to the deflected shape so small displacements are visible.</summary>
    [ObservableProperty]
    private double deflectionScale = 1.0;

    /// <summary>Whether a current (non-stale) analysis result is available to display.</summary>
    public bool HasResult => !ResultsAreStale && LastOutcome?.Result is not null;

    [RelayCommand]
    private async Task RunAnalysisAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        try
        {
            // The service snapshots the document before going to the background, so it is
            // safe to call from here and never throws.
            LastOutcome = await _analysisService.RunAsync(Document, cancellationToken);
            ResultsAreStale = false;
            RebuildScene();
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Runs the timber design off the UI thread. Enabled only when a current
    /// analysis result is available (demands come from it).</summary>
    [RelayCommand(CanExecute = nameof(HasResult))]
    private async Task RunDesignAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        try
        {
            FrameAnalysisResult? result = HasResult ? LastOutcome!.Result : null;
            LastDesignOutcome = await _designService.RunAsync(Document, result, cancellationToken);
            CurrentSheet = Sheet.DesignResults;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ShowSheet(Sheet sheet) => CurrentSheet = sheet;

    /// <summary>Add is available only on the editable data sheets (member-design rows are
    /// auto-synced from elements, so they are not user-added either).</summary>
    private bool CanAddRow => CurrentSheet is not (
        Sheet.Results or Sheet.LoadCases or Sheet.LoadCombinations
        or Sheet.DesignSettings or Sheet.MemberDesign or Sheet.DesignResults);

    /// <summary>
    /// Appends a blank row to the collection backing the current sheet. Going through the
    /// document's collection (rather than the DataGrid's new-row placeholder) guarantees the
    /// row is a real, change-tracked item, so edits to it update the canvas live.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddRow))]
    private void AddRow()
    {
        switch (CurrentSheet)
        {
            case Sheet.Nodes: Document.Nodes.Add(new NodeRowVm()); break;
            case Sheet.Elements: Document.Elements.Add(new ElementRowVm()); break;
            case Sheet.Materials: Document.Materials.Add(new MaterialRowVm()); break;
            case Sheet.Sections: Document.Sections.Add(new SectionRowVm()); break;
            case Sheet.Supports: Document.Supports.Add(new SupportRowVm()); break;
            case Sheet.JointLoads: Document.NodalLoads.Add(new NodalLoadRowVm()); break;
            case Sheet.DistributedLoads: Document.DistributedLoads.Add(new DistributedLoadRowVm()); break;
            case Sheet.PointLoads: Document.PointLoads.Add(new PointLoadRowVm()); break;
            case Sheet.Settlements: Document.Settlements.Add(new SettlementRowVm()); break;
            case Sheet.ThermalLoads: Document.TemperatureLoads.Add(new TemperatureLoadRowVm()); break;
        }
    }

    private void OnDocumentChanged(object? sender, EventArgs e)
    {
        // Any edit invalidates the solved overlay. Keep the last numbers for the results
        // panel but flag them stale and redraw the (static) geometry only.
        if (!ResultsAreStale && LastOutcome is not null)
            ResultsAreStale = true;

        RebuildScene();
    }

    partial void OnDeflectionScaleChanged(double value) => RebuildScene();

    partial void OnResultsAreStaleChanged(bool value) => OnPropertyChanged(nameof(HasResult));

    private void RebuildScene()
    {
        FrameAnalysisResult? result = HasResult ? LastOutcome!.Result : null;

        CurrentScene = SceneBuilder.Build(Document, result, DeflectionScale);
    }
}
