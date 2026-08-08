# Shape Mesh Face Provenance

`GetShapeMesh()` combines the triangulations of every Face into one `OcctMesh`. That is convenient for rendering and export, but a plain combined mesh does not say which source Face contributed a node or triangle.

Bridge 2.6 adds `GetShapeMeshData()` to preserve that relationship without changing the existing `OcctMesh` API.

## Build a combined mesh with provenance

```csharp
using var model = new OcctModelingSession();
var shape = model.MakeBox(100, 80, 60);

var data = model.GetShapeMeshData(shape);

Console.WriteLine($"Faces: {data.FaceCount}");
Console.WriteLine($"Nodes: {data.NodeCount}");
Console.WriteLine($"Triangles: {data.TriangleCount}");
```

`OcctShapeMeshData` contains:

- `Mesh` — the normal combined `OcctMesh`;
- `FaceRanges` — one range per source Face;
- `FaceCount`, `NodeCount`, `TriangleCount`;
- `GetFaceRange(faceIndex)`;
- `TryGetFaceForNode()` / `GetFaceForNode()`;
- `TryGetFaceForTriangle()` / `GetFaceForTriangle()`.

Existing code can continue calling:

```csharp
OcctMesh mesh = model.GetShapeMesh(shape);
```

`GetShapeMesh()` now delegates to `GetShapeMeshData(...).Mesh`, so the old API and the provenance API share one composition implementation.

## Face ranges

Each `OcctShapeMeshFaceRange` records a contiguous contribution from one Face:

```csharp
foreach (var range in data.FaceRanges)
{
    Console.WriteLine(
        $"Face {range.Face.Id}: " +
        $"nodes [{range.NodeStart}, {range.NodeEndExclusive}), " +
        $"triangles [{range.TriangleStart}, {range.TriangleEndExclusive})");
}
```

Properties:

- `Face`
- `NodeStart`, `NodeCount`, `NodeEndExclusive`
- `TriangleStart`, `TriangleCount`, `TriangleEndExclusive`
- `ContainsNode(index)`
- `ContainsTriangle(index)`

Ranges are emitted in the same order as the Faces traversed while building the combined mesh. Node and triangle ranges remain contiguous even when a source Face contributes an empty triangulation.

## Picking and BIM property mapping

When a rendering/picking layer returns a combined triangle index:

```csharp
if (data.TryGetFaceForTriangle(hitTriangleIndex, out var sourceFace))
{
    // Resolve BIM/CAD properties, selection state, analysis results, etc.
}
```

The lookup uses the ordered Face ranges rather than storing one Face ID beside every triangle. This keeps provenance memory proportional to the number of Faces rather than the number of triangles.

Node lookup is available for workflows that return a combined node index:

```csharp
var sourceFace = data.GetFaceForNode(nodeIndex);
```

## Index semantics

Combined triangle node indices still refer to `data.Mesh.Nodes` exactly as they do in the original `GetShapeMesh()` result. The provenance layer does not renumber them again.

For one Face:

```text
combinedNodeIndex = localNodeIndex + range.NodeStart
combinedTriangleIndex = localTriangleIndex + range.TriangleStart
```

Each copied triangle has its three node indices offset by that Face's `NodeStart`.

## Ownership

`OcctShapeMeshFaceRange.Face` is a normal `OcctModelShape` owned by the same `OcctModelingSession` that created the mesh data. Normal cross-session ownership rules still apply.

The returned `OcctShapeMeshData` is a snapshot. Re-triangulating or clearing triangulation later does not rewrite its managed node/triangle/range collections.

## Performance

No new Native ABI call is required. `GetShapeMeshData()`:

1. triangulates the root Shape once;
2. enumerates source Faces;
3. reads each Face mesh using the existing Face mesh ABI;
4. appends nodes/triangles into the combined mesh;
5. records one compact provenance range per Face.

If only rendering geometry is needed, `GetShapeMesh()` remains the simplest call. If the application needs picking, Face-aware analysis, BIM-property mapping, selective export, or per-Face diagnostics, prefer `GetShapeMeshData()`.

## Verification

`tests/OcctNet.Smoke/ShapeMeshProvenanceSmoke.cs` validates a Box with six source Faces, contiguous ranges, full mesh coverage, node/triangle reverse lookup, and invalid-index behavior.

Cloud CI compiles the Smoke project. Real OCCT execution remains part of the local native release gate:

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 root>"
```
