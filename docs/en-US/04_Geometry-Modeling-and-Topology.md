# 04 Geometry, Modeling and Topology

`OcctModelingSession` is the headless modeling boundary. It does not require a Viewer window.

## Modeling

The managed API covers primitives, wires/faces, Boolean operations, extrusion, revolution, sweep, loft, fillet, chamfer, offsets, shelling, healing and operation history. Results retain explicit session ownership.

## Geometry analysis

Strongly typed queries cover curve/surface type, parameter ranges, UV bounds, evaluation, derivatives, curvature, analytic geometry, projection, ray hits, distances and inertia properties.

## Topology analysis

Use topology traversal and structured analysis APIs for vertices, edges, wires, faces, shells, solids, adjacency and shape inspection. High-cardinality results use bulk Native copies where practical.

## Persistent references

`OcctTopologyReference` provides an application-facing way to describe and later resolve subshape identity across controlled modeling changes. It is intentionally different from raw transient OCCT handles.

## Viewer handoff

A shape created in `OcctModelingSession` can be copied to `OcctEngine.Display(model, shape)`. The Viewer then owns its presentation object; the headless session continues to own the modeling handle.

See the generated API reference for exact signatures.
