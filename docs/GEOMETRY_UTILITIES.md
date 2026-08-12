# Managed Geometry and Transform Utilities

`OcctGeometryExtensions` contains pure managed helpers for calculations that do not require an OCCT call. They are intended for CAD/BIM application code that needs lightweight vector math, bounds checks, coordinate transforms, or topology convenience before invoking the native kernel.

## Conventions

- Length units are application-defined and must be consistent with the geometry passed to OCCT.
- Angles are in **radians**.
- `OcctModelLocation` and `OcctTransform3d` use row-major affine matrices with column-vector semantics.
- For `left.Multiply(right)`, `right` is applied first, then `left`.
- Transforming a point includes translation; transforming a vector does not.
- `OcctModelLocation` helpers require an affine last row `[0, 0, 0, 1]`, matching the `gp_Trsf` mapping used by the native bridge.

## Points and vectors

```csharp
var a = new OcctPoint3d(0, 0, 0);
var b = new OcctPoint3d(100, 50, 0);
var midpoint = a.Lerp(b, 0.5);

var direction = new OcctVector3d(1, 1, 0);
var angle = direction.AngleTo(OcctVector3d.UnitX);
var xComponent = direction.ProjectOnto(OcctVector3d.UnitX);
var perpendicular = direction.RejectFrom(OcctVector3d.UnitX);
```

`AlmostEquals()` is provided for points and vectors so callers do not need exact floating-point equality for geometric checks.

## Bounds

```csharp
var bounds = model.GetShapeBounds(shape);

if (bounds.IsValid() && bounds.Contains(testPoint, tolerance: 1e-6))
{
    var volume = bounds.GetVolume();
    var diagonal = bounds.GetDiagonalLength();
}

var padded = bounds.Expanded(5.0);
var combined = bounds.Union(otherBounds);
var overlaps = bounds.Intersects(otherBounds, tolerance: 1e-6);
```

These helpers operate on axis-aligned `OcctBounds`; they do not replace exact OCCT interference or distance calculations.

## UV and distance results

```csharp
var uv = model.GetFaceUvBounds(face);
var center = uv.GetCenter();
var insideParameterRange = uv.Contains(center.U, center.V);

var distance = model.GetShapeDistance(first, second);
var separation = distance.GetSeparationVector();
var midpoint = distance.GetMidpoint();
var touching = distance.IsWithin(1e-6);
```

## Locations and object transforms

Create a translation, rotation, or uniform scale without a native call:

```csharp
var move = OcctGeometryExtensions.CreateTranslationLocation(100, 0, 0);
var rotate = OcctGeometryExtensions.CreateRotationLocation(
    OcctVector3d.UnitZ,
    Math.PI / 2,
    center: OcctPoint3d.Origin);

var transform = move.Multiply(rotate); // rotate first, then move
var worldPoint = transform.TransformPoint(localPoint);
var worldDirection = transform.TransformVector(localDirection);

var localPointAgain = transform.Inverted().TransformPoint(worldPoint);
```

Viewer/object transforms and headless-modeling locations can be converted explicitly:

```csharp
OcctTransform3d viewerTransform = transform.ToTransform3d();
OcctModelLocation modelLocation = viewerTransform.ToModelLocation();
```

This keeps the two existing public transform representations interoperable without changing the native ABI.

## Topology convenience

The generic topology API remains available:

```csharp
var faces = model.GetSubshapes(shape, OcctShapeType.Face);
```

For common operations, convenience methods reduce repeated enum plumbing:

```csharp
var vertices = model.GetVertices(shape);
var edges = model.GetEdges(shape);
var faces = model.GetFaces(shape);
var solids = model.GetSolids(shape);

var faceEdges = model.GetFaceEdges(face);
var wireEdges = model.GetWireEdges(wire);
var edgeVertices = model.GetEdgeVertices(edge);
var counts = model.GetTopologyCounts(shape);
```

All returned `OcctModelShape` instances retain the owning `OcctModelingSession`. The same cross-session ownership rules still apply.

## When to use native OCCT instead

Use the native-backed APIs when the result depends on exact B-Rep geometry or topology, including:

- shape-to-shape distance and witness points;
- point projection onto curves/surfaces;
- ray intersections and solid classification;
- Boolean and feature operations;
- exact curve/surface evaluation;
- topology identity and ancestry;
- meshing and engineering file exchange.

The managed utilities are intentionally deterministic, allocation-light helpers around existing bridge value types rather than a second geometry kernel.
