# Mesh and Data Exchange

The Core provides configurable triangulation plus STEP, IGES, BREP and STL-oriented exchange capabilities supported by the contracted OCCT 7.9 toolkits.

STEP assembly import uses XDE internally to preserve product hierarchy, occurrences, transforms, visibility, colors and subshape styles. Managed consumers receive `OcctAssemblyDocument` / `OcctAssemblyNode` snapshots rather than an exposed OCAF document model.

Valid multi-solid STEP parts remain one logical Part where the source product structure says they are one Part.

Non-geometric STEP metadata can round-trip through the retained imported XDE representation while geometry remains unchanged.

These Core exchange semantics are host-independent. WinForms, WPF and Avalonia all consume the same `OcctNet` Core contract; Linux Avalonia uses the same exchange model as Windows rather than a separate branch-specific contract.

## Background import into the viewer

`OcctEngine.ImportAsync`, `ImportStepAsync`, `ImportIgesAsync`, `ImportBrepAsync`, and `ImportStlAsync` parse files on the thread pool through an isolated headless `OcctModelingSession`. After parsing, only the TopoDS shape is copied to the surface thread to create the AIS viewer shape, so background threads never access the viewer.

Applications can also call `CreateShapeFromModel(session, shape)` to add an existing modeling shape to the viewer. Call it on the engine surface thread. The native bridge obtains a shape copy while holding the modeling-session lock before creating the viewer presentation.

Cancellation is honored before parsing starts and before the viewer commit. An OCCT parser already running is not forcibly interrupted, but a cancelled result is not added to the viewer.


## Direct mesh buffer copies

`OcctMeshResource.CopyVertices(Span<OcctMeshVertex>)` and `CopyTriangles(Span<OcctModelMeshTriangle>)` write directly into caller-owned pinned buffers. This avoids the intermediate marshalling arrays used by object-oriented mesh snapshots and is recommended for render uploads, large-model processing, and reusable pooled buffers.

Query `NodeCount` and `TriangleCount` before renting or allocating buffers. A destination smaller than the corresponding native count is rejected before entering the copy operation. Existing `GetMesh()` behavior remains compatible.


For per-face processing, use `GetFaceMeshCounts` followed by `CopyFaceMesh`. Combined shape meshes now precompute every face range, allocate exact final buffers once, and copy each face directly into its final slice instead of building temporary per-face meshes and growing lists.


## Editing imported STEP/XDE nodes

Nodes returned by `ImportStepDocument` can be edited through `SetStepNodeName`, `SetStepNodeVisibility`, and `SetStepOccurrenceTransform`. Node IDs are XDE path identifiers rather than display names, so duplicate names do not make edits ambiguous. Occurrence transforms are limited to component-instance nodes.

The managed node snapshot is updated after a successful native edit. Export with `ExportAllStep` to write the retained XDE document. Global transforms of descendants should be treated as an import-time snapshot until the document is read again.


`AddStepComponent` creates another occurrence of an existing XDE definition under an assembly and returns a refreshed document snapshot. `RemoveStepComponent` removes a leaf occurrence and its matching Viewer object. Both operations rebuild the leaf-to-viewer mapping before returning. Use the returned snapshot for subsequent edits; earlier snapshots intentionally remain immutable representations of their read time.


`SetStepNodeSurfaceColor` writes XDE surface RGBA, including transparency through alpha. `SetStepNodeCurveColor` writes the curve RGB style. Passing `null` clears the explicit color so XDE style inheritance can apply again. Every component must be finite and between zero and one.


`SetStepNodeLayer` adds or removes a node assignment in the XDE layer table. `SetStepLayerVisibility` changes the global visibility of an existing layer. Layer names are included in refreshed assembly node snapshots and are preserved by STEP export when supported by the target schema.
