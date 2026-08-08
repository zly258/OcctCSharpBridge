# Topology Adjacency and Free-Boundary Analysis

OcctCSharpBridge exposes two complementary levels of topology inspection for CAD/BIM quality checks: inexpensive adjacency-based screening and stricter OCCT free-boundary analysis.

## Fast adjacency screening

The convenience API composes the existing subshape/ancestor primitives without adding a native ABI call:

```csharp
var adjacentFaces = model.GetAdjacentFaces(rootShape, edge);
var incidentEdges = model.GetIncidentEdges(rootShape, vertex);
var incidentFaces = model.GetIncidentFaces(rootShape, vertex);

var boundaryCandidates = model.GetBoundaryEdgeCandidates(rootShape);
var manifoldEdges = model.GetManifoldInteriorEdges(rootShape);
var nonManifoldEdges = model.GetNonManifoldEdges(rootShape);
```

The edge classifiers are based on the number of ancestor faces in the supplied root shape:

- 1 adjacent face → boundary candidate;
- 2 adjacent faces → manifold interior edge;
- 3 or more adjacent faces → non-manifold candidate.

`GetEdgesByAdjacentFaceCount()` is available when a custom range is needed.

### Why the API says “candidate”

An edge with one ancestor face is a useful screening signal, but periodic/seam topology and imported-model peculiarities can make a simple adjacency count too strong as a final geometric conclusion. For that reason `GetBoundaryEdgeCandidates()` is intentionally not named `GetBoundaryEdges()`.

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

`OcctFreeBoundsResult` contains:

- `Tolerance`
- `ClosedWires`
- `OpenWires`
- `ClosedWireCount`
- `OpenWireCount`
- `TotalWireCount`
- `HasFreeBounds`
- `HasOpenFreeBounds`

The returned wires remain owned by the same `OcctModelingSession` as the analyzed shape.

## Tolerance

Tolerance is model-unit dependent. The default is `1e-7`, matching the bridge's fine geometric-analysis convention, but imported STEP/IGES models often require a value chosen from project/model tolerances rather than a hard-coded global value.

Always record the tolerance when free-boundary results are used for automated acceptance/rejection. `OcctFreeBoundsResult.Tolerance` preserves the actual value used for that reason.

## Typical engineering use

A practical model-quality pipeline can be:

1. Use `GetTopologyCounts()` for a fast structural summary.
2. Use `GetNonManifoldEdges()` to find obvious topology defects.
3. Use `GetBoundaryEdgeCandidates()` for inexpensive screening.
4. Use `AnalyzeFreeBounds()` before deciding that a shell has openings or requires healing/sewing.
5. Use existing validation/healing APIs when repair is required.

For large imported models, adjacency convenience methods currently favor API clarity over minimum native-call count. A future batch adjacency API can optimize that path without changing these high-level semantics.

## Native implementation

Strict analysis is isolated in `OcctModelingTopologyAnalysis.h/.cpp` and uses OCCT `ShapeAnalysis_FreeBounds`. One additive ABI 3 function returns either the closed-wire or open-wire compound selected by `boundaryKind`:

- `occt_model_shape_free_bounds`

The high-level method invokes the same ABI for both categories and returns managed wire collections rather than exposing the temporary compounds.

## Verification

Cloud CI checks the native declaration/definition/PInvoke/high-level contract and compiles the Smoke project. Real OCCT execution is covered by `tests/OcctNet.Smoke/FreeBoundsSmoke.cs` and requires the local native release gate:

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 root>"
```
