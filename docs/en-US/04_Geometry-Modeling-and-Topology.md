# Geometry, Modeling and Topology

`OcctModelingSession` provides headless modeling independently from the Avalonia Viewer. The same Core can be used in Windows or Linux processes without creating a viewport.

Capabilities include primitives, Boolean operations, extrude/revolve/sweep/loft, fillet/chamfer, offset/shelling, healing, validation, geometry evaluation, topology traversal, adjacency, intersections, inertia, mesh generation and operation history.

Shape handles remain scoped to their owning session. Product-level identity and persistence stay in the application layer. Topology-reference helpers support re-identification after modeling operations.

High-cardinality results use bulk Native APIs to avoid N+1 interop patterns.