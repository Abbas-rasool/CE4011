# TimberFrame2D — User Manual

A 2D structural analysis and timber-design program: build a frame model, run a linear-static
analysis, view the deformed shape and internal-force diagrams, and run code-based timber
member-design checks (Eurocode 5, the Turkish timber code, and the US/NDS method).

This manual covers the input format, the model-creation workflow, how to run the analysis,
and how to read the results. A complete worked example (a timber portal frame) is given at
the end.

> For installation and prerequisites, see the separate **Installation Manual**.

---

## 1. The application window

The window is split into two panes:

```
┌───────────────────────────────┬──────────────────────────────────────────┐
│  MENU BAR                      │  TOOLBAR                                   │
│  Geometry ▾ Loads ▾ Analysis ▾ │  [Run Analysis] [Run Design]               │
│  Units  Design ▾               │  [Zoom Extents]  Deflection × [ 1 ]        │
│                                │                                            │
│  [Add Row]  (Delete to remove) │                                            │
│  ┌──────────────────────────┐  │            (model viewer / canvas)         │
│  │  active input sheet       │  │                                            │
│  │  (a data grid)            │  │     nodes · members · supports · loads     │
│  │                           │  │     deflected shape · reactions            │
│  └──────────────────────────┘  │                                            │
└───────────────────────────────┴──────────────────────────────────────────┘
```

- **Left pane — input.** A menu chooses which *sheet* is shown; each sheet is a data grid for
  one part of the model (nodes, elements, loads, …). Only one sheet is visible at a time.
- **Right pane — viewer.** A live 2D picture of the model. It redraws automatically as you
  type. After an analysis it also shows the deflected shape and support reactions.

**Canvas controls**

| Action | Control |
|---|---|
| Zoom | Mouse wheel |
| Pan | Right-mouse drag |
| Select a member | Left-click it (highlights orange) |
| Fit model to view | **Zoom Extents** button |

> 📷 *Screenshot suggestion: the full window with the sample portal frame loaded.*

---

## 2. Units and sign conventions

Units are shown in each grid's column headers. The defaults are:

| Quantity | Unit |
|---|---|
| Node coordinates X, Y | m |
| Section width, depth | mm (see the Sections header) |
| Moment of inertia I | mm⁴ |
| Material modulus E, strengths | MPa |
| Density ρk | kg/m³ |
| Forces (Fx, Fy, reactions) | kN |
| Moments (Mz) | kN·m |
| Distributed load intensity | kN/m |
| Effective lengths (design) | m |

The **Units** menu opens a sheet where the display units can be changed.

**Sign convention:** global **+X is to the right, +Y is up**. A downward force is therefore a
**negative** Fy; a gravity (downward) distributed load is a **negative** intensity with
direction **Y**. A counter-clockwise moment Mz is positive.

---

## 3. Building a model

A sample portal frame is loaded automatically on launch, so you always have something to
explore. To build your own, work top-to-bottom through the **Geometry** and **Loads** menus.

