# OCCT Bridge API Coverage

OcctCSharpBridge 2.6 is a Windows x64 bridge for Open CASCADE Technology 7.9.0. The reusable SDK exposes two intentional managed façades and three optional UI hosts:

- `OcctEngine`: interactive AIS/viewer/object operations.
- `OcctModelingSession`: headless geometry, topology, algorithms, meshing, analysis, and exchange.
- `OcctNet.WinForms`, `OcctNet.Wpf`, `OcctNet.Avalonia`: reusable viewport adapters only.

OCAF/XDE is intentionally excluded. Application Document, Feature/Entity, Command, Tool, Undo/Redo, Snap/Grip, and persistence belong to `demo` or another CAD application layer. See [Architecture Boundaries](ARCHITECTURE_BOUNDARIES.md).

- Native bridge version: `2.6.0`
- Native ABI: `3`
- OCCT: `7.9.0`
- Native exports: `354`
- Managed P/Invoke declarations: `354`
- Public .NET types: `100`
- Compatibility .NET types: `1`
- Viewer API: `214`
- Modeling API: `140`

`Public .NET types` is the primary owner-aware 2.6 surface. `Compatibility .NET types` currently contains only the Bridge 2.5 `OcctObject` compatibility handle. Compatibility surface is tracked separately and is not expanded in 2.x.

## API rules

| Area | Rule | Example |
|---|---|---|
| Shape query | `GetShape...` / `IsShape...` / `SetShape...` | `GetShapeBounds()` |
| Edge query | `GetEdge...` / `EvaluateEdge...` | `GetEdgeCurveType()` |
| Face query | `GetFace...` / `EvaluateFace...` | `GetFaceUvBounds()` |
| Batch analysis | `Analyze...` | `AnalyzeFaces()`, `AnalyzeEdgeAdjacency()` |
| Structured inspection | `Inspect...` | `InspectShape()` |
| Indexed topology | `...At` suffix | `GetSubshapeAt()` |
| Construction | `Make...` | `MakePlanarFace()` |
| Algorithms | operation verb | `Extrude()`, `OffsetWire()` |
| Mesh | triangulation vocabulary | `Triangulate()`, `GetShapeMeshData()` |
| Native C ABI | exact `occt_...` symbol | `occt_model_shape_face_analysis` |

Public object handles are Engine/Session owned. Raw `long` values cannot construct primary `OcctShape`/`OcctModelShape` handles. Native 0/1 flags are represented by managed `bool` or enums.

## Assembly responsibilities

| Assembly | Responsibility |
|---|---|
| `OcctNet` | Core types, `OcctEngine`, headless `OcctModelingSession`, runtime loading/diagnostics, host-neutral viewport interaction policy |
| `OcctNet.WinForms` | Reusable WinForms HWND viewport host |
| `OcctNet.Wpf` | Reusable WPF viewport host using `WindowsFormsHost` |
| `OcctNet.Avalonia` | Reusable Avalonia `NativeControlHost` backed by a Windows child HWND |

`OcctNet.Avalonia` is Windows-only today; it does not claim Linux/macOS viewer support. `OcctNet` itself does not reference WinForms, WPF, or Avalonia.

The complete WinForms/WPF/Avalonia CAD applications and `CadCommon` remain on `demo` only.

## Interactive `OcctEngine`

`OcctEngine` owns displayed AIS objects and the native viewer context. Coverage includes:

- camera/view/projection control and screen/world conversion;
- registered object lifecycle, visibility, appearance, material, depth/display state, and transforms;
- object/subshape selection, rectangle selection, hover detection, and structured selection hits;
- interactive primitive/feature creation and annotations;
- STEP/IGES/BREP/STL exchange for viewer-managed shapes.

### Structured selection

- `GetSelectedHits()` returns registered AIS selections as `OcctSelectionHit` values.
- `TryGetDetectedHit()` returns the detected/hovered registered entity when available.
- `OcctSelectionHit` exposes `Owner`, `SubshapeType`, and runtime `SubshapeIndex`.
- Selected-hit retrieval uses a two-call batch ABI rather than N+1 P/Invoke access.
- Runtime subshape indices follow `TopExp_Explorer` order and are **not persistent naming**.

See [Structured Viewer Selection Hits](SELECTION_HITS.md).

## Headless `OcctModelingSession`

### Construction and algorithms

Construction covers vertex, line, polyline, circle/arc, polygon, ellipse, Bezier, interpolated B-Spline, rectangle/planar Face, Wire, Compound, sewn Shell/Solid, Box/Cylinder/Cone/Sphere/Torus/Wedge, including planar Faces with holes.

