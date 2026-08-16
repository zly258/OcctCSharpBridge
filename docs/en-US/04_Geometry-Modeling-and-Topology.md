# Geometry, Modeling and Topology

`OcctModelingSession` is the headless modeling owner on `main`. Its semantics are independent of WinForms, WPF and Avalonia and remain reusable by every supported consumer host.

The Modeling surface includes primitives, Boolean operations, extrusion/revolution/sweep/loft, fillet/chamfer, offset/shelling, healing, mesh generation, shape validation, geometry evaluation, topology traversal, adjacency, intersections, inertia and operation history.

Shape handles are scoped to the owning modeling session. Persistent application identity must be managed above the Bridge; topology-reference helpers are available to re-identify subshapes after modeling operations.

High-cardinality queries use bulk Native APIs. Avoid per-face/per-edge P/Invoke loops in application code when a bulk result is available.
