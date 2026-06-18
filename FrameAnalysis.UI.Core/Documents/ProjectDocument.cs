using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FrameAnalysis.UI.Core.Documents.Rows;

namespace FrameAnalysis.UI.Core.Documents;

/// <summary>
/// The editable, in-memory project — the single source of truth the UI binds to.
///
/// Holds observable row collections and surfaces every change (a collection edit OR an
/// individual cell edit) through one <see cref="Changed"/> event, so the scene/renderer
/// pipeline and dirty-tracking subscribe in exactly one place. This is a transport-free
/// representation; ModelInputMapper converts it to StructureInputData at the solver boundary.
///
/// Cross-row links (element → nodes/material/section, loads → element/node) are held by
/// object reference, so reordering/inserting/deleting rows never breaks references. The
/// 1-based ids on identity-bearing rows are kept in sync with grid order here and only
/// matter at export time.
/// </summary>
public sealed partial class ProjectDocument : ObservableObject, IDocumentChangeNotifier
{
    /// <inheritdoc />
    public event EventHandler? Changed;

    // --- Project metadata ---
    [ObservableProperty] private string projectName = "Untitled";
    [ObservableProperty] private string siteLocation = string.Empty;

    // --- Geometry & properties ---
    public ObservableRowCollection<NodeRowVm> Nodes { get; } = new();
    public ObservableRowCollection<MaterialRowVm> Materials { get; } = new();
    public ObservableRowCollection<SectionRowVm> Sections { get; } = new();
    public ObservableRowCollection<ElementRowVm> Elements { get; } = new();
    public ObservableRowCollection<SupportRowVm> Supports { get; } = new();

    // --- Loads ---
    public ObservableRowCollection<NodalLoadRowVm> NodalLoads { get; } = new();
    public ObservableRowCollection<DistributedLoadRowVm> DistributedLoads { get; } = new();
    public ObservableRowCollection<PointLoadRowVm> PointLoads { get; } = new();
    public ObservableRowCollection<TemperatureLoadRowVm> TemperatureLoads { get; } = new();
    public ObservableRowCollection<SettlementRowVm> Settlements { get; } = new();

    // --- Design ---
    /// <summary>Project-wide design settings (code, environment, factors).</summary>
    public DesignSettings Design { get; } = new();

    /// <summary>Per-member design parameters, kept aligned with <see cref="Elements"/>.</summary>
    public ObservableRowCollection<MemberDesignRowVm> MemberDesigns { get; } = new();

    public ProjectDocument()
    {
        // Keep 1-based ids aligned with grid order for the identity-bearing tables.
        Nodes.CollectionChanged += (_, _) => Renumber(Nodes);
        Materials.CollectionChanged += (_, _) => Renumber(Materials);
        Sections.CollectionChanged += (_, _) => Renumber(Sections);
        Elements.CollectionChanged += (_, _) => Renumber(Elements);
        Elements.CollectionChanged += (_, _) => SyncMemberDesigns();

        // Funnel every collection's change signal into the single document event.
        Nodes.Changed += OnChildChanged;
        Materials.Changed += OnChildChanged;
        Sections.Changed += OnChildChanged;
        Elements.Changed += OnChildChanged;
        Supports.Changed += OnChildChanged;
        NodalLoads.Changed += OnChildChanged;
        DistributedLoads.Changed += OnChildChanged;
        PointLoads.Changed += OnChildChanged;
        TemperatureLoads.Changed += OnChildChanged;
        Settlements.Changed += OnChildChanged;
        MemberDesigns.Changed += OnChildChanged;

        // Metadata edits count as document changes too.
        PropertyChanged += OnMetadataChanged;
        Design.PropertyChanged += OnMetadataChanged;
    }

    /// <summary>
    /// Keeps <see cref="MemberDesigns"/> in step with <see cref="Elements"/>: one design row
    /// per element, added for new elements and dropped for removed ones. Existing rows are
    /// left untouched so the user's per-member inputs survive reordering.
    /// </summary>
    private void SyncMemberDesigns()
    {
        // Drop rows whose element is gone.
        for (int i = MemberDesigns.Count - 1; i >= 0; i--)
        {
            if (MemberDesigns[i].Element is null || !Elements.Contains(MemberDesigns[i].Element!))
                MemberDesigns.RemoveAt(i);
        }

        // Add a row for any element that doesn't have one yet.
        foreach (ElementRowVm element in Elements)
        {
            bool exists = false;
            foreach (MemberDesignRowVm row in MemberDesigns)
            {
                if (ReferenceEquals(row.Element, element)) { exists = true; break; }
            }
            if (!exists)
                MemberDesigns.Add(new MemberDesignRowVm(element));
        }
    }

    private void OnChildChanged(object? sender, EventArgs e) => RaiseChanged();

    private void OnMetadataChanged(object? sender, PropertyChangedEventArgs e) => RaiseChanged();

    private void RaiseChanged() => Changed?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Reassigns ids 1..N in current order. Setting an id raises the row's
    /// PropertyChanged → the collection's Changed → the document Changed, but never
    /// re-enters CollectionChanged, so there is no renumbering loop.
    /// </summary>
    private static void Renumber<T>(ObservableRowCollection<T> rows)
        where T : IIdentifiedRow, INotifyPropertyChanged
    {
        for (int i = 0; i < rows.Count; i++)
        {
            int expected = i + 1;
            if (rows[i].Id != expected)
                rows[i].Id = expected;
        }
    }
}
