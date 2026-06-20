# TimberFrame2D (CE 4011)

A from-scratch **2D structural analysis and timber design** desktop application, written in
C# (**.NET 10, WPF**). It analyzes plane **frames and trusses** by the linear-elastic
**direct stiffness (matrix displacement) method** and performs **timber member design** to
three codes (**US / NDS**, **Eurocode 5**, and **Turkish Timber Code**), with a live 2D model view,
internal-force diagrams, and code-based load combinations. The analysis engine, loading
layer, design engine, and matrix library are all hand-built (only the sparse Cholesky
factorization uses CSparse).

## Capabilities

### Analysis
- Linear-elastic **2D frame & truss** analysis via the direct stiffness method.
- **Frame** elements (3 DOF/node: u, v, θ) and **truss** elements (2 DOF/node), with
  moment releases / hinges.
- Loads: **nodal** forces & moments, member **uniform-distributed**, member **point**,
  **thermal** (uniform + gradient), and support **settlements**.
- Custom sparse-matrix library with a **CSparse-backed sparse Cholesky** solve, plus a
  **singularity/instability detector** that pinpoints the offending node and DOF.
- Never crashes on a bad model — invalid input is surfaced as messages, not exceptions.

### Post-processing & visualization
- Support **reactions** and nodal **displacements**.
- Per-member **axial (N), shear (V), and moment (M)** diagrams recovered by statics, with
  exact extrema (shear-zero points and point-load steps sampled explicitly).
- **Deflected shape** from the Hermite cubic of the nodal displacements and rotations.
- Live 2D canvas (zoom / pan / hit-test / member selection) and a per-member result window.

### Timber design
- Three codes: **US** (NDS, ASD & LRFD), **Eurocode 5** (EN 338 / EN 14080), **Turkish TBDY**.
- Checks: tension, compression, bending, shear, and combined bending + axial. **D/C
  utilization ratios** per member, color-coded Pass / Warning / Fail.
- **Grade-driven material database** (EN 338 strength classes, NDS species/grade) that
  auto-fills design values.
- Code-standard **load combinations** (ASCE 7, EN 1990, TBDY) with load-duration
  adjustment (k_mod / C_D / λ) and a per-combination **design envelope**.

## Architecture

A layered solution of 7 projects with a strictly one-directional dependency
(presentation → domain):

| Project | Role |
|---|---|
| `FrameAnalysisProgram` | FEM solver: model, DOF numbering, assembly, result recovery |
| `MemberDesigner` | Timber design engine (US / EC5 / TR) |
| `StructuralLoads` | Material-agnostic load natures & code combination generators |
| `Matrix_Library` | Sparse matrix types and linear solvers (custom LDLᵀ + CSparse Cholesky) |
| `FrameAnalysis.UI.Core` | WPF-free presentation core: document model, mappers, scene, services, view-models |
| `FrameAnalysis.UI` | WPF desktop app: views, 2D renderer, app shell |
| `FEMTestProject` | Unit & verification tests |

Key design choices:
- The observable **`ProjectDocument` is the single source of truth**; `StructureInputData`
  is only a boundary DTO handed to the solver.
- The renderer consumes a plain **`Scene` through `IStructuralRenderer`** — no WPF or solver
  types leak in — which keeps the door open to a future 3D renderer.
- Analysis and design run **on demand on a background thread**; geometry re-renders **live**
  on every edit.
- Polymorphic elements, interface-based loads, a strategy-swappable solver, and a
  dependency-ordered design-check graph keep the engine extensible.

## Verification & testing

**88 unit tests** (`FEMTestProject`), including closed-form **verification** of the
section-force / deflection recovery against textbook cases (cantilever, simply-supported
beam under UDL, point load), plus a post-solve **equilibrium residual check** inside the
analyzer.

## Building & running

Requires the **.NET 10 SDK**.

```bash
# build the whole solution
dotnet build FrameAnalysisProgram.sln -c Release

# run the app
dotnet run --project FrameAnalysis.UI

# run the tests
dotnet test FEMTestProject

# self-contained Windows build (bundles the .NET runtime; runs without an install)
dotnet publish FrameAnalysis.UI -c Release -r win-x64 --self-contained
```

## Technologies

- **Language & runtime:** C# 14, .NET 10
- **UI:** WPF, CommunityToolkit.Mvvm
- **Solver:** CSparse.NET (sparse Cholesky)

## Limitations & roadmap

- **2D only**; linear-elastic, small-displacement (no P-Δ, buckling, or dynamic/modal analysis).
- **Timber-only** design (US limited to solid members); steel/concrete modules are a planned extension.
- **Project save/load** (JSON) and **calculation-report export** are designed for but not yet implemented.
- Future: **3D rendering** via the swappable renderer, and **CAD/BIM (e.g. Revit) import**.
