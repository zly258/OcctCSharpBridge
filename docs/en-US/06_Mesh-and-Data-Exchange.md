# 06 Mesh and Data Exchange

## Meshing

`GetShapeMeshData` returns one combined mesh with nodes, triangle connectivity and per-face provenance ranges. Face triangle indices are normalized to the combined node array, allowing consumers to validate or visualize exactly the topology returned by the Bridge.

Meshing parameters expose linear/angular deflection, minimum size, relative/parallel flags, internal vertices and surface-deflection control.

## STEP

`ImportStep()` remains the source-compatible shape-oriented Viewer import API.

Use `ImportStepDocument()` when assembly semantics matter:

```csharp
OcctAssemblyDocument document = engine.ImportStepDocument("assembly.step");
```

The snapshot preserves:

- stable XDE assembly-item IDs;
- Assembly / Instance / Part roles and parent/child relationships;
- reference names;
- local and global occurrence transforms;
- visibility;
- surface RGBA/transparency and curve colors;
- explicit subshape styles.

A Part is defined by XDE product structure, not by the number of contained solids. A legitimate multi-solid Part remains one Part.

When geometry remains unchanged, name/color/transparency/visibility edits can be synchronized back to the pristine imported XDE document, preserving assembly references and untouched subshape styles during STEP export. Topology-changing edits invalidate that pristine-document path and require reconstruction.

## IGES / BREP / STL

IGES and STEP use OCCT exchange toolkits; BREP persists OCCT topology directly; STL exchanges triangulated geometry. Runtime resource directories may be required in addition to DLLs.

XDE here is an internal STEP exchange implementation detail, not the application document architecture.
