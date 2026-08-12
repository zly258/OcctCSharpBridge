# Mesh and Data Exchange

The Core provides configurable triangulation plus STEP, IGES, BREP and STL-oriented exchange capabilities supported by the contracted OCCT 7.9 toolkits.

STEP assembly import uses XDE internally to preserve product hierarchy, occurrences, transforms, visibility, colors and subshape styles. Managed consumers receive `OcctAssemblyDocument` / `OcctAssemblyNode` snapshots rather than an exposed OCAF document model.

Valid multi-solid STEP parts remain one logical Part where the source product structure says they are one Part.

Non-geometric STEP metadata can round-trip through the retained imported XDE representation while geometry remains unchanged.

These Core exchange semantics are host-independent; the `avalonia` branch reuses the same Core concepts under its separate cross-platform source contract.