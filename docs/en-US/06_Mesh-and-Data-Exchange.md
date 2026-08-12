# Mesh and Data Exchange

Mesh and exchange APIs belong to Core and do not depend on a desktop window system.

The Bridge supports configurable triangulation and engineering exchange based on the contracted OCCT 7.9 toolkits, including STEP assembly workflows, IGES, BREP and STL-oriented operations.

STEP assembly import uses XDE internally to preserve hierarchy, occurrences, transforms, visibility, colors and subshape styles, then projects that state to managed `OcctAssemblyDocument` snapshots.

OCAF/XDE is not the consuming application's document/persistence architecture. These semantics are the same on Windows and Linux subject to the installed OCCT runtime.