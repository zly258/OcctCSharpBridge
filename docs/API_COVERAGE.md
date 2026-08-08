# OCCT Bridge API Coverage

OcctCSharpBridge 2.6 is a Windows x64 OCCT 7.9.0 bridge with two intentional managed façades:

- `OcctEngine`: interactive AIS/viewer/document operations for desktop CAD applications.
- `OcctModelingSession`: headless geometry, topology, algorithms, meshing, analysis, and exchange.

OCAF/XDE is intentionally excluded. Document persistence, undo/redo, application entities, and JSON state belong to the application layer.

- Native bridge version: `2.6.0`
- Native ABI: `3`
- OCCT: `7.9.0`
- Native exports: `345`
- Managed P/Invoke declarations: `345`
- Public .NET types: `90`
- Viewer API: `212`
- Modeling API: `133`

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
| Mesh | triangulation vocabulary | `Triangulate()`, `GetShapeMeshData()` |
| Native C ABI | exact `occt_...` symbol | `occt_model_trim_edge` |

Public object handles are session/engine owned. Raw `long` IDs cannot be used to construct `OcctShape` or `OcctModelShape`; persisted IDs must be resolved through `GetShape()`, `TryGetShape()`, or `GetObject()`.

Native 0/1 flags are not exposed as managed `int` options. Public modeling options use `bool` and enums; internal P/Invoke DTOs perform the ABI conversion.

## Assembly responsibilities

| Assembly | Responsibility |
|---|---|
| `OcctNet` | Core types, interactive engine, headless modeling session, runtime loading and diagnostics |
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
- Local adjacency helpers: `GetAdjacentFaces()`, `GetIncidentEdges()`, and `GetIncidentFaces()`.
- `AnalyzeEdgeAdjacency()` builds one native Edge→distinct-Face map for the entire root shape and returns `OcctEdgeAdjacencyResult`, including isolated, boundary-candidate, manifold-interior, and non-manifold classifications. Existing `GetBoundaryEdgeCandidates()`, `GetManifoldInteriorEdges()`, `GetNonManifoldEdges()`, and `GetEdgesByAdjacentFaceCount()` reuse this batch path.
- `GetBoundaryEdgeCandidates()` intentionally returns topological candidates; periodic seam topology may require a stricter free-boundary analysis before every returned edge is treated as an open geometric edge.
- `AnalyzeFreeBounds()` runs OCCT `ShapeAnalysis_FreeBounds` and returns `OcctFreeBoundsResult` with closed and open free-boundary wires plus the tolerance used. See [Topology Adjacency and Free-Boundary Analysis](TOPOLOGY_ANALYSIS.md).
- `IsSameShape()` and `IsPartnerShape()` expose OCCT topological identity semantics.

### Geometry and differential geometry

- Vertex point and edge endpoints.
- Normalized edge evaluation and exact curve-parameter evaluation.
- Curve/surface type queries and exact analytic parameters.
- Edge parameter ranges, derivatives, tangent/normal, curvature, and center of curvature.
- Face U/V bounds, periodicity, derivatives, normals, and principal/mean/Gaussian curvature.
- Point projection to edge/face, ray intersections, and solid point classification.
- `TrimEdge()` creates an edge from an exact sub-range of the source curve.
- `GetBSplineCurveData()` returns an immutable B-Spline curve snapshot containing degree, rational/periodic flags, poles, weights, distinct knots, and multiplicities.
- `GetBSplineSurfaceData()` returns an immutable B-Spline surface snapshot containing U/V degree, rational/periodic flags, pole/weight grid, U/V knots, and multiplicities. Surface pole storage is U-major with V varying fastest, while `GetPole(u,v)` / `GetWeight(u,v)` provide direct grid access.
- Managed B-Spline collections are zero-based even though OCCT indexes poles and knots from one. See [B-Spline Curve and Surface Inspection](BSPLINE_CURVES.md).

### Modeling algorithms

- Fuse, cut, common, section, splitter.
- Extrude, revolve, sweep, loft.
- Edge fillet and chamfer.
- `OffsetShape()` for topological/3D offset.
- `OffsetWire()` for planar wire offset with arc/tangent/intersection join rules.
- Thick solid, same-domain unification, shape healing, and operation history.

### Triangulation and provenance

- `Triangulate()` creates OCCT triangulation using managed `OcctModelMeshParameters`.
- `GetFaceMesh()` returns one Face mesh.
- `GetShapeMesh()` remains the compatibility API for one combined `OcctMesh`.
- `GetShapeMeshData()` returns the same combined mesh plus `OcctShapeMeshFaceRange` entries that preserve each source Face's contiguous node/triangle contribution.
- `OcctShapeMeshData.GetFaceForNode()` and `GetFaceForTriangle()` resolve combined mesh indices back to source `OcctModelShape` Faces without adding a Native ABI call or a per-triangle FaceId array.
- `ClearTriangulation()` removes cached triangulation.

See [Shape Mesh Face Provenance](MESH_PROVENANCE.md).

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
- `OcctRuntime` resolves application-local runtime files first.
- `OcctRuntime.GetDiagnosticReport()` remains the full human-readable troubleshooting report and is side-effect free.
- `OcctRuntime.GetDiagnosticInfo()` returns a typed `OcctRuntimeDiagnosticInfo` snapshot with process/OS architecture, configured bridge/OCCT paths and existence states, already-loaded `OcctNative.dll` / `TKernel.dll` paths, and the original text report. The snapshot does not configure or force native loading. See [Structured Runtime Diagnostics](RUNTIME_DIAGNOSTICS.md).
- Native errors become `OcctException` with operation and native-message metadata.

## Validation

Cloud CI has no project OCCT SDK, so it validates declarations and managed code rather than pretending to run native geometry:

- Native declarations and C# P/Invoke symbols are name-for-name consistent.
- Every P/Invoke uses Cdecl and exact symbol spelling.
- Counts come from `bridge-contract.json`.
- Managed projects and managed-only regression tests run in CI.
- Managed geometry/transform helpers and structured runtime diagnostics are regression-tested without loading OCCT.
- B-Spline curve/surface declaration, definition, P/Invoke, and high-level API parity is checked statically.
- Batched edge adjacency and strict free-boundary topology analysis have a dedicated static contract check.
- Shape-mesh provenance source organization, Smoke coverage, and bilingual documentation are part of the geometry contract check.
- `main` and `demo` reusable wrapper content is compared directly.

Before release, run on Windows with OCCT 7.9.0:

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 root>"
```

The native smoke suite covers ABI/version loading, Booleans, batched adjacency, strict free-boundary analysis, analytic/differential geometry, B-Spline curve/surface data extraction, mesh provenance, OBB, shape identity, face-with-hole construction, edge trimming, planar wire offset, whole-shape triangulation, loft, healing, and BREP/STEP round trips.
