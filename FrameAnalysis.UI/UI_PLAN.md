# FrameAnalysis.UI — Development Plan

A precise, phased plan for the WPF UI. Built to fit the existing backend pipeline
and to stay extensible (2D→3D, timber→other codes, members→surfaces).

---

## 0. Guiding principles (the rules that keep this extensible)

These are decisions, not suggestions. Everything below follows from them.

1. **The Document view-model is the single source of truth.** Grids bind to
   observable row VMs, *not* to `StructureInputData`. `StructureInputData` is only
   a transport DTO at the boundary to the solver.
2. **The renderer consumes a presentation `Scene`, never `StructureModel`, never
   solver types, never WPF types.** This is what makes the 2D→3D swap real.
3. **Analysis is code-agnostic; design-code selection lives in the design layer**,
   not on the global Project Setup tab. (Backend today is timber-only: US / EC5 / TR.)
4. **"Live" = geometry only.** The scene re-renders on every edit. The solver and
   the design checks run **on demand** (explicit buttons), on a **background thread**.
5. **Never crash on a bad model.** Catch `StructuralAnalysisException` and bind
   `FrameAnalysisResult.ValidationMessages` to a messages panel. The backend was
   built for this ("the program never crashes — it explains the cause").
6. **IDs are implicit and sequential**, reassigned at export. Users can insert /
   delete / reorder rows freely. Keep the hidden `Z` column; do **no** other 3D work now.
7. **The presentation logic lives in a WPF-free class library, separate from the
   WinExe.** The document model, mapper, and renderer abstraction go in
   `FrameAnalysis.UI.Core` (`net10.0`, no `UseWPF`); only Views, `App`, and the
   concrete `Wpf2DCanvasRenderer` live in `FrameAnalysis.UI` (the WinExe). This makes
   the "no WPF in the model" rule a compile-time guarantee and lets `FEMTestProject`
   test the mapper without dragging in WPF.

---

## 1. Solution wiring (do this first)

Two projects, not one:

- **`FrameAnalysis.UI.Core`** — new class library, target **`net10.0`** (NOT
  `-windows`, NO `UseWPF`). Holds the document model, mapper, and renderer
  abstraction. References:
  - `FrameAnalysisProgram` (needs `StructureInputData` for the mapper)
  - `MemberDesigner` (design input/result types)
  - NuGet **`CommunityToolkit.Mvvm`** (platform-agnostic; `ObservableObject`/
    `[ObservableProperty]` do not pull in WPF)
- **`FrameAnalysis.UI`** — the existing WinExe (`net10.0-windows`, `UseWPF` already
  set). Holds Views, `App`, `Wpf2DCanvasRenderer`. References `FrameAnalysis.UI.Core`
  (and gets `FrameAnalysisProgram`/`MemberDesigner` transitively).

Dependency direction (presentation → domain, one way):

```
FrameAnalysis.UI (WinExe, WPF) → FrameAnalysis.UI.Core (net10.0) → FrameAnalysisProgram
FEMTestProject                 → FrameAnalysis.UI.Core               MemberDesigner
```

- [ ] Create `FrameAnalysis.UI.Core` class library; add the references above.
- [ ] Add the `FrameAnalysis.UI.Core` reference to `FrameAnalysis.UI`.
- [ ] Add both projects to `FrameAnalysisProgram.sln`.
- [ ] Add a `FrameAnalysis.UI.Core` reference to `FEMTestProject` (for mapper tests).
- [ ] Confirm the WinExe builds and runs an empty window.

**Acceptance:** empty app launches from the solution; references resolve; the test
project can `using` the document/mapper types without referencing WPF.

---

## 2. Architecture & data flow

```
                 ┌─────────────────────────────────────────────┐
   user edits →  │  ProjectDocument (VM)  ← single source of    │
                 │  ObservableCollections of row VMs            │
                 └───────────────┬───────────────┬─────────────┘
                                 │               │
                  (on edit)      │               │ (on "Run Analysis")
                  SceneBuilder   │               │ ModelInputMapper
                                 ▼               ▼
                          IStructuralRenderer   StructureInputData (DTO)
                          (Wpf2DCanvasRenderer)        │
                                                StructureModelBuilder
                                                       ▼
                                                 StructureModel
                                                       ▼
                                                  FrameAnalyzer.Analyze
                                                       ▼
                                                 FrameAnalysisResult ──► Results VM
                                                       │                   │
                                              (deflected shape,        (tables,
                                               diagrams) → Scene        messages)
                                                       │
                          (on "Run Design")            ▼
              selected member + demands from ElementEndForceResult
                          → TimberDesignCheckInputFactory → ATimberDesigner
                          → D/C ratio VM (Design tab)
```

