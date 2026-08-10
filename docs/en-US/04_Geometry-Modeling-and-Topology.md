# 04 Geometry, Modeling and Topology

`OcctModelingSession` is the primary headless modeling entry point. The same underlying native capabilities can also be exposed through viewer-aware operations where appropriate.

## Geometry and primitives

The bridge covers common 3D primitives and curve/surface construction, including lines, circles/arcs and B-Spline/NURBS-oriented geometry. Managed value types represent points, vectors, transforms, bounds and other transfer-safe data without exposing OCCT C++ classes.

## B-Rep and topology

Shapes can be queried by topology type such as vertex, edge, wire, face, shell, solid and compound. Topology enumeration returns owner-aware shape handles rather than raw native pointers.

## Modeling operations

The modeling layer includes Boolean operations, feature construction, offsets/fillets where wrapped, healing/validation, transforms, copying/compounds and shape inspection. Results that need more than a single shape use structured managed result types.

## History and topology reference

Runtime subshape indices are not treated as durable identity. Topology Reference APIs use a versioned geometric/topological fingerprint with runtime indices only as hints. History APIs expose generated/modified/deleted relationships where the underlying OCCT operation supports them.

## Analysis

Inspection APIs cover bounds, validity, topology counts and inertia/mass-style properties. Structured intersection APIs return geometric results rather than forcing callers to infer meaning from raw point arrays.

Use the generated API Reference for exact overloads and result types.