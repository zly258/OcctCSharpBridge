# OcctScript command reference

The first preview focuses on ordinary parametric CAD construction and intentionally excludes steel-profile generators, complex transition parts and arrays.

## Curves and wires

| Type | Output | Main fields |
| --- | --- | --- |
| `Vertex` | Vertex | `point` |
| `Line` | Edge | `start`, `end` |
| `Polyline` | Wire | `points`, `closed` |
| `Circle` | Edge | `center`, `normal`, `radius` |
| `Arc` | Edge | `start`, `middle`, `end` |
| `Ellipse` | Edge | `center`, `normal`, `majorRadius`, `minorRadius` |
| `RegularPolygon` | Wire / Face | `center`, `normal`, `xDirection`, `radius`, `sideCount`, `makeFace` |
| `Bezier` | Edge | `poles` |
| `BSpline` | Edge | `points`, `periodic`, `tolerance` |
| `Rectangle` | Wire | `origin`, `xDirection`, `normal`, `width`, `height` |
| `Wire` | Wire | `curves` |

## Surfaces

`Face` creates a planar face from a closed edge/wire. `PlaneFace` creates a rectangular face directly.

## Primitive / topology solids

`Box`, `Cylinder`, `Cone`, `Sphere`, `Torus`, `Wedge`, `Compound`, `Sew`, `SolidFromShell`.

## Features

| Type | Main fields |
| --- | --- |
| `Extrude` | `profile`, `direction`, `distance` |
| `Revolve` | `profile`, `axisPoint`, `axisDirection`, `angle` |
| `Sweep` | `spine`, `profile` |
| `Loft` | `sections`, `makeSolid`, `ruled`, `tolerance` |
| `Fillet` | `shape`, `edgeIndices`, `radius` |
| `Chamfer` | `shape`, `edgeIndices`, `distance` |
| `Offset` | `shape`, `offset`, `tolerance` |
| `Shell` | `solid`, `faceIndices`, `thickness`, `tolerance` |

`Sweep` preserves the dimensional meaning of its profile: sweeping an edge or wire normally produces a face or shell, while sweeping a face is the supported path for producing a solid. For a circular solid pipe, use `Circle -> Face -> Sweep` rather than `Circle -> Sweep`.

Topology indices used by `Fillet`, `Chamfer` and `Shell` are zero-based. A future persistent subshape-naming layer can replace index-based selection without changing the command registry.

## Boolean

`Fuse`, `Cut`, `Common` and `Section` use `left`, `right` and optional `fuzzyValue`.

## Explicit transforms

`Move`, `RotateShape`, `ScaleShape`, `Mirror` create transformed copies of referenced shapes. Every command also contains a generic post-build `transform` block.

## Field syntax

- Scalar numeric fields are expressions.
- Point/vector: `X, Y, Z`.
- Point list: `X,Y,Z; X,Y,Z; ...`.
- Command reference: editor accepts a command name or GUID; JSON stores GUID.
- Reference list: semicolon-separated names in the editor.
- Index list: comma, semicolon or whitespace-separated non-negative integers.