**Two mappers, symmetric to the existing `StructureModelBuilder`:**
- `ModelInputMapper.ToInputData(ProjectDocument)` → `StructureInputData` (assigns
  sequential IDs at this point).
- `ModelInputMapper.ToDocument(StructureInputData)` → `ProjectDocument` (for load
  from file). Optional until persistence (Phase 7).

---

## 3. Project / folder / namespace layout

Two projects. The library (`FrameAnalysis.UI.Core`) is WPF-free and testable; the
WinExe (`FrameAnalysis.UI`) holds everything that touches WPF.

### `FrameAnalysis.UI.Core` (class library, `net10.0`, NO WPF)

```
FrameAnalysis.UI.Core/
  Documents/
    ProjectDocument.cs           // root VM, holds all row collections + metadata
    Rows/                        // one observable VM per grid row type
      NodeRowVm.cs
      ElementRowVm.cs
      MaterialRowVm.cs
      SectionRowVm.cs
      SupportRowVm.cs
      NodalLoadRowVm.cs
      MemberLoadRowVm.cs         // distributed / point / temperature (or split)
      SettlementRowVm.cs
  Mapping/
    ModelInputMapper.cs          // Document <-> StructureInputData  (← unit-tested)
  Rendering/
    IStructuralRenderer.cs       // the isolation interface
    Scene/                       // presentation geometry — NO solver/WPF types
      Scene.cs
      SceneNode.cs  SceneMember.cs  SceneSupport.cs
      SceneLoad.cs  SceneDiagram.cs  SceneDeflected.cs
    SceneBuilder.cs              // Document(+Result) -> Scene
    Viewport.cs                  // world<->screen transform, zoom/pan (pure math)
  Services/
    IAnalysisService.cs
    AnalysisService.cs           // async wrapper over FrameAnalyzer + builder
    IDesignService.cs
    DesignService.cs             // wraps ATimberDesigner, feeds demands
  ViewModels/
    MainViewModel.cs             // owns ProjectDocument, services, selection
    ResultsViewModel.cs
    DesignViewModel.cs
    ProjectSetupViewModel.cs
```

> `Viewport` stays pure math (no WPF). If you prefer, the concrete transform can use
> WPF `Matrix`/`Point` — if so, move `Viewport` into the WinExe instead. Keeping it
> as plain `(double x, double y)` math here keeps it testable; pick one.

### `FrameAnalysis.UI` (WinExe, `net10.0-windows`, `UseWPF`)

```
FrameAnalysis.UI/
  App.xaml(.cs)
  MainWindow.xaml(.cs)           // hosts the 40/60 split + TabControl
  Rendering/
    Wpf2DCanvasRenderer.cs       // the ONLY file that touches Canvas
  Views/
    GeometryTab.xaml
    LoadsTab.xaml
    ResultsTab.xaml
    DesignTab.xaml
    ProjectSetupTab.xaml
    DesignInputTemplates.xaml    // DataTemplates per design-input type
```

Rule of thumb for "which project does this go in?": if it would compile without a
reference to `PresentationFramework`/`WindowsBase`, it belongs in `.Core`.

---

## 4. Core contracts (sketches — adapt as you build)

### 4.1 Document & row VMs (source of truth)

