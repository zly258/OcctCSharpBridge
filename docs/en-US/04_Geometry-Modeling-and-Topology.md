# Geometry, Modeling, and Topology Analysis

`OcctModelingSession` is the owner of headless modeling resources. Its semantics are independent of WinForms, WPF, and Avalonia and can be reused by every supported consumer host.

Modeling capabilities include primitives, booleans, extrude, revolve, sweep, loft, fillet, chamfer, offset, shelling, healing, mesh, shape validation, geometry evaluation, topology traversal, adjacency, intersection, inertia, and operation history.

Shape handles remain valid only during the lifetime of their owning modeling session. Product-level persistent IDs belong to the application layer; the bridge provides topology references for subshape re-identification after modeling operations.

High-cardinality results use bulk native APIs where possible to avoid N+1 P/Invoke calls per face or edge.

## Concurrency and asynchronous operations

Native state in `OcctModelingSession` is not thread-safe. Operations submitted through `BooleanAsync`, `FuseAsync`, `CutAsync`, `CommonAsync`, `SectionAsync`, `SplitAsync`, `ImportStepAsync`, or `ExportStepAsync` are executed sequentially for each session, preventing concurrent async mutations of the same native session.

Synchronous methods do not participate in the async queue. Do not call synchronous methods on a session while one of its async operations is running. Create a separate `OcctModelingSession` for each workflow that must run in parallel.

A cancellation token is honored while an operation is waiting to enter the queue. Once a native OCCT algorithm has started, it cannot be interrupted safely and is allowed to run to completion instead of being reported as cancelled.
