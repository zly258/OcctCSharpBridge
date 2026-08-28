# Geometry, Modeling, and Topology Analysis

`OcctModelingSession` is the owner of headless modeling resources. Its semantics are independent of WinForms, WPF, and Avalonia and can be reused by every supported consumer host.

Modeling capabilities include primitives, booleans, extrude, revolve, sweep, loft, fillet, chamfer, offset, shelling, healing, mesh, shape validation, geometry evaluation, topology traversal, adjacency, intersection, inertia, and operation history.

Shape handles remain valid only during the lifetime of their owning modeling session. Product-level persistent IDs belong to the application layer; the bridge provides topology references for subshape re-identification after modeling operations.

High-cardinality results use bulk native APIs where possible to avoid N+1 P/Invoke calls per face or edge.

## Geometry inspection and algorithms

The geometry layer exposes canonical managed entry points for analytic and free-form inspection without introducing a second geometry object hierarchy.

Curve and surface inspection includes line, circle, ellipse, parabola, hyperbola, Bezier, B-Spline, plane, cylinder, cone, sphere, torus, extrusion, revolution, and offset surface data. Bezier and B-Spline control data is transferred through bulk native buffers; the managed `GetBezierCurveData`, `GetBezierSurfaceData`, `GetBSplineCurveData`, and `GetBSplineSurfaceData` APIs do not perform per-pole or per-knot P/Invoke calls.

Geometry algorithms include point projection, shape distance, curve/curve extrema, curve/surface extrema, surface/surface extrema, parameterized edge/edge and edge/face intersection, ray intersection, and underlying surface/surface intersection. `IntersectEdgeFace` reports both point intersections and tangential curve-on-surface overlap segments through `OcctIntersectionKind`. `IntersectSurfaces` returns the OCCT intersection curves as session-owned topology; Boolean `Section` remains the topology-level section operation.

Mesh inspection exposes face nodes, normals, UV coordinates, triangle connectivity, face ranges, and shape-level mesh aggregation through the existing mesh APIs. These capabilities are not duplicated by separate inspection-specific interfaces.

## Concurrency and asynchronous operations

Native state in `OcctModelingSession` is not thread-safe. Operations submitted through `BooleanAsync`, `FuseAsync`, `CutAsync`, `CommonAsync`, `SectionAsync`, `SplitAsync`, `ImportStepAsync`, or `ExportStepAsync` are executed sequentially for each session, preventing concurrent async mutations of the same native session.

The native session also serializes synchronous and asynchronous entry points, so a synchronous call cannot mutate shapes, operations, intersection caches, or error state concurrently with an async call. Applications should still avoid mixing synchronous calls with an in-flight async operation because the calling thread may block for a long time. Create a separate `OcctModelingSession` for each workflow that must run in parallel.

Each calling thread has an independent last-error context. A successful or failed call on one thread does not clear an unread error message produced by another thread.

A cancellation token is honored while an operation is waiting to enter the queue. Once a native OCCT algorithm has started, it cannot be interrupted safely and is allowed to run to completion instead of being reported as cancelled.
