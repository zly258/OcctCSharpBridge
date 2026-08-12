# API Coverage and Boundary

## Session separation

The bridge has three independent high-level sessions:

- `OcctModelingSession`: headless `TopoDS_Shape` construction, algorithms, analysis, healing, mesh and pure-shape exchange;
- `OcafDocument`: OCAF/TDF/TNaming/XDE document, metadata, assembly, persistence and STEPCAF/IGESCAF exchange;
- `OcctEngine`: AIS/V3d presentation, selection and annotations.

Native registries are deliberately independent. Shapes are copied between the modeling registry, OCAF/XDE documents and the Viewer; no C++ pointer or lifetime dependency crosses the stable C ABI.

## Native modules

| File | Responsibility |
|---|---|
| `OcctModeling.h` and `OcctModeling*.cpp` | Headless C ABI, shape registry, topology, algorithms, history, healing, mesh and pure-shape exchange |
| `OcctOcaf.h` and `OcctOcaf*.cpp` | OCCT 7.9.0 OCAF documents, TDF/TDataStd/TDataXtd, TNaming, XDE and metadata-preserving exchange |
| `OcctNative.h` and existing `.cpp` files | Viewer/AIS-compatible C ABI |
| `OcctInternal.hxx`, `OcctModelingInternal.hxx`, `OcctOcafInternal.hxx` | Private native state and runtime helpers |

## Managed modules

| Type | Responsibility |
|---|---|
| `OcctModelingSession` | Headless public modeling API and native shape lifetime |
| `OcafDocument` | OCAF/XDE document, labels, attributes, TNaming, assemblies, metadata and persistence |
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

## Implemented OCAF/XDE groups

- exact OCCT 7.9.0 version enforcement;
- `TDocStd_Document` lifecycle, persistence, transactions and undo/redo;
- TDF label hierarchy and generic attribute inspection;
- common TDataStd/TDataXtd scalar, reference, array, position and shape attributes;
- TNaming evolution and selector workflows;
- XDE shapes, assemblies, components and locations;
- XDE colors, visibility, layers, physical materials and validation properties;
- BinXCAF/XmlXCAF/BinOcaf/XmlOcaf persistence;
- STEPCAF and IGESCAF metadata-preserving exchange.

See [OCAF/XDE coverage](OCAF_COVERAGE.md) for the detailed contract and advanced boundaries.

## Stable topology guidance

Do not persist traversal indices as long-term references across reconstruction. For transient modeling, use the `OperationId` history in `OcctModelingSession`. For document-backed identity and regeneration, store shapes and selections through `OcafDocument` TNaming APIs inside OCAF command transactions.

## Extension policy

New native functionality must be exposed as a narrow, versioned C ABI workflow. Structures crossing the ABI must remain blittable and free of C++ standard-library ownership. Raw OCCT handles, labels, attributes, drivers and callback objects must remain native implementation details.