```csharp
public partial class ProjectDocument : ObservableObject
{
    // Project Setup metadata
    [ObservableProperty] private string projectName = "Untitled";
    [ObservableProperty] private string siteLocation = "";

    // Geometry / properties
    public ObservableCollection<NodeRowVm>     Nodes     { get; } = new();
    public ObservableCollection<ElementRowVm>  Elements  { get; } = new();
    public ObservableCollection<MaterialRowVm> Materials { get; } = new();
    public ObservableCollection<SectionRowVm>  Sections  { get; } = new();
    public ObservableCollection<SupportRowVm>  Supports  { get; } = new();

    // Loads
    public ObservableCollection<NodalLoadRowVm>  NodalLoads  { get; } = new();
    public ObservableCollection<MemberLoadRowVm> MemberLoads { get; } = new();
    public ObservableCollection<SettlementRowVm> Settlements { get; } = new();
}

// Example row VM — named, observable, with a STABLE id used for selection.
public partial class NodeRowVm : ObservableObject
{
    public int Id { get; set; }                 // display/selection id
    [ObservableProperty] private double x;
    [ObservableProperty] private double y;
    [ObservableProperty] private double z;      // hidden in grid; kept for 3D
}
```

Rule: every row VM raises `PropertyChanged`; subscribing to the collections +
property changes is what drives the **live geometry** re-render.

> **Implemented (Phase 1 data layer, in `FrameAnalysis.UI.Core/Documents/`).** The
> sketch above was refined while building — current shape:
> - Collections are `ObservableRowCollection<T>` (an `ObservableCollection<T>` that
>   *also* raises `Changed` on per-row cell edits, not just add/remove). This is the
>   "listeners" wiring — it solves the fact that plain `ObservableCollection` is silent
>   when an existing row's cell changes.
> - `ProjectDocument` implements `IDocumentChangeNotifier` — one aggregated
>   `event EventHandler? Changed` funnels every collection + cell edit. The renderer /
>   scene pipeline subscribes here, in one place.
> - Identity rows (Node/Material/Section/Element) implement `IIdentifiedRow`; the
>   document renumbers `Id = index+1` on collection changes (display + selection id).
> - Cross-row links are **object references** (`ElementRowVm.StartNode` is a
>   `NodeRowVm?`, loads reference `ElementRowVm?`/`NodeRowVm?`), not raw ints — this is
>   what makes "reorder is safe" actually true. The mapper resolves refs → ids at export.
> - Domain enums reused directly: `MomentRelease`, `LoadDirection`. New UI enum
>   `ElementKind { Frame=0, Truss=1 }`.
> - **Member loads = three typed collections** (`DistributedLoads`, `PointLoads`,
>   `TemperatureLoads`) + `Settlements`, mapping 1:1 to the optional tables — resolving
>   the §8 open item at the data layer (a single "Loads grid" with a Kind column is
>   still an option at the *view* level later).

### 4.2 Mapper (Document → solver DTO; assigns IDs here)

```csharp
public static class ModelInputMapper
{
    public static StructureInputData ToInputData(ProjectDocument doc)
    {
        // Reassign sequential 1..N ids in current grid order, then build the
        // double[,]/int[,] tables exactly as StructureInputData expects.
        // Element columns: [Start,End,Mat,Sec,Type(0=Frame,1=Truss),Release(0-3)]
        // ...
    }
}
```

> **Implemented** in `FrameAnalysis.UI.Core/Mapping/ModelInputMapper.cs`. Notes:
> - Ids come from **collection position** (reference-identity lookups), not the rows'
>   own `Id` fields — a stale id can never corrupt the export, and reorder is safe.
> - Optional load tables are emitted only when non-empty (else `null`, matching the DTO).
> - Null / dangling references throw a clear `InvalidOperationException` naming the row
>   (e.g. *"Element 3 has no start node assigned."*).
> - `ToDocument` (reverse, for file load) is still deferred to Phase 7 (persistence).
> - Verified by `FEMTestProject/ModelInputMapperTests.cs` (4 tests: column mapping +
>   optional-null, end-to-end solve, reorder safety, missing-reference guard). The test
>   project now references `FrameAnalysis.UI.Core`.

### 4.3 Renderer isolation

```csharp
public interface IStructuralRenderer
{
    void Render(Scene scene);          // consumes presentation geometry only
    int? HitTestElement(double screenX, double screenY);  // returns element id or null
}
```