Algorithms cover Fuse/Cut/Common/Section/Splitter, Extrude/Revolve/Sweep/Loft, Fillet/Chamfer, 3D offset, planar wire offset, thick solid, same-domain unification, healing, and operation history.

Ray-hit retrieval and generated/modified topology history use bulk-copy Native ABI calls. Legacy indexed `...At` exports remain available for ABI compatibility but are not used by the managed collection APIs.

### Topology and shape queries

- shape type/orientation/closure/validity/check report/hash/tolerance;
- AABB and oriented bounds;
- linear/surface/volume mass properties, inertia tensor/principal properties, and shape distance;
- location read/write;
- generic subshape traversal and common convenience collections;
- edge/face/wire ancestry and adjacency;
- batched `AnalyzeEdgeAdjacency()` and strict `AnalyzeFreeBounds()`;
- `IsSameShape()` / `IsPartnerShape()` OCCT identity semantics.

See [Topology Adjacency and Free-Boundary Analysis](TOPOLOGY_ANALYSIS.md).

### Geometry and differential geometry

- vertex and edge evaluation;
- curve/surface type and analytic geometry parameters;
- edge parameter range, derivatives, tangent/normal, curvature, and curvature center;
- Face UV range, periodicity, derivatives, normal, principal/mean/Gaussian curvature;
- point projection to Edge/Face, ray intersection, solid point classification, and exact `TrimEdge()`;
- B-Spline curve/surface degree, poles, weights, knots, multiplicities, and control grids.

See [B-Spline Curve and Surface Inspection](BSPLINE_CURVES.md).

### Batched inspection

`AnalyzeFaces()` returns per-Face type/orientation/area/tolerance/UV/AABB/topology metadata in one native traversal. `InspectShape()` composes validity, closure, tolerance, check report, bounds, topology counts, edge adjacency, face analysis, optional free bounds, and optional mesh statistics without embedding application pass/fail rules.

See [Batch Face Analysis and Shape Inspection](SHAPE_INSPECTION.md).

### Triangulation and provenance

`Triangulate()`, `GetFaceMesh()`, `GetShapeMesh()`, `GetShapeMeshData()`, and `ClearTriangulation()` cover meshing. `OcctShapeMeshData` preserves source-Face ranges so combined mesh node/triangle indices can be mapped back to CAD topology without a per-triangle FaceId array.

See [Shape Mesh Face Provenance](MESH_PROVENANCE.md).

### Exchange

STEP, IGES, BREP, and STL import/export are exposed directly, plus generic file import. STL export accepts explicit tessellation parameters.

## Native internal organization

Internal source organization is not ABI:

- session/registry, shape queries, topology, geometry queries, and viewer interop are separate modules;
- geometry construction is split into Curves, Planar, Primitives, Assembly, and Transform modules;
- Boolean, feature, healing, operation-history, projection/ray/classification, Mesh, and file Exchange responsibilities are separate modules;
- broad `OcctModelingInternal.hxx` has been retired; modules include the narrow internal header and direct OCCT headers they actually use.

These changes preserve existing ABI 3 signatures while extending the additive C surface to 354 exported symbols.

## UI host interaction boundary

WinForms and Avalonia share only framework-neutral interaction decisions such as hover/world-point throttling, rectangle-selection threshold/direction, drag-end recovery, and default zoom factors. Window creation, DPI, mouse capture, WPF hosting, and Win32 subclassing remain host-specific. This removes meaningful duplication without introducing a universal UI framework abstraction.

## Runtime and ownership

- `OcctEngine` and `OcctModelingSession` use `SafeHandle` internally.
- Managed objects/shapes carry an owner token and cross-engine/session use is rejected before native invocation.
- `OcctRuntime.GetDiagnosticInfo()` and `GetDiagnosticReport()` are side-effect-free diagnostic entry points.
- Native failures are converted to `OcctException` with operation/native-message metadata.

See [Structured Runtime Diagnostics](RUNTIME_DIAGNOSTICS.md).

## Validation

Cloud CI does not have the project's OCCT SDK, so it does not claim native execution. It validates:

- Native declarations/definitions/PInvoke symbol parity and Cdecl/exact spelling;
- API counts from `bridge-contract.json`, including separate primary and compatibility managed-type counts;
- public managed API signature snapshot across Core, WinForms, WPF, and Avalonia hosts;
- managed builds/regression tests without loading OCCT;
- UI-host, selection, topology, runtime, package, source-organization, and branch-boundary contracts;
- smoke-project compilation;
- direct `main`/`demo` reusable-source synchronization.

Before release, run the real native gate on Windows with OCCT 7.9.0:

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 root>"
```

The local Native Smoke remains authoritative for actual C++ compile/link/load and geometry/topology execution.