On every sheet, use the **Add Row** button to append a row, edit the cells in place, and press
**Delete** on a selected row to remove it. Rows that reference other rows (an element's start
node, a load's element, …) use drop-down pickers, so you never type raw ID numbers — reordering
or deleting rows stays safe.

### 3.1 Geometry ▸ Nodes
One row per node. Enter **X** and **Y** (m). IDs are assigned automatically.

### 3.2 Geometry ▸ Materials
Strengths are in MPa. Choose a **strength class** (e.g. C24 for EC5/TR) or a **species/grade**
(US); the design values and the elastic modulus **auto-fill** from the built-in database. Tick
**Override** to type values by hand.

### 3.3 Geometry ▸ Sections
Enter **Width**, **Depth**, and **I** (moment of inertia about the in-plane bending axis).
**Area** is computed for you. For a rectangle, `I = width · depth³ / 12`.

### 3.4 Geometry ▸ Elements
One row per member. Pick **Start** node, **End** node, **Material**, and **Section** from the
drop-downs. **Type** is `Frame` (bending + axial) or `Truss` (axial only). **Release** adds an
end moment hinge (`None`, `Start`, `End`, or `Both`).

### 3.5 Geometry ▸ Supports
Pick the **Node**, then tick the restrained DOFs: **Ux**, **Uy**, **Rz**. The renderer picks the
glyph automatically — pin (Ux+Uy), roller (one translation), or fixed (Rz too).

### 3.6 Loads
Each load type has its own sheet under the **Loads** menu:

- **Joint Loads** — nodal Fx, Fy (kN), Mz (kN·m) at a node.
- **Distributed Loads** — a uniform load on an element: intensity (kN/m) and **direction**
  (X or Y). Downward gravity = negative intensity, direction Y.
- **Point Loads** — a concentrated load at a distance along an element.
- **Settlements** — a prescribed support displacement.
- **Thermal Loads** — a temperature change on an element.

Each load carries a **Nature** (Dead, Live, Wind, …) used later by the load-combination
generator.

---

## 4. Running the analysis

1. Click **Run Analysis** (viewer toolbar). The solve runs on a background thread; a "Running…"
   indicator shows while it works.
2. The viewer adds the **deflected shape** (dashed purple curve) and the **support reactions**
   (green arrows / moment arcs).
3. Open **Analysis ▸ Summary** to see the result tables.

The program never crashes on a bad model: if the structure is unstable or under-restrained, the
**Messages** list on the Summary sheet explains the cause instead.

> If you **edit the model after solving**, the results become *stale* — a warning banner appears
> and the deflected overlay is dropped. Click **Run Analysis** again to refresh.

---

## 5. Reading the results

### 5.1 Summary sheet
**Analysis ▸ Summary** shows:

- **Members** — the element list. (Double-click a member here → see §5.3.)
- **Reactions** — Node, Fx [kN], Fy [kN], Mz [kN·m] at each support.
- **Messages** — warnings/info (e.g. static indeterminacy notes).

### 5.2 Deflected shape on the canvas
After a run, the canvas overlays the deflected shape. Because displacements are tiny, use the
**Deflection ×** box in the toolbar to exaggerate them (e.g. 50, 100, 500). The curve is the
true bending shape (Hermite cubic interpolation of the nodal displacements **and rotations**),
not just straight lines between displaced joints.

### 5.3 Internal-force diagrams (per member)
On the Summary sheet, **double-click a member** in the Members list. A popup opens with four
stacked plots for that member:

1. **Deflected shape**
2. **Normal force N** (kN) — tension positive
3. **Shear V** (kN)
4. **Moment M** (kN·m) — sagging positive

Each plot draws the member axis as a horizontal baseline with the value plotted (and filled) to
one side, auto-scaled so the shape is always visible. Under a distributed load the moment is the
expected parabola; a point load shows the shear step and the moment kink.

> 📷 *Screenshot suggestion: the member-results popup for the rafter of the worked example.*

---

## 6. Timber design checks (optional)

The **Design** menu runs code-based member-design checks against the analysis demands.

1. **Design ▸ Project Details** — choose the design **code** (EC5, TR, or US) and the code-aware
   settings (service class / moisture condition, load-duration class, factor overrides).
2. **Design ▸ Materials** — assign each material a strength class/grade so the design values are
   defined.
3. **Design ▸ Member Design** — per member: effective lengths (major/minor/beam, in m), lateral
   support, etc.
4. Click **Run Design** (enabled once an analysis result exists; demands are taken from it).
5. **Design ▸ Design Results** — the demand/capacity (D/C) ratio for each member and check,
   coloured by status.

Design demands are taken from the **station envelope** of each member (the worst section force
anywhere along the span — e.g. the mid-span moment under a UDL, not just the end values).

---

## 7. Complete worked example — timber portal frame

This is the model loaded on launch (**Portal Frame (sample)**), so you can follow along
immediately, or rebuild it from scratch with the steps below.

### 7.1 The structure
A 4 m × 3 m fixed-base timber portal: two columns and a rafter (beam), a lateral load at the
left eaves and a gravity UDL on the rafter.

```
        w = 8 kN/m (down)
      ↓ ↓ ↓ ↓ ↓ ↓ ↓ ↓
 N2 *━━━━━━━━━━━━━━━* N3        Nodes (m):  N1(0,0)  N2(0,3)  N3(4,3)  N4(4,0)
 →  ┃ (E2 rafter)   ┃           Elements:   E1: N1→N2 (col)
 8kN┃               ┃                       E2: N2→N3 (rafter)
    ┃E1           E3┃                       E3: N4→N3 (col)
    ┃               ┃           Supports:   N1, N4 fixed (Ux,Uy,Rz)
   ▟▙ N1          ▟▙ N4
  (fixed)        (fixed)
```

### 7.2 Input
| Item | Value |
|---|---|
| Nodes | N1 (0, 0), N2 (0, 3), N3 (4, 3), N4 (4, 0) [m] |
| Material | Timber **C24** (E ≈ 11 000 MPa, auto-filled from the class) |
| Section | 150 × 300 mm, `I = 150·300³/12 = 3.375×10⁸ mm⁴` |
| Elements | E1 N1→N2, E2 N2→N3, E3 N4→N3 — all *Frame*, release *None* |
| Supports | N1 and N4: Ux, Uy, Rz all restrained (fixed) |
| Joint load | N2: **Fx = +8 kN** (lateral, →) |
| Distributed load | E2: **−8 kN/m**, direction **Y** (gravity, down) |

To build it by hand: add the four nodes, one C24 material, the 150×300 section, the three
elements, two fixed supports, the joint load, and the distributed load — in that order.

### 7.3 Run and read
1. **Run Analysis.** The frame sways slightly to the right (lateral load) and the rafter sags.
   Set **Deflection ×** to ~100 to see it clearly.
2. **Analysis ▸ Summary** → the **Reactions** table lists the base reactions at N1 and N4.
3. **Double-click E2** (the rafter) in the Members list → the popup shows its deflected shape,
   the near-constant axial, the linear-then-stepped shear, and the **parabolic moment** from the
   8 kN/m UDL.

### 7.4 Sanity checks (hand verification)
These equilibrium checks should hold for the reactions in the Summary table:

- **Vertical:** the only vertical load is the rafter UDL, `8 kN/m × 4 m = 32 kN` down.
  → the two vertical base reactions must sum to **+32 kN** (up).
- **Horizontal:** the only horizontal load is the 8 kN joint load (→).
  → the two horizontal base reactions must sum to **−8 kN** (i.e. 8 kN total pushing back, ←).
- **Moment:** the sum of base moments and the moments of all reactions/loads about any point
  must be zero.

The program also runs an internal equilibrium-residual check and reports a warning if
`‖K·U − F‖` is large, so a clean run with these totals confirms the solution.

### 7.5 Design (optional)
Open **Design ▸ Project Details**, choose **EC5**, confirm the material is C24, then **Run
Design**. **Design ▸ Design Results** lists each member's D/C ratio; the sample is sized to pass
with a sensible margin.

---

## 8. Troubleshooting

| Symptom | Cause / fix |
|---|---|
| Double-clicking a member does nothing | Run an analysis first; the popup needs a current result. |
| "Results are out of date" banner | You edited the model after solving — click **Run Analysis** again. |
| Deflected shape looks flat | Increase **Deflection ×**. |
| A red error in **Messages** | The model is unstable / under-restrained; the message names the node or DOF at fault. |
| **Run Design** is disabled | Run an analysis first (design demands come from it). |
| Nothing on the canvas | Use **Zoom Extents**, or check that elements have both end nodes assigned. |