`Scene` holds plain geometry (`SceneNode { Id, X, Y }`, `SceneMember { Id, StartX,
StartY, EndX, EndY, IsTruss }`, support glyphs, load arrows, optional deflected
polyline + diagram polylines). **No** `Node`, **no** `Canvas`, **no** `FrameAnalysisResult`.
`Viewport` owns world↔screen + zoom/pan. `Wpf2DCanvasRenderer` is the only file
allowed to reference `System.Windows.Controls.Canvas`. Swapping to Helix (3D) later
= new `IStructuralRenderer` impl + a 3D `SceneBuilder`; grids/services untouched.

> **Implemented** in `FrameAnalysis.UI.Core/Rendering/` (all WPF-free):
> - `IStructuralRenderer` (`Render(Scene)` + `int? HitTestElement(x,y)`).
> - `Scene` (immutable) + records: `SceneNode/SceneMember/SceneSupport`,
>   `SceneNodalLoad/SceneDistributedLoad/ScenePointLoad`, `SceneDeflectedMember/SceneReaction`,
>   `ScenePoint`, `SceneBounds`. Load directions are resolved to unit vectors (no domain
>   enums leak into the scene). Diagram polylines deferred to Phase 5.
> - `SceneBuilder.Build(doc, result?, deflectionScale)` — flattens document (+ optional
>   result → deflected shape, reactions) to a `Scene`; **skips incomplete rows** instead of
>   throwing, so the picture stays live while editing.
> - `Viewport` — pure world↔screen math with `FitToBounds` / `ZoomAt` / `PanBy` (Y-flip,
>   aspect-preserving, point/empty-model fallback).
> - Verified by `SceneBuilderTests` (3) and `ViewportTests` (4).

### 4.4 Analysis service (async, non-crashing)

```csharp
public sealed record AnalysisOutcome(
    FrameAnalysisResult? Result,
    IReadOnlyList<ValidationMessage> Messages,
    bool Fatal);

public interface IAnalysisService
{
    Task<AnalysisOutcome> RunAsync(ProjectDocument doc, CancellationToken ct = default);
}
// Impl: Task.Run( build DTO -> StructureModelBuilder.Build -> FrameAnalyzer.Analyze ),
// catch StructuralAnalysisException -> Fatal=true + ex.Messages; never throw to UI.
```

> **Implemented** in `FrameAnalysis.UI.Core/Services/` (`IAnalysisService`,
> `AnalysisService`, `AnalysisOutcome`). Notes:
> - **Maps the document to the DTO on the caller's thread**, then `Task.Run`s the
>   build+analyze — the background task never touches the non-thread-safe document.
> - A **fresh `FrameAnalyzer` per run** (the linear solver is stateful), via a
>   `Func<FrameAnalyzer>`; `AnalysisService.CreateDefault()` wires the CSparse solver.
> - Never throws: `StructuralAnalysisException` → `Fatal` + `ex.Messages`; mapping /
>   builder errors → `Fatal` + a single error message; success → result + its
>   `ValidationMessages` (warnings/info), `Fatal=false`.
> - Verified by `AnalysisServiceTests` (3: stable success, unstable→fatal, unassigned-ref→fatal).

### 4.5 Selection (the "hybrid" model — just binding)

`MainViewModel` exposes `[ObservableProperty] int? selectedElementId;`
- Sidebar list `SelectedValue` ⟷ `SelectedElementId` (two-way).
- Canvas click → `HitTestElement` → set `SelectedElementId`.
- Diagrams panel + renderer highlight **read** `SelectedElementId`.
No per-click command needed; one shared property does it.

> **Implemented** — `FrameAnalysis.UI.Core/ViewModels/MainViewModel.cs` (the central
> coordinator, WPF-free). It owns `Document`, keeps `CurrentScene` in sync on every
> `Document.Changed` (live geometry), exposes `SelectedElementId`, and provides
> `RunAnalysisCommand` (async, sets `IsBusy`, stores `LastOutcome`, rebuilds the scene with
> the deflected overlay). Editing after a run sets `ResultsAreStale` and drops the overlay
> while keeping `LastOutcome` for the results panel; `DeflectionScale` re-scales the
> deflected shape live. Verified by `MainViewModelTests` (6). The sidebar/canvas binding
> itself lands with the WinExe.

