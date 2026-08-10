# OCCT Bridge API Coverage

OcctCSharpBridge `2.6.0` is a Windows x64 bridge for Open CASCADE Technology `7.9.0` and .NET 10. The reusable API is intentionally divided into `OcctEngine` for AIS/viewer work and `OcctModelingSession` for headless modeling.

OCAF/XDE and application-level Document, Command, Tool, Undo/Redo, Snap/Grip, feature-tree and persistence systems are outside the `main` bridge boundary.

- Native bridge version: `2.6.0`
- Native ABI: `3`
- OCCT: `7.9.0`
- Native exports: `348`
- Managed P/Invoke declarations: `348`
- Public .NET types: `105`
- Viewer API: `214`
- Modeling API: `134`

## Managed assemblies

| Assembly | Responsibility |
|---|---|
| `OcctNet` | Core types, runtime loading, `OcctEngine`, `OcctModelingSession`, host-neutral interaction policy |
| `OcctNet.WinForms` | WinForms HWND viewport host |
| `OcctNet.Wpf` | WPF viewport host using the WinForms host internally |
| `OcctNet.Avalonia` | Avalonia `NativeControlHost` backed by a Windows child HWND |

The Avalonia host is Windows-only. It does not imply a Linux/macOS OCCT viewer backend.

## Interactive viewer API

`OcctEngine` covers:

- view, camera, projection, zoom/pan/rotation and screen/world conversion;
- object creation, registration, identity, visibility and deletion;
- appearance, material, transparency, display mode, depth and transforms;
- object and subshape selection, rectangle selection, hover detection and structured selection hits;
- viewer-side geometry, features and annotations;
- STEP/IGES/BREP/STL exchange for viewer-owned shapes.

`GetSelectedHits()` and `TryGetDetectedHit()` return owner-aware `OcctSelectionHit` values. A selection hit contains its registered owner, topology type and runtime subshape index. Runtime subshape indices are not persistent naming.

## Headless modeling API

### Geometry and topology

`OcctModelingSession` covers primitive/curve construction, wire/face/solid assembly, transforms, shape queries, analytic and differential geometry, B-Spline inspection, topology traversal, adjacency, free boundaries, face analysis, shape inspection and oriented bounds.

High-cardinality collections use a two-call bulk C ABI: first query the required size with a null buffer, then copy the complete result in one native call. This applies to session shape enumeration, subshapes, inner wires, ancestors, ray hits, operation history and face mesh arrays.

### Modeling algorithms

Supported operations include Boolean Fuse/Cut/Common/Section, Splitter, Extrude, Revolve, Sweep, Loft, Fillet, Chamfer, Offset, Thick Solid, same-domain unification and healing. Algorithm results carry operation history when OCCT provides it.

### P0 — Inertia properties

The bridge exposes linear, surface and volume inertia properties through:

- `GetLinearInertiaProperties()`
- `GetSurfaceInertiaProperties()`
- `GetVolumeInertiaProperties()`

`OcctInertiaProperties` contains mass, center of mass, the inertia tensor, principal moments, principal axes, radii of gyration and symmetry flags.

### P1 — Structured Edge/Edge intersection

`IntersectEdges()` returns `IReadOnlyList<OcctEdgeIntersection>` rather than a Boolean hit flag. Each result is either:

- `Point` — one common point;
- `Overlap` — a bounded common Edge interval.

The result preserves start/end points and native parameter ranges on both source Edges. Results are transferred through `occt_model_edge_intersections_copy`.

### P2 — Topology references

`CreateTopologyReference()` builds a versioned fingerprint for a Vertex, Edge or Face inside a root shape. The fingerprint includes topology type, geometric type, measure, center, bounds, tolerance, orientation, adjacency counts and a runtime index hint.

`ResolveTopologyReference()` returns one of:

- `Resolved`
- `Ambiguous`
- `Removed`
- `NotFound`
- `InvalidReference`

The runtime index is only a low-weight hint. Resolution is based on the fingerprint and can optionally use OCCT operation history.

### P3 — Native/managed ABI cleanup

The modeling collection ABI is bulk-only. Removed indexed collection pairs include the old shape, topology, ray-hit, history and mesh `Count + At` paths. Managed collection APIs no longer cross the native boundary once per item.

Native responsibilities remain separated by domain:

- session/registry;
- shape and geometry queries;
- topology;
- Boolean/features/healing/history;
- projection/ray/classification;
- mesh;
- exchange;
- inertia;
- structured intersection;
- topology references.

The retired broad `OcctModelingInternal.hxx` umbrella must not be reintroduced.

## Data exchange and mesh

STEP, IGES, BREP and STL import/export are available, including generic file import. Triangulation supports explicit meshing parameters and combined shape mesh provenance through source-face ranges.

## Runtime and ownership

`OcctEngine` and `OcctModelingSession` use owner-aware handles. Cross-engine/session handles are rejected before native execution. Runtime diagnostics expose bridge/OCCT loading information without requiring application-level CAD state.

## Local validation

Static and managed validation:

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
```

Authoritative native compile/link/load and geometry validation with the real OCCT SDK:

```powershell
.\build.ps1 smoke Release
```

The local checks validate native declaration/definition/PInvoke parity, API counts, bulk ABI rules, source organization, UI host boundaries, package contents and the no-OCAF/XDE boundary.
