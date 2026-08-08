# OCCT Bridge API Coverage

OcctCSharpBridge 2.6 is a Windows x64 OCCT 7.9.0 bridge with two intentional managed façades:

- `OcctEngine`: interactive AIS/viewer/document operations for desktop CAD applications.
- `OcctModelingSession`: headless geometry, topology, algorithms, meshing, analysis, and exchange.

OCAF/XDE is intentionally excluded. Document persistence, undo/redo, application entities, and JSON state belong to the application layer.

- Native bridge version: `2.6.0`
- Native ABI: `3`
- OCCT: `7.9.0`
- Native exports: `336`
- Managed P/Invoke declarations: `336`
- Public .NET types: `82`
- Viewer API: `212`
- Modeling API: `124`

## 2.6 API rules

Bridge 2.6 removes compatibility aliases instead of carrying multiple names for the same operation.

| Area | Rule | Example |
|---|---|---|
| Shape query | `GetShape...` / `IsShape...` / `SetShape...` | `GetShapeBounds()` |
| Edge query | `GetEdge...` / `EvaluateEdge...` | `GetEdgeCurveType()` |
| Face query | `GetFace...` / `EvaluateFace...` | `GetFaceUvBounds()` |
| Indexed topology | `...At` suffix | `GetSubshapeAt()` |
| Construction | `Make...` | `MakePlanarFace()` |
| Algorithms | operation verb | `Extrude()`, `OffsetWire()` |
| Mesh | triangulation vocabulary | `Triangulate()`, `GetShapeMesh()` |
| Native C ABI | exact `occt_...` symbol | `occt_model_trim_edge` |

Public object handles are session/engine owned. Raw `long` IDs cannot be used to construct `OcctShape` or `OcctModelShape`; persisted IDs must be resolved through `GetShape()`, `TryGetShape()`, or `GetObject()`.

Native 0/1 flags are not exposed as managed `int` options. Public modeling options use `bool` and enums; internal P/Invoke DTOs perform the ABI conversion.

## Assembly responsibilities

| Assembly | Responsibility |
|---|---|
| `OcctNet` | Core types, interactive engine, headless modeling session, runtime loading |
| `OcctNet.WinForms` | Reusable WinForms OCCT viewport host |
| `OcctNet.Wpf` | Reusable WPF OCCT viewport host |

The full WinForms/WPF/Avalonia CAD applications live on the `demo` branch, not in `main`.

## Interactive `OcctEngine`

`OcctEngine` owns displayed AIS objects and the native viewer context. Its shape methods are appropriate when construction is part of an interactive CAD document and the result must immediately participate in visibility, selection, appearance, and manipulation.

Coverage includes camera/view control, screen/world conversion, selection, object lifecycle, appearance, transformations, interactive primitives/features, annotations, and STEP/IGES/BREP/STL exchange. Lighting uses the strongly typed `OcctSceneLightingSettings` API; the older simplified lighting overload and redundant C ABI aliases were removed in ABI 3.

## Headless `OcctModelingSession`

### Construction

- Vertex, line, polyline, circle, arc, regular polygon, ellipse, Bezier, interpolated B-Spline.
- Rectangle wire and planar rectangular face.
- Wire, compound, sewn shell, solid from shell.
- Box, cylinder, cone, sphere, torus, wedge.
- `MakePlanarFace(outerWire, innerWires)` constructs planar faces with holes directly instead of requiring Boolean cuts.

### Topology and shape queries

- Shape type, orientation, closure, validity, maximum tolerance, validation report, hash.
- Axis-aligned bounds and `GetShapeOrientedBounds()` OBB.
- Linear/surface/volume mass properties and shape distance.
- Location read/write.
- Generic subshape traversal, outer/inner wires, ancestor queries.
- Convenience collections: `GetVertices()`, `GetEdges()`, `GetWires()`, `GetFaces()`, `GetShells()`, `GetSolids()`, `GetCompSolids()`, and `GetCompounds()`.
- Local topology helpers: `GetEdgeVertices()`, `GetWireEdges()`, `GetFaceEdges()`, `GetFaceVertices()`, and `GetTopologyCounts()`.
- `IsSameShape()` and `IsPartnerShape()` expose OCCT topological identity semantics.

### Geometry and differential geometry

- Vertex point and edge endpoints.
- Normalized edge evaluation and exact curve-parameter evaluation.
- Curve/surface type queries and exact analytic parameters.
- Edge parameter ranges, derivatives, tangent/normal, curvature, and center of curvature.
- Face U/V bounds, periodicity, derivatives, normals, and principal/mean/Gaussian curvature.
- Point projection to edge/face, ray intersections, and solid point classification.
- `TrimEdge()` creates an edge from an exact sub-range of the source curve.

### Modeling algorithms

- Fuse, cut, common, section, splitter.
- Extrude, revolve, sweep, loft.
- Edge fillet and chamfer.
- `OffsetShape()` for topological/3D offset.
- `OffsetWire()` for planar wire offset with arc/tangent/intersection join rules.
- Thick solid, same-domain unification, shape healing, and operation history.

### Triangulation

`Triangulate()` creates OCCT triangulation using managed `OcctModelMeshParameters`. `GetFaceMesh()` returns one face mesh; `GetShapeMesh()` combines all face triangulations into one `OcctMesh` with adjusted triangle indices. `ClearTriangulation()` removes cached triangulation.

### Exchange

STEP, IGES, BREP, and STL import/export are exposed directly, plus generic file import. STL export accepts explicit tessellation parameters.

## Pure managed geometry utilities

`OcctGeometryExtensions` adds allocation-light calculations around existing bridge value types without invoking Native OCCT:

- point interpolation and tolerance-aware point/vector comparison;
- vector angle, projection, and rejection;
- AABB validation, containment, intersection, expansion, union, volume, and diagonal length;
- UV-bound validation, center, and containment;
- distance-result separation vector, midpoint, and tolerance test;
- affine point/vector transformation, composition, inversion, translation, rotation, and uniform scale;
- conversion between `OcctModelLocation` and `OcctTransform3d`.

Angles are radians. Matrix multiplication uses row-major affine matrices with column-vector semantics: `left.Multiply(right)` applies `right` first. See [Managed Geometry and Transform Utilities](GEOMETRY_UTILITIES.md).

## Runtime and ownership

- `OcctEngine` and `OcctModelingSession` use `SafeHandle` internally.
- Every managed object/shape returned by the bridge carries an internal owner token.
- Cross-engine and cross-session object use is rejected before native invocation.
- `OcctRuntime` resolves application-local runtime files first and provides `GetDiagnosticReport()` for deployment failures.
- Native errors become `OcctException` with operation and native-message metadata.

## Validation

Cloud CI has no OCCT SDK, so it validates declarations and managed code rather than pretending to run native geometry:

- Native declarations and C# P/Invoke symbols are byte-for-name consistent.
- Every P/Invoke uses Cdecl and exact symbol spelling.
- Counts come from `bridge-contract.json`.
- Managed projects and managed-only regression tests run in CI.
- Managed geometry/transform helpers are regression-tested without an OCCT runtime.
- `main` and `demo` reusable wrapper content is compared directly.

Before release, run on Windows with OCCT 7.9.0:

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 root>"
```

The native smoke suite covers ABI/version loading, Booleans, topology, analytic/differential geometry, OBB, shape identity, face-with-hole construction, edge trimming, planar wire offset, whole-shape triangulation, loft, healing, and BREP/STEP round trips.
