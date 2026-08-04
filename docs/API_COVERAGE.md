# API Coverage and Boundary

## Session separation

`OcctModelingSession` is the authoritative headless modeling API. It owns a native registry of `TopoDS_Shape` values and operation-history objects. `OcctEngine` remains the AIS/V3d presentation API. The two registries are deliberately independent.

## Native modules

| File | Responsibility |
|---|---|
| `OcctModeling.h/.cpp` | Headless C ABI, shape registry, topology, algorithms, history, healing, mesh and data exchange |
| `OcctNative.h` and existing `.cpp` files | Viewer/AIS-compatible legacy C ABI |
| `OcctInternal.hxx` | Existing Viewer internals and shared runtime helpers |

## Managed modules

| Type | Responsibility |
|---|---|
| `OcctModelingSession` | Headless public API and native lifetime |
| `OcctModelShape` | Session-local shape ID |
| `OcctModelAlgorithmResult` | Result shape, operation ID, warning/error flags and report |
| `OcctFaceMesh` | Face triangulation nodes and triangles |
| `OcctEngine` | Viewer and AIS operations |

## Implemented headless groups

- shape lifecycle, metadata, transform and mass properties;
- topology traversal, wires and ancestor relations;
- vertex/edge/face geometry evaluation;
- common geometry and primitive construction;
- Boolean operations and Splitter with advanced BOP options;
- extrusion, revolution, sweep, loft, fillet, chamfer, offset and thick solid;
- same-domain unification and general ShapeFix healing;
- BRepTools history for generated, modified and removed topology;
- point projection, ray/shape intersection and solid classification;
- explicit meshing and face triangulation extraction;
- STEP, IGES, BREP and STL pure-shape exchange;
- copy-to-viewer interoperability.

## Deliberately excluded

- OCAF/TDF/TDocStd/TDataStd;
- TNaming and OCAF feature naming;
- OCAF command transactions and undo/redo;
- XDE document tools and assembly metadata persistence;
- a direct one-to-one projection of every OCCT class into C#.

## Stable topology guidance

Do not persist traversal indices as long-term references across reconstruction. Use the `OperationId` returned by modeling algorithms and query generated/modified/removed relations while the source shapes and operation remain in the same `OcctModelingSession`.

## Extension policy

New native functionality should be added as a narrow C ABI workflow rather than exposing OCCT C++ handles. Structures crossing the ABI must remain blittable, versionable and free of C++ standard-library ownership.
