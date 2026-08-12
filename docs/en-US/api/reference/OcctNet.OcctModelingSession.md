# OcctModelingSession

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Declaration

```csharp
public sealed class OcctModelingSession
```

## Description

Headless OCCT modeling session. No HWND, AIS context, or viewer is required.

## Constructors

### `OcctModelingSession`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelingSession()
```

## Properties

### `Capabilities`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public string Capabilities { get; }
```

### `IsDisposed`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsDisposed { get; }
```

### `ShapeCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int ShapeCount { get; }
```

### `Shapes`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> Shapes { get; }
```

## Events

None

## Methods

### `AnalyzeEdgeAdjacency`

Builds one native edge-to-face topology map and returns the adjacency count for every edge. Use this snapshot when several edge classifications are required for the same root shape.

```csharp
public OcctEdgeAdjacencyResult AnalyzeEdgeAdjacency(OcctModelShape root)
```

**Parameters**

- `root` — `OcctModelShape`

**Returns:** `OcctEdgeAdjacencyResult`

### `AnalyzeFaces`

Analyzes all faces in one native batch call and returns a stable managed snapshot.

```csharp
public OcctFaceAnalysisResult AnalyzeFaces(OcctModelShape root)
```

**Parameters**

- `root` — `OcctModelShape`

**Returns:** `OcctFaceAnalysisResult`

### `AnalyzeFreeBounds`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctFreeBoundsResult AnalyzeFreeBounds(OcctModelShape shape, double tolerance = 1E-07, bool splitClosed = true, bool splitOpen = true)
```

**Parameters**

- `shape` — `OcctModelShape`
- `tolerance` — `double` = 1E-07
- `splitClosed` — `bool` = true
- `splitOpen` — `bool` = true

**Returns:** `OcctFreeBoundsResult`

### `Boolean`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelAlgorithmResult Boolean(OcctBooleanOperation operation, OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options = null)
```

**Parameters**

- `operation` — `OcctBooleanOperation`
- `left` — `OcctModelShape`
- `right` — `OcctModelShape`
- `options` — `OcctModelBooleanOptions?` = null

**Returns:** `OcctModelAlgorithmResult`

### `ChamferEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelAlgorithmResult ChamferEdges(OcctModelShape shape, IEnumerable<int> edgeIndices, double distance)
```

**Parameters**

- `shape` — `OcctModelShape`
- `edgeIndices` — `IEnumerable<int>`
- `distance` — `double`

**Returns:** `OcctModelAlgorithmResult`

### `ClassifyPoint`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelState ClassifyPoint(OcctModelShape solid, OcctPoint3d point, double tolerance = 1E-07)
```

**Parameters**

- `solid` — `OcctModelShape`
- `point` — `OcctPoint3d`
- `tolerance` — `double` = 1E-07

**Returns:** `OcctModelState`

