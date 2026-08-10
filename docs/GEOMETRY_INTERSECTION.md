# Structured Geometry Intersection Design

## Goal

Add CAD-grade geometric intersection to `OcctModelingSession` without reducing intersection to a Boolean answer and without introducing N+1 Native calls.

The first implementation should focus on topology-bounded Edge/Edge intersection. Edge/Face and Curve/Surface can follow using the same result model where their semantics match.

## Why `bool Intersects()` is insufficient

CAD callers usually need the actual intersection evidence:

- point coordinates;
- parameter on both participating curves;
- overlap intervals for coincident edges;
- classification of point versus overlap;
- tolerance used to establish the result.

Two edges can share a point, overlap over an interval, or produce multiple common parts. Returning only true/false loses the information required by snapping, trimming, constraints, measurements and topology reconstruction.

## OCCT mapping

For topology-bounded Edge/Edge intersection, use OCCT `IntTools_EdgeEdge` and its `IntTools_CommonPrt` results rather than intersecting only the underlying infinite curves.

A common part can represent a Vertex-like point result or an Edge-like overlapping interval. The Bridge result must preserve that distinction.

Curve/Surface intersection should be added separately. OCCT curve/surface intersectors may also expose point and segment results; the Bridge must not force all results into a point-only DTO.

## Proposed public values

```text
OcctIntersectionKind
- Point
- Overlap

OcctEdgeIntersection
- Kind
- Point
- FirstParameterStart
- FirstParameterEnd
- SecondParameterStart
- SecondParameterEnd
```

For `Point`, start/end parameters are equal within the intersection tolerance. For `Overlap`, the ranges define the common interval on each Edge.

Do not expose raw OCCT `IntTools_CommonPrt` objects across the C ABI.

## Native ABI

The API should be bulk-oriented from the first version.

Recommended pattern:

```text
occt_model_intersect_edges(...)
  -> computes and stores the current result set in the owning ModelingSession
  -> returns result count or -1 on failure

occt_model_edge_intersections_copy(..., buffer, capacity)
  -> copies the complete result set in one Native call
```

This follows the existing bulk approach used for selected hits, ray hits and operation history. Do not add a managed loop around `intersection_at(index)` as the primary collection path.

A compatibility indexed Native export is unnecessary if this is a new API; the initial ABI can start clean with bulk-only retrieval.

## Native module

Create a dedicated:

`OcctModelingIntersection.cpp`

Do not grow `OcctModelingAnalysis.cpp` again. That module is intentionally limited to projection, ray intersection and point classification.

Session-internal transient storage can use a vector of blittable intersection records, just like ray results. The storage is scoped to the owning `OcctModelingSession` and is not a persistent topology identity.

## Edge/Edge algorithm

1. Validate both handles belong to the same ModelingSession.
2. Require both inputs to be `TopAbs_EDGE`.
3. Run `IntTools_EdgeEdge` with an explicit tolerance/fuzzy value.
4. Iterate all `IntTools_CommonPrt` values.
5. Convert Vertex-like common parts to `Point` records.
6. Convert Edge-like common parts to `Overlap` records, retaining parameter ranges on both input edges.
7. Sort results deterministically by the first Edge parameter range.
8. Deduplicate only when two results are equivalent within the configured tolerance; do not collapse distinct common intervals.
9. Copy all records through one bulk ABI call.

## Parameter semantics

Parameters are the native parameters of the input Edge curves, not normalized 0..1 values. This is important for exact trimming and downstream OCCT operations.

If a convenience normalized form is ever needed, provide it as an additional managed helper rather than changing the exact Native result.

## Tolerance

The caller must be able to provide an explicit non-negative intersection tolerance. The Bridge should not silently hide a large fuzzy value.

The result should be interpreted relative to that tolerance; applications that require stricter engineering checks can choose a smaller policy value.

## Edge/Face follow-up

Edge/Face intersection should preserve topology bounds and return at least:

- point;
- Edge parameter;
- Face `u/v`;
- transition/state information when reliable and useful.

If an Edge lies on a Face over an interval, the API must represent the overlap instead of manufacturing many sample points.

## Curve/Surface follow-up

A lower-level Curve/Surface API may be useful for sketch/constraint geometry that is not represented as topology. Keep it separate from Edge/Face so callers can choose topology-bounded or pure geometric semantics explicitly.

## Application boundary

Bridge supplies geometric facts only. It does not decide that an intersection is a Snap Point, Constraint, Grip, Trim candidate or routing waypoint. Those decisions remain in the consuming CAD application.

## Required tests

Before release, cover at least:

- two lines crossing at one point;
- endpoint touch;
- parallel disjoint edges;
- collinear partially overlapping edges;
- coincident equal edges;
- line/arc intersection;
- tangent contact;
- multiple intersections where supported by the curve pair;
- reversed Edge orientation;
- transformed shapes;
- tolerance just below/above a near-contact case;
- bulk result ordering and zero-result behavior;
- cross-session input rejection at the managed boundary.

## Decision

Implement structured Edge/Edge intersection first in its own Native module, with point/overlap semantics and bulk transfer from day one. Do not add a Boolean-only intersection API and do not reintroduce N+1 indexed result retrieval.