> **Status — the WPF-free spine is complete and under test (42 tests):** document model →
> mapper → analysis service → scene/viewport → `MainViewModel`. Everything remaining
> (§5 Phase 0/1 shell + grids, `Wpf2DCanvasRenderer`, per-tab VMs that need WPF or the
> design service) is WinExe/WPF work or later phases.

---

## 5. Phased build (each phase ships something runnable)

> **Build status (WinExe now stands up).** The `FrameAnalysis.UI` WinExe is wired and
> the whole solution compiles (7 projects, 0 warnings). Both UI projects are in
> `FrameAnalysisProgram.sln`. `App.OnStartup` seeds a sample portal frame
> (`SampleModels.PortalFrame()` — temporary until file open/Phase 7) and shows the window.
> Run with `dotnet run --project FrameAnalysis.UI` or set it as the VS startup project.
> Phases 0–4 are largely implemented (see notes); not yet visually verified by a human.
>
> **Navigation:** the left pane is a `Menu` (Geometry ▾ / Loads ▾ / Results ▾) driving a
> single active sheet — only one table exists in the visual tree at a time. Nav state is
> `MainViewModel.CurrentSheet` (`Sheet` enum) + `ShowSheetCommand`. The view swaps sheets via
> a `ContentControl` whose `ContentTemplate` is selected from `CurrentSheet` (DataTriggers);
> each sheet's `DataTemplate` lives in `FrameAnalysis.UI/Views/SheetTemplates.xaml` (merged in
> `App.xaml`), keeping the table markup out of `MainWindow`. (Promote any sheet to a full
> UserControl later if it grows its own logic — the host swap is identical.) Load Cases /
> Combinations are menu items opening a "coming soon" placeholder (backend pending).
> **Row add/delete:** grids use an explicit **Add Row** button (`AddRowCommand`, adds to the
> current sheet's collection) rather than the DataGrid new-row placeholder — the placeholder
> binds template-column combos to a sentinel, so new rows never reached the model.
> `CanUserAddRows=False`; delete is still the Delete key (`CanUserDeleteRows`).
> **Zoom:** `Viewport.FitToBounds` now adds a world-space margin (default 15%) so supports /
> load arrows / deflected shape stay visible, and degenerate (flat/vertical) models fit
> properly instead of falling back to 1:1.

### Phase 0 — Shell & split layout ✅ implemented
- 40/60 `Grid` (`GridSplitter`); left = `TabControl`, right = renderer host border.
- `MainViewModel` set as `DataContext` (constructed in `App.OnStartup`, passed to `MainWindow`).
- **Acceptance:** window shows tabs on the left, canvas on the right.
- 40/60 `Grid` (`GridSplitter`); left = `TabControl`, right = renderer host border.
- `MainViewModel` set as `DataContext`.
- **Acceptance:** window shows empty tabs on the left, empty canvas on the right.

### Phase 1 — Document model + Geometry grids ✅ implemented
- **In `.Core`:** `ProjectDocument` + Nodes/Elements/Materials/Sections/Supports row VMs.
- **In WinExe:** Geometry tab (in `MainWindow.xaml`) — a `DataGrid` per collection in
  stacked `GroupBox`es. Headers carry **units** (`X [m]`, `I [m⁴]`, `E [kN/m²]`).
  Element grid uses combo columns for Start/End/Material/Section (object refs) and
  Type/Release (enums). `CanUserAddRows/DeleteRows` on. `Z` column omitted.
- **Acceptance:** can enter the portal frame by hand into the grids.

### Phase 2 — Renderer (static) + live update ✅ implemented
- **In `.Core`:** `Scene`, `SceneBuilder`, `Viewport`, `IStructuralRenderer`.
  **In WinExe:** `Wpf2DCanvasRenderer` (nodes/members/supports/loads + deflected shape +
  reactions), wheel-zoom, right-drag pan, Zoom Extents.
- `MainWindow` subscribes to `MainViewModel.CurrentScene` → `renderer.Render`; the VM
  rebuilds the scene on every `Document.Changed`.
- **Acceptance:** typing geometry updates the picture; zoom/pan works.

### Phase 3 — Loads tabs ✅ implemented (grids); SceneBuilder glyphs done
- Loads tab grids: Joint loads, Distributed, Point, Settlements, Thermal (each its own
  typed collection). Element/Node references via combo columns; direction via enum combo.
- `SceneBuilder` already emits load glyphs (nodal arrows, UDL arrows, point arrows);
  `Wpf2DCanvasRenderer` draws them. (Settlement/thermal have no 2D glyph by design.)
- **Acceptance:** portal-frame UDL + nodal load appear on canvas and map into the DTO.

### Phase 4 — Run Analysis + Results ✅ core implemented
- `IAnalysisService`/`AnalysisService` (async) + `MainViewModel.RunAnalysisCommand`.
- "Run Analysis" button bound to the command; `IsBusy` shows a running indicator.
- Results tab: reactions grid (`LastOutcome.Result.Reactions`), messages list
  (`LastOutcome.Messages`), and a stale-results warning. Fatal problems surface as
  messages, no crash. *(Nodal-displacement table still TODO.)*
- **Acceptance:** sample solves; reactions show; an unstable model shows a clear message.

### Phase 5 — Diagrams + hybrid selection + deflected shape ✅ implemented
- **Post-processing engine** `ANALYSIS_CORE/Recovery/SectionForceRecovery` →
  `FrameAnalysisResult.MemberStations`: per-member N/V/M and deflected shape sampled at
  station points. Section forces by statics on the left free body (recovered member-end
  forces + span loads to the cut); deflected shape by the **Hermite cubic** of the nodal
  displacements + rotations (the lecture-note `y(ξ)` formula — captures bending, not just
  the displaced end nodes). Station layout subdivides each span (default 8/span, cosmetic
  only) and always samples the shear-zero point and point-load steps, so the extrema are
  exact. Closed-form tests in `SectionForceRecoveryTests` (cantilever, SS UDL, point load).
- **Deflected overlay** on the canvas now reads the station curve (`SceneBuilder` samples
  `MemberStations`); `Wpf2DCanvasRenderer.DrawDeflected` unchanged (already a polyline).
- **Per-member popup** (the chosen UX): double-click a member in the Summary → Members
  list (`MouseBinding` → `MainViewModel.OpenMemberResultCommand`, which raises
  `MemberResultRequested`; `MainWindow` opens `Views/MemberResultWindow`). Shows the
  deflected shape on top, then N / V / M stacked.
- **Design demands** (single-state run) now read the per-member **station envelope** (exact
  worst section force along the span, e.g. mid-span UDL moment), via `DesignInputMapper`.
- **Acceptance:** clicking a member on canvas selects it in the list and highlights it;
  double-clicking shows its deflected shape + N/V/M; the deflected overlay is curved.
- **Deferred follow-ups:** the per-combination ULS design envelope (`RunEnvelopeAsync`)
  still superposes member-*end* forces only — extending the `SuperpositionBasis` to carry
  station data would let combos use the span envelope too. Station count is hard-coded
  (8/span); a "user-defined # points" toolbar control is the remaining board item.

### Phase 6 — Member Design tab ✅ implemented (timber: US-solid + EC5 + TR)
Implemented via the existing **menu + `Sheet`-enum** navigation (not a separate
`DesignTab.xaml`), with a `_Design` menu → Project Details / Materials / Member Design /
Design Results sheets and a **Run Design** toolbar button (enabled only when a current
analysis result exists). Locked decisions made during build:
- **Grade-driven material database** `MemberDesigner/TimberMaterialData/TimberMaterialDatabase`
  (EN 338 / EN 14080 for EC5/TR, NDS species+grade for US). Picking a grade on a material
  auto-fills the design values and syncs `ElasticModulus`; a manual-override toggle allows
  hand entry. All values in MPa/mm/N (the design backend's units).
- **Codes:** US (solid members only — `eMemberConfigurationType.SolidMembers`), EC5, TR.
  `PrepareCheckInputUS` is implemented; the provider/factory code is no longer hardcoded.
- **Single project-level design code** in Project Details (no per-member override — material
  grade values are code-family-specific, so one code per project keeps them coherent).
- **Demands auto-fed** from `FrameAnalysisResult.ElementEndForces` via `DesignInputMapper`
  (minor-axis moment = 0 in 2D). `DesignService` wraps `ATimberDesigner.CheckDesignAsync`,
  per member, off the UI thread; readiness problems surface as messages, never exceptions.
- D/C ratios shown per member/check, colored by `eDesignStatus`.
- Deferred: dynamic per-check input panels, glulam volume-factor fields, multi-select.

### Phase 7 — Project Setup + persistence
- Project Details (the `DesignSettings` sheet) now holds the design code + code-aware
  environment (service class for EC5/TR, moisture for US) + load duration + factor overrides.
- Save/Load: serialize `ProjectDocument` to JSON (`System.Text.Json`) — **still TODO**.
- **Acceptance:** save a project, reopen it, geometry/loads/design-inputs restored.

### Phase 8 — Deferred (placeholders only, do not build now)
- Surface elements grid (4-node panels) — leave a collection slot in `ProjectDocument`.
- 3D renderer swap (Helix) — enabled by §4.3; **note: solver is still 2D**.
- Units system; concrete/ACI design; timber shear walls (`eShearWallAnalysisType`).

---

## 6. Wiring notes for the existing backend seams (precise)

These are the exact integration points; get them right and the rest is UI work.

1. **Demand flow into design.** Analysis returns
   `FrameAnalysisResult.ElementEndForces` →
   `ElementEndForceResult.LocalEndForces` ordered `[Fx1,Fy1,Mz1,Fx2,Fy2,Mz2]`
   (frame) or `[Fx1,Fy1,Fx2,Fy2]` (truss). Map these to the design input
   `Max*Demand` fields:
   - axial demand ≈ `Fx` (sign per convention),
   - shear demand ≈ max(|Fy1|,|Fy2|),
   - moment demand ≈ max(|Mz1|,|Mz2|).
   This replaces the static placeholders in
   `TimberDesignCheckInputFactory` (see the `// TO DO ... should be called from UI!`
   comments).

2. **Design-code source.** `TimberDesignCheckInputFactory` hardcodes
   `_TimberCode = eTimberCode.EC5`. Drive it from the UI's per-member / default
   design-code selector instead of the constructor literal.

3. **Stop silently swallowing design errors.** `TimberDesignCheckInputFactory
   .PrepareAllCheckInputs()` has a bare `catch { }`. Surface failures to the Design
   tab (a message row), don't drop them.

4. **Truss vs frame.** Element grid `Type` column = `ElementTable[...,4]`
   (0=Frame, 1=Truss); member loads are frame-only (the builder already rejects
   truss member loads). Diagrams differ (truss = axial only, 4-component result).

5. **IDs.** Do not expose raw table indices. The mapper assigns `1..N` from grid
   order at export, so reorder/insert/delete in grids is safe.

---

## 7. Locked decisions (quick reference)

| Topic            | Decision                                                              |
|------------------|----------------------------------------------------------------------|
| Project split    | `FrameAnalysis.UI.Core` (net10.0, no WPF: document/mapper/scene/services/VMs) + `FrameAnalysis.UI` (WinExe: Views/App/`Wpf2DCanvasRenderer`). |
| Source of truth  | `ProjectDocument` VM; `StructureInputData` is a boundary DTO only.    |
| Renderer input   | `Scene` (plain geometry). No solver/WPF types in the interface.       |
| Live update      | Geometry re-renders on edit. Analysis/design = on-demand, async.      |
| Threading        | `FrameAnalyzer`/`ATimberDesigner` run via `Task.Run`; UI stays free.  |
| Errors           | Catch `StructuralAnalysisException`; bind `ValidationMessages`.       |
| Design code      | Lives in design layer (project-level, in Design → Project Details), not global setup. US solid-only + EC5 + TR. |
| IDs              | Implicit, sequential, assigned at export.                            |
| 3D               | Keep `Z` column only. Renderer interface enables UI swap; solver stays 2D. |
| MVVM             | CommunityToolkit.Mvvm.                                                |
| Persistence      | JSON of the document (System.Text.Json).                             |

---

## 8. Open items to decide as you go
- ~~Member-load grid: one grid with a `Kind` column vs. three separate grids.~~
  Data layer decided: three typed collections. View can still present one grid later.
- Whether design demands take the worst end or full diagram envelope.
- Units: header labels now; a real unit system is a later, separate effort.