### `Clear`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Clear()
```

**Returns:** `void`

### `ClearTriangulation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ClearTriangulation(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `void`

### `Common`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelAlgorithmResult Common(OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options = null)
```

**Parameters**

- `left` — `OcctModelShape`
- `right` — `OcctModelShape`
- `options` — `OcctModelBooleanOptions?` = null

**Returns:** `OcctModelAlgorithmResult`

### `Copy`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape Copy(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `OcctModelShape`

### `CreateTopologyReference`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctTopologyReference CreateTopologyReference(OcctModelShape root, OcctModelShape subshape)
```

**Parameters**

- `root` — `OcctModelShape`
- `subshape` — `OcctModelShape`

**Returns:** `OcctTopologyReference`

### `Cut`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelAlgorithmResult Cut(OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options = null)
```

**Parameters**

- `left` — `OcctModelShape`
- `right` — `OcctModelShape`
- `options` — `OcctModelBooleanOptions?` = null

**Returns:** `OcctModelAlgorithmResult`

### `Delete`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Delete(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `void`

### `Dispose`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Dispose()
```

**Returns:** `void`

### `EvaluateEdge`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctEdgeEvaluation EvaluateEdge(OcctModelShape edge, double normalizedParameter)
```

**Parameters**

- `edge` — `OcctModelShape`
- `normalizedParameter` — `double`

**Returns:** `OcctEdgeEvaluation`

### `EvaluateEdgeAtParameter`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelCurveDifferential EvaluateEdgeAtParameter(OcctModelShape edge, double parameter)
```

**Parameters**

- `edge` — `OcctModelShape`
- `parameter` — `double`

**Returns:** `OcctModelCurveDifferential`

### `EvaluateFace`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctFaceEvaluation EvaluateFace(OcctModelShape face, double u, double v)
```

**Parameters**

- `face` — `OcctModelShape`
- `u` — `double`
- `v` — `double`

**Returns:** `OcctFaceEvaluation`

### `EvaluateFaceDifferential`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelSurfaceDifferential EvaluateFaceDifferential(OcctModelShape face, double u, double v, double resolution = 1E-09)
```

**Parameters**

- `face` — `OcctModelShape`
- `u` — `double`
- `v` — `double`
- `resolution` — `double` = 1E-09

**Returns:** `OcctModelSurfaceDifferential`

### `Exists`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Exists(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `bool`

### `ExportBrep`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ExportBrep(OcctModelShape shape, string filePath)
```

**Parameters**

- `shape` — `OcctModelShape`
- `filePath` — `string`

**Returns:** `void`

### `ExportIges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ExportIges(OcctModelShape shape, string filePath)
```

**Parameters**

- `shape` — `OcctModelShape`
- `filePath` — `string`

**Returns:** `void`

### `ExportStep`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ExportStep(OcctModelShape shape, string filePath)
```

**Parameters**

- `shape` — `OcctModelShape`
- `filePath` — `string`

**Returns:** `void`

### `ExportStl`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ExportStl(OcctModelShape shape, string filePath, double linearDeflection = 0.1, double angularDeflection = 0.5, bool ascii = false)
```

**Parameters**

- `shape` — `OcctModelShape`
- `filePath` — `string`
- `linearDeflection` — `double` = 0.1
- `angularDeflection` — `double` = 0.5
- `ascii` — `bool` = false

**Returns:** `void`

### `Extrude`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelAlgorithmResult Extrude(OcctModelShape profile, OcctVector3d vector)
```

**Parameters**

- `profile` — `OcctModelShape`
- `vector` — `OcctVector3d`

**Returns:** `OcctModelAlgorithmResult`

### `FilletEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelAlgorithmResult FilletEdges(OcctModelShape shape, IEnumerable<int> edgeIndices, double radius)
```

**Parameters**

- `shape` — `OcctModelShape`
- `edgeIndices` — `IEnumerable<int>`
- `radius` — `double`

**Returns:** `OcctModelAlgorithmResult`

### `FixShape`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelAlgorithmResult FixShape(OcctModelShape shape, double precision = 1E-07, double minTolerance = 1E-07, double maxTolerance = 1)
```

**Parameters**

- `shape` — `OcctModelShape`
- `precision` — `double` = 1E-07
- `minTolerance` — `double` = 1E-07
- `maxTolerance` — `double` = 1

**Returns:** `OcctModelAlgorithmResult`

### `Fuse`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelAlgorithmResult Fuse(OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options = null)
```

**Parameters**

- `left` — `OcctModelShape`
- `right` — `OcctModelShape`
- `options` — `OcctModelBooleanOptions?` = null

**Returns:** `OcctModelAlgorithmResult`

### `GetAdjacentFaces`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetAdjacentFaces(OcctModelShape root, OcctModelShape edge)
```

**Parameters**

- `root` — `OcctModelShape`
- `edge` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetAncestors`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetAncestors(OcctModelShape root, OcctModelShape child, OcctShapeType ancestorType)
```

**Parameters**

- `root` — `OcctModelShape`
- `child` — `OcctModelShape`
- `ancestorType` — `OcctShapeType`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetBSplineCurveData`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctBSplineCurveData GetBSplineCurveData(OcctModelShape edge)
```

**Parameters**

- `edge` — `OcctModelShape`

**Returns:** `OcctBSplineCurveData`

### `GetBSplineSurfaceData`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctBSplineSurfaceData GetBSplineSurfaceData(OcctModelShape face)
```

**Parameters**

- `face` — `OcctModelShape`

**Returns:** `OcctBSplineSurfaceData`

### `GetBoundaryEdgeCandidates`

Returns edges that are referenced by exactly one distinct face in root. These are useful free-boundary candidates, but periodic seam topology should be checked before treating every returned edge as an open geometric boundary.

```csharp
public IReadOnlyList<OcctModelShape> GetBoundaryEdgeCandidates(OcctModelShape root)
```

**Parameters**

- `root` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetCircleGeometry`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctCircleGeometry GetCircleGeometry(OcctModelShape edge)
```

**Parameters**

- `edge` — `OcctModelShape`

**Returns:** `OcctCircleGeometry`

### `GetCompSolids`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetCompSolids(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetCompounds`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetCompounds(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetConeGeometry`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctConeGeometry GetConeGeometry(OcctModelShape face)
```

**Parameters**

- `face` — `OcctModelShape`

**Returns:** `OcctConeGeometry`

### `GetCylinderGeometry`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctCylinderGeometry GetCylinderGeometry(OcctModelShape face)
```

**Parameters**

- `face` — `OcctModelShape`

**Returns:** `OcctCylinderGeometry`

### `GetEdgeCurvature`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelCurveCurvature GetEdgeCurvature(OcctModelShape edge, double parameter, double resolution = 1E-09)
```

**Parameters**

- `edge` — `OcctModelShape`
- `parameter` — `double`
- `resolution` — `double` = 1E-09

**Returns:** `OcctModelCurveCurvature`

### `GetEdgeCurveType`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctCurveType GetEdgeCurveType(OcctModelShape edge)
```

**Parameters**

- `edge` — `OcctModelShape`

**Returns:** `OcctCurveType`

### `GetEdgeEndpoints`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public ValueTuple<OcctPoint3d, OcctPoint3d> GetEdgeEndpoints(OcctModelShape edge)
```

**Parameters**

- `edge` — `OcctModelShape`

**Returns:** `ValueTuple<OcctPoint3d, OcctPoint3d>`

### `GetEdgeParameterRange`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelParameterRange GetEdgeParameterRange(OcctModelShape edge)
```

**Parameters**

- `edge` — `OcctModelShape`

**Returns:** `OcctModelParameterRange`

### `GetEdgeVertices`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetEdgeVertices(OcctModelShape edge)
```

**Parameters**

- `edge` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetEdges(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetEdgesByAdjacentFaceCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetEdgesByAdjacentFaceCount(OcctModelShape root, int minimumFaceCount, int maximumFaceCount)
```

**Parameters**

- `root` — `OcctModelShape`
- `minimumFaceCount` — `int`
- `maximumFaceCount` — `int`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetEllipseGeometry`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctEllipseGeometry GetEllipseGeometry(OcctModelShape edge)
```

**Parameters**

- `edge` — `OcctModelShape`

**Returns:** `OcctEllipseGeometry`

### `GetFaceCurvature`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelSurfaceCurvature GetFaceCurvature(OcctModelShape face, double u, double v, double resolution = 1E-09)
```

**Parameters**

- `face` — `OcctModelShape`
- `u` — `double`
- `v` — `double`
- `resolution` — `double` = 1E-09

**Returns:** `OcctModelSurfaceCurvature`

### `GetFaceEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetFaceEdges(OcctModelShape face)
```

**Parameters**

- `face` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetFaceMesh`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctMesh GetFaceMesh(OcctModelShape face)
```

**Parameters**

- `face` — `OcctModelShape`

**Returns:** `OcctMesh`

### `GetFacePeriodicity`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelSurfacePeriodicity GetFacePeriodicity(OcctModelShape face)
```

**Parameters**

- `face` — `OcctModelShape`

**Returns:** `OcctModelSurfacePeriodicity`

### `GetFaceSurfaceType`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctSurfaceType GetFaceSurfaceType(OcctModelShape face)
```

**Parameters**

- `face` — `OcctModelShape`

**Returns:** `OcctSurfaceType`

### `GetFaceUvBounds`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctUvBounds GetFaceUvBounds(OcctModelShape face)
```

**Parameters**

- `face` — `OcctModelShape`

**Returns:** `OcctUvBounds`

### `GetFaceVertices`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetFaceVertices(OcctModelShape face)
```

**Parameters**

- `face` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetFaces`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetFaces(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetGeneratedShapes`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetGeneratedShapes(long operationId, OcctModelShape source)
```

**Parameters**

- `operationId` — `long`
- `source` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetIncidentEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetIncidentEdges(OcctModelShape root, OcctModelShape vertex)
```

**Parameters**

- `root` — `OcctModelShape`
- `vertex` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetIncidentFaces`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetIncidentFaces(OcctModelShape root, OcctModelShape vertex)
```

**Parameters**

- `root` — `OcctModelShape`
- `vertex` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetInnerWires`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetInnerWires(OcctModelShape face)
```

**Parameters**

- `face` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetLineGeometry`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctLineGeometry GetLineGeometry(OcctModelShape edge)
```

**Parameters**

- `edge` — `OcctModelShape`

**Returns:** `OcctLineGeometry`

### `GetLinearInertiaProperties`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctInertiaProperties GetLinearInertiaProperties(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `OcctInertiaProperties`

### `GetManifoldInteriorEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetManifoldInteriorEdges(OcctModelShape root)
```

**Parameters**

- `root` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetModifiedShapes`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetModifiedShapes(long operationId, OcctModelShape source)
```

**Parameters**

- `operationId` — `long`
- `source` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetNonManifoldEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetNonManifoldEdges(OcctModelShape root)
```

**Parameters**

- `root` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetOperationReport`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public string GetOperationReport(long operationId)
```

**Parameters**

- `operationId` — `long`

**Returns:** `string`

### `GetOuterWire`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape GetOuterWire(OcctModelShape face)
```

**Parameters**

- `face` — `OcctModelShape`

**Returns:** `OcctModelShape`

### `GetPlaneGeometry`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPlaneGeometry GetPlaneGeometry(OcctModelShape face)
```

**Parameters**

- `face` — `OcctModelShape`

**Returns:** `OcctPlaneGeometry`

### `GetShape`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape GetShape(long id)
```

**Parameters**

- `id` — `long`

**Returns:** `OcctModelShape`

### `GetShapeBounds`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctBounds GetShapeBounds(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `OcctBounds`

### `GetShapeCheckReport`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public string GetShapeCheckReport(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `string`

### `GetShapeDistance`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctDistanceResult GetShapeDistance(OcctModelShape first, OcctModelShape second)
```

**Parameters**

- `first` — `OcctModelShape`
- `second` — `OcctModelShape`

**Returns:** `OcctDistanceResult`

### `GetShapeHash`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public long GetShapeHash(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `long`

### `GetShapeLinearProperties`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctMassProperties GetShapeLinearProperties(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `OcctMassProperties`

### `GetShapeLocation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelLocation GetShapeLocation(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `OcctModelLocation`

### `GetShapeMaximumTolerance`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double GetShapeMaximumTolerance(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `double`

### `GetShapeMesh`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctMesh GetShapeMesh(OcctModelShape shape, OcctModelMeshParameters? parameters = null)
```

**Parameters**

- `shape` — `OcctModelShape`
- `parameters` — `OcctModelMeshParameters?` = null

**Returns:** `OcctMesh`

### `GetShapeMeshData`

Builds one combined mesh while preserving the contiguous node and triangle ranges contributed by every source face.

```csharp
public OcctShapeMeshData GetShapeMeshData(OcctModelShape shape, OcctModelMeshParameters? parameters = null)
```

**Parameters**

- `shape` — `OcctModelShape`
- `parameters` — `OcctModelMeshParameters?` = null

**Returns:** `OcctShapeMeshData`

### `GetShapeOrientation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelOrientation GetShapeOrientation(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `OcctModelOrientation`

### `GetShapeOrientedBounds`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctOrientedBounds GetShapeOrientedBounds(OcctModelShape shape, bool optimal = false)
```

**Parameters**

- `shape` — `OcctModelShape`
- `optimal` — `bool` = false

**Returns:** `OcctOrientedBounds`

### `GetShapeSurfaceProperties`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctMassProperties GetShapeSurfaceProperties(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `OcctMassProperties`

### `GetShapeType`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShapeType GetShapeType(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `OcctShapeType`

### `GetShapeVolumeProperties`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctMassProperties GetShapeVolumeProperties(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `OcctMassProperties`

### `GetShells`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetShells(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetSolids`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetSolids(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetSphereGeometry`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctSphereGeometry GetSphereGeometry(OcctModelShape face)
```

**Parameters**

- `face` — `OcctModelShape`

**Returns:** `OcctSphereGeometry`

### `GetSubshapes`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetSubshapes(OcctModelShape shape, OcctShapeType type)
```

**Parameters**

- `shape` — `OcctModelShape`
- `type` — `OcctShapeType`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetSurfaceInertiaProperties`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctInertiaProperties GetSurfaceInertiaProperties(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `OcctInertiaProperties`

### `GetTopologyCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int GetTopologyCount(OcctModelShape shape, OcctShapeType type)
```

**Parameters**

- `shape` — `OcctModelShape`
- `type` — `OcctShapeType`

**Returns:** `int`

### `GetTopologyCounts`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyDictionary<OcctShapeType, int> GetTopologyCounts(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `IReadOnlyDictionary<OcctShapeType, int>`

### `GetTorusGeometry`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctTorusGeometry GetTorusGeometry(OcctModelShape face)
```

**Parameters**

- `face` — `OcctModelShape`

**Returns:** `OcctTorusGeometry`

### `GetVertexPoint`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d GetVertexPoint(OcctModelShape vertex)
```

**Parameters**

- `vertex` — `OcctModelShape`

**Returns:** `OcctPoint3d`

### `GetVertices`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetVertices(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetVolumeInertiaProperties`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctInertiaProperties GetVolumeInertiaProperties(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `OcctInertiaProperties`

### `GetWireEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetWireEdges(OcctModelShape wire)
```

**Parameters**

- `wire` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `GetWires`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> GetWires(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `IReadOnlyList<OcctModelShape>`

### `Import`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape Import(string filePath)
```

**Parameters**

- `filePath` — `string`

**Returns:** `OcctModelShape`

### `ImportBrep`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape ImportBrep(string filePath)
```

**Parameters**

- `filePath` — `string`

**Returns:** `OcctModelShape`

### `ImportIges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape ImportIges(string filePath)
```

**Parameters**

- `filePath` — `string`

**Returns:** `OcctModelShape`

### `ImportStep`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape ImportStep(string filePath)
```

**Parameters**

- `filePath` — `string`

**Returns:** `OcctModelShape`

### `ImportStl`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape ImportStl(string filePath)
```

**Parameters**

- `filePath` — `string`

**Returns:** `OcctModelShape`

### `InspectShape`

Builds a structured inspection snapshot without making application-specific pass/fail decisions. Mesh statistics are generated only when explicitly requested.

```csharp
public OcctShapeInspectionReport InspectShape(OcctModelShape shape, OcctShapeInspectionOptions? options = null)
```

**Parameters**

- `shape` — `OcctModelShape`
- `options` — `OcctShapeInspectionOptions?` = null

**Returns:** `OcctShapeInspectionReport`

### `IntersectEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctEdgeIntersection> IntersectEdges(OcctModelShape firstEdge, OcctModelShape secondEdge, double tolerance = 1E-07)
```

**Parameters**

- `firstEdge` — `OcctModelShape`
- `secondEdge` — `OcctModelShape`
- `tolerance` — `double` = 1E-07

**Returns:** `IReadOnlyList<OcctEdgeIntersection>`

### `IntersectRay`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelRayHit> IntersectRay(OcctModelShape shape, OcctPoint3d origin, OcctVector3d direction, double minimumParameter = 0, double maximumParameter = 1000000000000, double tolerance = 1E-07)
```

**Parameters**

- `shape` — `OcctModelShape`
- `origin` — `OcctPoint3d`
- `direction` — `OcctVector3d`
- `minimumParameter` — `double` = 0
- `maximumParameter` — `double` = 1000000000000
- `tolerance` — `double` = 1E-07

**Returns:** `IReadOnlyList<OcctModelRayHit>`

### `IsPartnerShape`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsPartnerShape(OcctModelShape first, OcctModelShape second)
```

**Parameters**

- `first` — `OcctModelShape`
- `second` — `OcctModelShape`

**Returns:** `bool`

### `IsRemoved`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsRemoved(long operationId, OcctModelShape source)
```

**Parameters**

- `operationId` — `long`
- `source` — `OcctModelShape`

**Returns:** `bool`

### `IsSameShape`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsSameShape(OcctModelShape first, OcctModelShape second)
```

**Parameters**

- `first` — `OcctModelShape`
- `second` — `OcctModelShape`

**Returns:** `bool`

### `IsShapeClosed`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsShapeClosed(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `bool`

### `IsShapeValid`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsShapeValid(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `bool`

### `Loft`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelAlgorithmResult Loft(IEnumerable<OcctModelShape> sectionWires, bool makeSolid = true, bool ruled = false, double tolerance = 1E-06)
```

**Parameters**

- `sectionWires` — `IEnumerable<OcctModelShape>`
- `makeSolid` — `bool` = true
- `ruled` — `bool` = false
- `tolerance` — `double` = 1E-06

**Returns:** `OcctModelAlgorithmResult`

### `MakeArc`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeArc(OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees)
```

**Parameters**

- `center` — `OcctPoint3d`
- `normal` — `OcctVector3d`
- `xDirection` — `OcctVector3d`
- `radius` — `double`
- `startAngleDegrees` — `double`
- `endAngleDegrees` — `double`

**Returns:** `OcctModelShape`

### `MakeArc`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeArc(OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end)
```

**Parameters**

- `start` — `OcctPoint3d`
- `middle` — `OcctPoint3d`
- `end` — `OcctPoint3d`

**Returns:** `OcctModelShape`

### `MakeBezier`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeBezier(IEnumerable<OcctPoint3d> poles)
```

**Parameters**

- `poles` — `IEnumerable<OcctPoint3d>`

**Returns:** `OcctModelShape`

### `MakeBox`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeBox(double dx, double dy, double dz, double x = 0, double y = 0, double z = 0)
```

**Parameters**

- `dx` — `double`
- `dy` — `double`
- `dz` — `double`
- `x` — `double` = 0
- `y` — `double` = 0
- `z` — `double` = 0

**Returns:** `OcctModelShape`

### `MakeCircle`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeCircle(OcctPoint3d center, OcctVector3d normal, double radius)
```

**Parameters**

- `center` — `OcctPoint3d`
- `normal` — `OcctVector3d`
- `radius` — `double`

**Returns:** `OcctModelShape`

### `MakeCompound`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeCompound(IEnumerable<OcctModelShape> shapes)
```

**Parameters**

- `shapes` — `IEnumerable<OcctModelShape>`

**Returns:** `OcctModelShape`

### `MakeCone`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeCone(OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height)
```

**Parameters**

- `origin` — `OcctPoint3d`
- `axis` — `OcctVector3d`
- `radius1` — `double`
- `radius2` — `double`
- `height` — `double`

**Returns:** `OcctModelShape`

### `MakeCylinder`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeCylinder(OcctPoint3d origin, OcctVector3d axis, double radius, double height)
```

**Parameters**

- `origin` — `OcctPoint3d`
- `axis` — `OcctVector3d`
- `radius` — `double`
- `height` — `double`

**Returns:** `OcctModelShape`

### `MakeEllipse`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeEllipse(OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius)
```

**Parameters**

- `center` — `OcctPoint3d`
- `normal` — `OcctVector3d`
- `majorRadius` — `double`
- `minorRadius` — `double`

**Returns:** `OcctModelShape`

### `MakeFace`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeFace(OcctModelShape wire)
```

**Parameters**

- `wire` — `OcctModelShape`

**Returns:** `OcctModelShape`

### `MakeInterpolatedBSpline`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeInterpolatedBSpline(IEnumerable<OcctPoint3d> points, bool periodic = false, double tolerance = 1E-07)
```

**Parameters**

- `points` — `IEnumerable<OcctPoint3d>`
- `periodic` — `bool` = false
- `tolerance` — `double` = 1E-07

**Returns:** `OcctModelShape`

### `MakeLine`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeLine(OcctPoint3d start, OcctPoint3d end)
```

**Parameters**

- `start` — `OcctPoint3d`
- `end` — `OcctPoint3d`

**Returns:** `OcctModelShape`

### `MakePlanarFace`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakePlanarFace(OcctModelShape outerWire, IEnumerable<OcctModelShape> innerWires = null)
```

**Parameters**

- `outerWire` — `OcctModelShape`
- `innerWires` — `IEnumerable<OcctModelShape>` = null

**Returns:** `OcctModelShape`

### `MakePlaneFace`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakePlaneFace(double width, double height, OcctPoint3d? origin = null, OcctVector3d? xDirection = null, OcctVector3d? normal = null)
```

**Parameters**

- `width` — `double`
- `height` — `double`
- `origin` — `OcctPoint3d?` = null
- `xDirection` — `OcctVector3d?` = null
- `normal` — `OcctVector3d?` = null

**Returns:** `OcctModelShape`

### `MakePolyline`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakePolyline(IEnumerable<OcctPoint3d> points, bool closed = false)
```

**Parameters**

- `points` — `IEnumerable<OcctPoint3d>`
- `closed` — `bool` = false

**Returns:** `OcctModelShape`

### `MakeRectangleWire`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeRectangleWire(double width, double height, OcctPoint3d? origin = null, OcctVector3d? xDirection = null, OcctVector3d? normal = null)
```

**Parameters**

- `width` — `double`
- `height` — `double`
- `origin` — `OcctPoint3d?` = null
- `xDirection` — `OcctVector3d?` = null
- `normal` — `OcctVector3d?` = null

**Returns:** `OcctModelShape`

### `MakeRegularPolygon`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeRegularPolygon(double radius, int sideCount, bool makeFace = false, OcctPoint3d? center = null, OcctVector3d? normal = null, OcctVector3d? xDirection = null)
```

**Parameters**

- `radius` — `double`
- `sideCount` — `int`
- `makeFace` — `bool` = false
- `center` — `OcctPoint3d?` = null
- `normal` — `OcctVector3d?` = null
- `xDirection` — `OcctVector3d?` = null

**Returns:** `OcctModelShape`

### `MakeSolidFromShell`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeSolidFromShell(OcctModelShape shell)
```

**Parameters**

- `shell` — `OcctModelShape`

**Returns:** `OcctModelShape`

### `MakeSphere`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeSphere(OcctPoint3d center, double radius)
```

**Parameters**

- `center` — `OcctPoint3d`
- `radius` — `double`

**Returns:** `OcctModelShape`

### `MakeThickSolid`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelAlgorithmResult MakeThickSolid(OcctModelShape solid, IEnumerable<int> faceIndicesToRemove, double thickness, double tolerance = 0.0001)
```

**Parameters**

- `solid` — `OcctModelShape`
- `faceIndicesToRemove` — `IEnumerable<int>`
- `thickness` — `double`
- `tolerance` — `double` = 0.0001

**Returns:** `OcctModelAlgorithmResult`

### `MakeTorus`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeTorus(OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius)
```

**Parameters**

- `center` — `OcctPoint3d`
- `axis` — `OcctVector3d`
- `majorRadius` — `double`
- `minorRadius` — `double`

**Returns:** `OcctModelShape`

### `MakeVertex`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeVertex(OcctPoint3d point)
```

**Parameters**

- `point` — `OcctPoint3d`

**Returns:** `OcctModelShape`

### `MakeWedge`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeWedge(double dx, double dy, double dz, double ltx)
```

**Parameters**

- `dx` — `double`
- `dy` — `double`
- `dz` — `double`
- `ltx` — `double`

**Returns:** `OcctModelShape`

### `MakeWire`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MakeWire(IEnumerable<OcctModelShape> edges)
```

**Parameters**

- `edges` — `IEnumerable<OcctModelShape>`

**Returns:** `OcctModelShape`

### `MirrorPlane`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape MirrorPlane(OcctModelShape shape, OcctPoint3d planePoint, OcctVector3d planeNormal)
```

**Parameters**

- `shape` — `OcctModelShape`
- `planePoint` — `OcctPoint3d`
- `planeNormal` — `OcctVector3d`

**Returns:** `OcctModelShape`

### `OffsetShape`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelAlgorithmResult OffsetShape(OcctModelShape shape, double offset, double tolerance = 0.0001)
```

**Parameters**

- `shape` — `OcctModelShape`
- `offset` — `double`
- `tolerance` — `double` = 0.0001

**Returns:** `OcctModelAlgorithmResult`

### `OffsetWire`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape OffsetWire(OcctModelShape wire, double offset, double altitude = 0, OcctJoinType joinType = OcctJoinType.Arc, bool openResult = false)
```

**Parameters**

- `wire` — `OcctModelShape`
- `offset` — `double`
- `altitude` — `double` = 0
- `joinType` — `OcctJoinType` = OcctJoinType.Arc
- `openResult` — `bool` = false

**Returns:** `OcctModelShape`

### `Owns`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Owns(OcctModelShape shape)
```

**Parameters**

- `shape` — `OcctModelShape`

**Returns:** `bool`

### `ProjectPointOnEdge`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelProjectionResult ProjectPointOnEdge(OcctModelShape edge, OcctPoint3d point)
```

**Parameters**

- `edge` — `OcctModelShape`
- `point` — `OcctPoint3d`

**Returns:** `OcctModelProjectionResult`

### `ProjectPointOnFace`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelProjectionResult ProjectPointOnFace(OcctModelShape face, OcctPoint3d point)
```

**Parameters**

- `face` — `OcctModelShape`
- `point` — `OcctPoint3d`

**Returns:** `OcctModelProjectionResult`

### `ResolveTopologyReference`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctTopologyReferenceResult ResolveTopologyReference(OcctModelShape root, OcctTopologyReference reference, double matchingTolerance = 1E-06)
```

**Parameters**

- `root` — `OcctModelShape`
- `reference` — `OcctTopologyReference`
- `matchingTolerance` — `double` = 1E-06

**Returns:** `OcctTopologyReferenceResult`

### `ResolveTopologyReference`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctTopologyReferenceResult ResolveTopologyReference(OcctModelShape root, OcctTopologyReference reference, long operationId, OcctModelShape sourceShape, double matchingTolerance = 1E-06)
```

**Parameters**

- `root` — `OcctModelShape`
- `reference` — `OcctTopologyReference`
- `operationId` — `long`
- `sourceShape` — `OcctModelShape`
- `matchingTolerance` — `double` = 1E-06

**Returns:** `OcctTopologyReferenceResult`

### `Revolve`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelAlgorithmResult Revolve(OcctModelShape profile, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees = 360)
```

**Parameters**

- `profile` — `OcctModelShape`
- `axisPoint` — `OcctPoint3d`
- `axisDirection` — `OcctVector3d`
- `angleDegrees` — `double` = 360

**Returns:** `OcctModelAlgorithmResult`

### `Rotate`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape Rotate(OcctModelShape shape, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees)
```

**Parameters**

- `shape` — `OcctModelShape`
- `axisPoint` — `OcctPoint3d`
- `axisDirection` — `OcctVector3d`
- `angleDegrees` — `double`

**Returns:** `OcctModelShape`

### `Scale`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape Scale(OcctModelShape shape, OcctPoint3d center, double factor)
```

**Parameters**

- `shape` — `OcctModelShape`
- `center` — `OcctPoint3d`
- `factor` — `double`

**Returns:** `OcctModelShape`

### `Section`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelAlgorithmResult Section(OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options = null)
```

**Parameters**

- `left` — `OcctModelShape`
- `right` — `OcctModelShape`
- `options` — `OcctModelBooleanOptions?` = null

**Returns:** `OcctModelAlgorithmResult`

### `SetLocation`

Bridge 2.5 source-compatibility entry point. New code should use OcctNet.OcctModelingSession.SetShapeLocation(OcctNet.OcctModelShape,OcctNet.OcctModelLocation,System.Boolean).

```csharp
public OcctModelShape SetLocation(OcctModelShape shape, OcctModelLocation location, bool copyShape = true)
```

**Parameters**

- `shape` — `OcctModelShape`
- `location` — `OcctModelLocation`
- `copyShape` — `bool` = true

**Returns:** `OcctModelShape`

### `SetShapeLocation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape SetShapeLocation(OcctModelShape shape, OcctModelLocation location, bool copyShape = true)
```

**Parameters**

- `shape` — `OcctModelShape`
- `location` — `OcctModelLocation`
- `copyShape` — `bool` = true

**Returns:** `OcctModelShape`

### `Sew`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape Sew(IEnumerable<OcctModelShape> shapes, double tolerance = 1E-06)
```

**Parameters**

- `shapes` — `IEnumerable<OcctModelShape>`
- `tolerance` — `double` = 1E-06

**Returns:** `OcctModelShape`

### `Split`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelAlgorithmResult Split(IEnumerable<OcctModelShape> objects, IEnumerable<OcctModelShape> tools, OcctModelBooleanOptions? options = null)
```

**Parameters**

- `objects` — `IEnumerable<OcctModelShape>`
- `tools` — `IEnumerable<OcctModelShape>`
- `options` — `OcctModelBooleanOptions?` = null

**Returns:** `OcctModelAlgorithmResult`

### `Sweep`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelAlgorithmResult Sweep(OcctModelShape spineWire, OcctModelShape profile)
```

**Parameters**

- `spineWire` — `OcctModelShape`
- `profile` — `OcctModelShape`

**Returns:** `OcctModelAlgorithmResult`

### `Translate`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape Translate(OcctModelShape shape, OcctVector3d vector)
```

**Parameters**

- `shape` — `OcctModelShape`
- `vector` — `OcctVector3d`

**Returns:** `OcctModelShape`

### `Triangulate`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Triangulate(OcctModelShape shape, OcctModelMeshParameters? parameters = null)
```

**Parameters**

- `shape` — `OcctModelShape`
- `parameters` — `OcctModelMeshParameters?` = null

**Returns:** `void`

### `TrimEdge`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape TrimEdge(OcctModelShape edge, double firstParameter, double lastParameter)
```

**Parameters**

- `edge` — `OcctModelShape`
- `firstParameter` — `double`
- `lastParameter` — `double`

**Returns:** `OcctModelShape`

### `TryGetShape`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool TryGetShape(long id, out OcctModelShape shape)
```

**Parameters**

- `id` — `long`
- `shape` — `out OcctModelShape`

**Returns:** `bool`

### `UnifySameDomain`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelAlgorithmResult UnifySameDomain(OcctModelShape shape, bool unifyEdges = true, bool unifyFaces = true, bool concatenateBSplines = false)
```

**Parameters**

- `shape` — `OcctModelShape`
- `unifyEdges` — `bool` = true
- `unifyFaces` — `bool` = true
- `concatenateBSplines` — `bool` = false

**Returns:** `OcctModelAlgorithmResult`

## Fields / Enum Values

None

