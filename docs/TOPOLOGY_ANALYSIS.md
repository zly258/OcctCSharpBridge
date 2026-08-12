# Topology Adjacency and Free-Boundary Analysis

OcctCSharpBridge exposes complementary topology-inspection levels for CAD/BIM quality checks: direct local queries, batched edge-to-face adjacency analysis, and strict OCCT free-boundary analysis.

## Local adjacency queries

Use the direct helpers when only one selected subshape needs inspection:

```csharp
var adjacentFaces = model.GetAdjacentFaces(rootShape, edge);
var incidentEdges = model.GetIncidentEdges(rootShape, vertex);
var incidentFaces = model.GetIncidentFaces(rootShape, vertex);
```

These methods are convenient for interactive inspection and isolated queries.

## Batch adjacency analysis

When many edges of the same root shape must be classified, call `AnalyzeEdgeAdjacency()` once:

```csharp
var adjacency = model.AnalyzeEdgeAdjacency(rootShape);

foreach (var entry in adjacency.Entries)
{
    Console.WriteLine($"{entry.Edge.Id}: {entry.AdjacentFaceCount} faces");
}

var boundaryCandidates = adjacency.BoundaryCandidates;
var manifoldEdges = adjacency.ManifoldInteriorEdges;
var nonManifoldEdges = adjacency.NonManifoldEdges;
```

`OcctEdgeAdjacencyInfo` provides:

- `Edge`
- `AdjacentFaceCount`
- `IsIsolated`
- `IsBoundaryCandidate`
- `IsManifoldInterior`
- `IsNonManifold`

`OcctEdgeAdjacencyResult` provides the complete immutable snapshot plus preclassified edge collections and `GetEdgesByAdjacentFaceCount(min,max)`.

The native implementation uses one `TopExp::MapShapesAndUniqueAncestors()` Edge→Face map for the root shape. Distinct ancestor faces are counted once, including seam situations where the same face can reference an edge more than once. The managed buffer protocol performs one count query and one fill call; it does not rebuild the ancestor map for every edge.

The existing convenience methods now reuse this batch path:

```csharp
model.GetBoundaryEdgeCandidates(rootShape);
model.GetManifoldInteriorEdges(rootShape);
model.GetNonManifoldEdges(rootShape);
model.GetEdgesByAdjacentFaceCount(rootShape, minimum, maximum);
```

The classification rules remain:

- 0 adjacent faces → isolated edge;
- 1 distinct adjacent face → boundary candidate;
- 2 distinct adjacent faces → manifold interior edge;
- 3 or more distinct adjacent faces → non-manifold candidate.

### Why the API says “candidate”

An edge with one distinct ancestor face is a useful screening signal, but periodic/seam topology and imported-model peculiarities can make an adjacency count too strong as a final geometric conclusion. For that reason `GetBoundaryEdgeCandidates()` and `BoundaryCandidates` deliberately use candidate terminology.

## Strict free-boundary analysis

Use `AnalyzeFreeBounds()` when the next action depends on OCCT's free-boundary algorithm:

```csharp
var result = model.AnalyzeFreeBounds(
    shape,
    tolerance: 1e-6,
    splitClosed: true,
    splitOpen: true);

foreach (var wire in result.ClosedWires)
{
    // Closed free-boundary loop, for example a hole/opening perimeter.
}

foreach (var wire in result.OpenWires)
{
    // Open free-boundary chain, useful for shell-gap diagnostics.
}
```

`OcctFreeBoundsResult` contains the tolerance, closed/open wire collections, counts, and convenience flags. Returned wires remain owned by the same `OcctModelingSession` as the analyzed shape.

## Tolerance

Free-boundary tolerance is model-unit dependent. The default is `1e-7`, but imported STEP/IGES models often require a value chosen from project/model tolerances rather than one global hard-coded value. `OcctFreeBoundsResult.Tolerance` preserves the actual value used so automated quality results can be traced.

## Typical engineering use

A practical model-quality pipeline can be:

1. Use `GetTopologyCounts()` for a structural summary.
2. Call `AnalyzeEdgeAdjacency()` once for the model or shell being checked.
3. Use `NonManifoldEdges` for obvious topology defects.
4. Use `BoundaryCandidates` for inexpensive opening screening.
5. Use `AnalyzeFreeBounds()` before deciding that a shell has true openings or requires healing/sewing.
6. Use existing validation/healing APIs when repair is required.

For large imported models, prefer retaining one `OcctEdgeAdjacencyResult` when several classifications are needed instead of calling multiple convenience methods separately.

## Native implementation

Topology analysis is isolated in `OcctModelingTopologyAnalysis.h/.cpp`.

Additive ABI 3 functions:

- `occt_model_shape_edge_adjacency` — batched Edge→distinct-Face counts;
- `occt_model_shape_free_bounds` — strict closed/open free-boundary extraction using `ShapeAnalysis_FreeBounds`.

## Verification

Cloud CI checks native declaration/definition/PInvoke/high-level contracts and compiles the Smoke project. Real OCCT execution is covered by:

- `tests/OcctNet.Smoke/EdgeAdjacencySmoke.cs`
- `tests/OcctNet.Smoke/FreeBoundsSmoke.cs`

Run the local native release gate on Windows with OCCT 7.9.0:

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 root>"
```
