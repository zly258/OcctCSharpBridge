# OcctModelingSession

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 声明

```csharp
public sealed class OcctModelingSession
```

## 说明

Headless OCCT modeling session. No HWND, AIS context, or viewer is required.

## 构造函数

### `OcctModelingSession`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelingSession()
```

## 属性

### `Capabilities`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public string Capabilities { get; }
```

### `IsDisposed`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsDisposed { get; }
```

### `ShapeCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int ShapeCount { get; }
```

### `Shapes`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> Shapes { get; }
```

## 事件

无

## 方法

### `AnalyzeEdgeAdjacency`

Builds one native edge-to-face topology map and returns the adjacency count for every edge. Use this snapshot when several edge classifications are required for the same root shape.

```csharp
public OcctEdgeAdjacencyResult AnalyzeEdgeAdjacency(OcctModelShape root)
```

**参数**

- `root` — `OcctModelShape`

**返回值:** `OcctEdgeAdjacencyResult`

### `AnalyzeFaces`

Analyzes all faces in one native batch call and returns a stable managed snapshot.

```csharp
public OcctFaceAnalysisResult AnalyzeFaces(OcctModelShape root)
```

**参数**

- `root` — `OcctModelShape`

**返回值:** `OcctFaceAnalysisResult`

### `AnalyzeFreeBounds`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctFreeBoundsResult AnalyzeFreeBounds(OcctModelShape shape, double tolerance = 1E-07, bool splitClosed = true, bool splitOpen = true)
```

**参数**

- `shape` — `OcctModelShape`
- `tolerance` — `double` = 1E-07
- `splitClosed` — `bool` = true
- `splitOpen` — `bool` = true

**返回值:** `OcctFreeBoundsResult`

### `Boolean`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelAlgorithmResult Boolean(OcctBooleanOperation operation, OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options = null)
```

**参数**

- `operation` — `OcctBooleanOperation`
- `left` — `OcctModelShape`
- `right` — `OcctModelShape`
- `options` — `OcctModelBooleanOptions?` = null

**返回值:** `OcctModelAlgorithmResult`

### `ChamferEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelAlgorithmResult ChamferEdges(OcctModelShape shape, IEnumerable<int> edgeIndices, double distance)
```

**参数**

- `shape` — `OcctModelShape`
- `edgeIndices` — `IEnumerable<int>`
- `distance` — `double`

**返回值:** `OcctModelAlgorithmResult`

### `ClassifyPoint`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelState ClassifyPoint(OcctModelShape solid, OcctPoint3d point, double tolerance = 1E-07)
```

**参数**

- `solid` — `OcctModelShape`
- `point` — `OcctPoint3d`
- `tolerance` — `double` = 1E-07

**返回值:** `OcctModelState`

### `Clear`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Clear()
```

**返回值:** `void`

### `ClearTriangulation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ClearTriangulation(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `void`

### `Common`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelAlgorithmResult Common(OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options = null)
```

**参数**

- `left` — `OcctModelShape`
- `right` — `OcctModelShape`
- `options` — `OcctModelBooleanOptions?` = null

**返回值:** `OcctModelAlgorithmResult`

### `Copy`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape Copy(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `OcctModelShape`

### `CreateTopologyReference`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctTopologyReference CreateTopologyReference(OcctModelShape root, OcctModelShape subshape)
```

**参数**

- `root` — `OcctModelShape`
- `subshape` — `OcctModelShape`

**返回值:** `OcctTopologyReference`

### `Cut`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelAlgorithmResult Cut(OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options = null)
```

**参数**

- `left` — `OcctModelShape`
- `right` — `OcctModelShape`
- `options` — `OcctModelBooleanOptions?` = null

**返回值:** `OcctModelAlgorithmResult`

### `Delete`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Delete(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `void`

### `Dispose`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Dispose()
```

**返回值:** `void`

### `EvaluateEdge`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctEdgeEvaluation EvaluateEdge(OcctModelShape edge, double normalizedParameter)
```

**参数**

- `edge` — `OcctModelShape`
- `normalizedParameter` — `double`

**返回值:** `OcctEdgeEvaluation`

### `EvaluateEdgeAtParameter`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelCurveDifferential EvaluateEdgeAtParameter(OcctModelShape edge, double parameter)
```

**参数**

- `edge` — `OcctModelShape`
- `parameter` — `double`

**返回值:** `OcctModelCurveDifferential`

### `EvaluateFace`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctFaceEvaluation EvaluateFace(OcctModelShape face, double u, double v)
```

**参数**

- `face` — `OcctModelShape`
- `u` — `double`
- `v` — `double`

**返回值:** `OcctFaceEvaluation`

### `EvaluateFaceDifferential`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelSurfaceDifferential EvaluateFaceDifferential(OcctModelShape face, double u, double v, double resolution = 1E-09)
```

**参数**

- `face` — `OcctModelShape`
- `u` — `double`
- `v` — `double`
- `resolution` — `double` = 1E-09

**返回值:** `OcctModelSurfaceDifferential`

### `Exists`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Exists(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `bool`

### `ExportBrep`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ExportBrep(OcctModelShape shape, string filePath)
```

**参数**

- `shape` — `OcctModelShape`
- `filePath` — `string`

**返回值:** `void`

### `ExportIges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ExportIges(OcctModelShape shape, string filePath)
```

**参数**

- `shape` — `OcctModelShape`
- `filePath` — `string`

**返回值:** `void`

### `ExportStep`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ExportStep(OcctModelShape shape, string filePath)
```

**参数**

- `shape` — `OcctModelShape`
- `filePath` — `string`

**返回值:** `void`

### `ExportStl`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ExportStl(OcctModelShape shape, string filePath, double linearDeflection = 0.1, double angularDeflection = 0.5, bool ascii = false)
```

**参数**

- `shape` — `OcctModelShape`
- `filePath` — `string`
- `linearDeflection` — `double` = 0.1
- `angularDeflection` — `double` = 0.5
- `ascii` — `bool` = false

**返回值:** `void`

### `Extrude`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelAlgorithmResult Extrude(OcctModelShape profile, OcctVector3d vector)
```

**参数**

- `profile` — `OcctModelShape`
- `vector` — `OcctVector3d`

**返回值:** `OcctModelAlgorithmResult`

### `FilletEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelAlgorithmResult FilletEdges(OcctModelShape shape, IEnumerable<int> edgeIndices, double radius)
```

**参数**

- `shape` — `OcctModelShape`
- `edgeIndices` — `IEnumerable<int>`
- `radius` — `double`

**返回值:** `OcctModelAlgorithmResult`

### `FixShape`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelAlgorithmResult FixShape(OcctModelShape shape, double precision = 1E-07, double minTolerance = 1E-07, double maxTolerance = 1)
```

**参数**

- `shape` — `OcctModelShape`
- `precision` — `double` = 1E-07
- `minTolerance` — `double` = 1E-07
- `maxTolerance` — `double` = 1

**返回值:** `OcctModelAlgorithmResult`

### `Fuse`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelAlgorithmResult Fuse(OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options = null)
```

**参数**

- `left` — `OcctModelShape`
- `right` — `OcctModelShape`
- `options` — `OcctModelBooleanOptions?` = null

**返回值:** `OcctModelAlgorithmResult`

### `GetAdjacentFaces`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetAdjacentFaces(OcctModelShape root, OcctModelShape edge)
```

**参数**

- `root` — `OcctModelShape`
- `edge` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetAncestors`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetAncestors(OcctModelShape root, OcctModelShape child, OcctShapeType ancestorType)
```

**参数**

- `root` — `OcctModelShape`
- `child` — `OcctModelShape`
- `ancestorType` — `OcctShapeType`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetBSplineCurveData`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctBSplineCurveData GetBSplineCurveData(OcctModelShape edge)
```

**参数**

- `edge` — `OcctModelShape`

**返回值:** `OcctBSplineCurveData`

### `GetBSplineSurfaceData`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctBSplineSurfaceData GetBSplineSurfaceData(OcctModelShape face)
```

**参数**

- `face` — `OcctModelShape`

**返回值:** `OcctBSplineSurfaceData`

### `GetBoundaryEdgeCandidates`

Returns edges that are referenced by exactly one distinct face in root. These are useful free-boundary candidates, but periodic seam topology should be checked before treating every returned edge as an open geometric boundary.

```csharp
public IReadOnlyList<OcctModelShape> GetBoundaryEdgeCandidates(OcctModelShape root)
```

**参数**

- `root` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetCircleGeometry`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctCircleGeometry GetCircleGeometry(OcctModelShape edge)
```

**参数**

- `edge` — `OcctModelShape`

**返回值:** `OcctCircleGeometry`

### `GetCompSolids`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetCompSolids(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetCompounds`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetCompounds(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetConeGeometry`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctConeGeometry GetConeGeometry(OcctModelShape face)
```

**参数**

- `face` — `OcctModelShape`

**返回值:** `OcctConeGeometry`

### `GetCylinderGeometry`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctCylinderGeometry GetCylinderGeometry(OcctModelShape face)
```

**参数**

- `face` — `OcctModelShape`

**返回值:** `OcctCylinderGeometry`

### `GetEdgeCurvature`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelCurveCurvature GetEdgeCurvature(OcctModelShape edge, double parameter, double resolution = 1E-09)
```

**参数**

- `edge` — `OcctModelShape`
- `parameter` — `double`
- `resolution` — `double` = 1E-09

**返回值:** `OcctModelCurveCurvature`

### `GetEdgeCurveType`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctCurveType GetEdgeCurveType(OcctModelShape edge)
```

**参数**

- `edge` — `OcctModelShape`

**返回值:** `OcctCurveType`

### `GetEdgeEndpoints`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public ValueTuple<OcctPoint3d, OcctPoint3d> GetEdgeEndpoints(OcctModelShape edge)
```

**参数**

- `edge` — `OcctModelShape`

**返回值:** `ValueTuple<OcctPoint3d, OcctPoint3d>`

### `GetEdgeParameterRange`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelParameterRange GetEdgeParameterRange(OcctModelShape edge)
```

**参数**

- `edge` — `OcctModelShape`

**返回值:** `OcctModelParameterRange`

### `GetEdgeVertices`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetEdgeVertices(OcctModelShape edge)
```

**参数**

- `edge` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetEdges(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetEdgesByAdjacentFaceCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetEdgesByAdjacentFaceCount(OcctModelShape root, int minimumFaceCount, int maximumFaceCount)
```

**参数**

- `root` — `OcctModelShape`
- `minimumFaceCount` — `int`
- `maximumFaceCount` — `int`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetEllipseGeometry`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctEllipseGeometry GetEllipseGeometry(OcctModelShape edge)
```

**参数**

- `edge` — `OcctModelShape`

**返回值:** `OcctEllipseGeometry`

### `GetFaceCurvature`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelSurfaceCurvature GetFaceCurvature(OcctModelShape face, double u, double v, double resolution = 1E-09)
```

**参数**

- `face` — `OcctModelShape`
- `u` — `double`
- `v` — `double`
- `resolution` — `double` = 1E-09

**返回值:** `OcctModelSurfaceCurvature`

### `GetFaceEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetFaceEdges(OcctModelShape face)
```

**参数**

- `face` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetFaceMesh`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctMesh GetFaceMesh(OcctModelShape face)
```

**参数**

- `face` — `OcctModelShape`

**返回值:** `OcctMesh`

### `GetFacePeriodicity`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelSurfacePeriodicity GetFacePeriodicity(OcctModelShape face)
```

**参数**

- `face` — `OcctModelShape`

**返回值:** `OcctModelSurfacePeriodicity`

### `GetFaceSurfaceType`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctSurfaceType GetFaceSurfaceType(OcctModelShape face)
```

**参数**

- `face` — `OcctModelShape`

**返回值:** `OcctSurfaceType`

### `GetFaceUvBounds`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctUvBounds GetFaceUvBounds(OcctModelShape face)
```

**参数**

- `face` — `OcctModelShape`

**返回值:** `OcctUvBounds`

### `GetFaceVertices`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetFaceVertices(OcctModelShape face)
```

**参数**

- `face` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetFaces`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetFaces(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetGeneratedShapes`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetGeneratedShapes(long operationId, OcctModelShape source)
```

**参数**

- `operationId` — `long`
- `source` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetIncidentEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetIncidentEdges(OcctModelShape root, OcctModelShape vertex)
```

**参数**

- `root` — `OcctModelShape`
- `vertex` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetIncidentFaces`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetIncidentFaces(OcctModelShape root, OcctModelShape vertex)
```

**参数**

- `root` — `OcctModelShape`
- `vertex` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetInnerWires`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetInnerWires(OcctModelShape face)
```

**参数**

- `face` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetLineGeometry`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctLineGeometry GetLineGeometry(OcctModelShape edge)
```

**参数**

- `edge` — `OcctModelShape`

**返回值:** `OcctLineGeometry`

### `GetLinearInertiaProperties`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctInertiaProperties GetLinearInertiaProperties(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `OcctInertiaProperties`

### `GetManifoldInteriorEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetManifoldInteriorEdges(OcctModelShape root)
```

**参数**

- `root` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetModifiedShapes`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetModifiedShapes(long operationId, OcctModelShape source)
```

**参数**

- `operationId` — `long`
- `source` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetNonManifoldEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetNonManifoldEdges(OcctModelShape root)
```

**参数**

- `root` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetOperationReport`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public string GetOperationReport(long operationId)
```

**参数**

- `operationId` — `long`

**返回值:** `string`

### `GetOuterWire`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape GetOuterWire(OcctModelShape face)
```

**参数**

- `face` — `OcctModelShape`

**返回值:** `OcctModelShape`

### `GetPlaneGeometry`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPlaneGeometry GetPlaneGeometry(OcctModelShape face)
```

**参数**

- `face` — `OcctModelShape`

**返回值:** `OcctPlaneGeometry`

### `GetShape`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape GetShape(long id)
```

**参数**

- `id` — `long`

**返回值:** `OcctModelShape`

### `GetShapeBounds`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctBounds GetShapeBounds(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `OcctBounds`

### `GetShapeCheckReport`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public string GetShapeCheckReport(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `string`

### `GetShapeDistance`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctDistanceResult GetShapeDistance(OcctModelShape first, OcctModelShape second)
```

**参数**

- `first` — `OcctModelShape`
- `second` — `OcctModelShape`

**返回值:** `OcctDistanceResult`

### `GetShapeHash`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public long GetShapeHash(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `long`

### `GetShapeLinearProperties`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctMassProperties GetShapeLinearProperties(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `OcctMassProperties`

### `GetShapeLocation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelLocation GetShapeLocation(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `OcctModelLocation`

### `GetShapeMaximumTolerance`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double GetShapeMaximumTolerance(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `double`

### `GetShapeMesh`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctMesh GetShapeMesh(OcctModelShape shape, OcctModelMeshParameters? parameters = null)
```

**参数**

- `shape` — `OcctModelShape`
- `parameters` — `OcctModelMeshParameters?` = null

**返回值:** `OcctMesh`

### `GetShapeMeshData`

Builds one combined mesh while preserving the contiguous node and triangle ranges contributed by every source face.

```csharp
public OcctShapeMeshData GetShapeMeshData(OcctModelShape shape, OcctModelMeshParameters? parameters = null)
```

**参数**

- `shape` — `OcctModelShape`
- `parameters` — `OcctModelMeshParameters?` = null

**返回值:** `OcctShapeMeshData`

### `GetShapeOrientation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelOrientation GetShapeOrientation(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `OcctModelOrientation`

### `GetShapeOrientedBounds`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctOrientedBounds GetShapeOrientedBounds(OcctModelShape shape, bool optimal = false)
```

**参数**

- `shape` — `OcctModelShape`
- `optimal` — `bool` = false

**返回值:** `OcctOrientedBounds`

### `GetShapeSurfaceProperties`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctMassProperties GetShapeSurfaceProperties(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `OcctMassProperties`

### `GetShapeType`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShapeType GetShapeType(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `OcctShapeType`

### `GetShapeVolumeProperties`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctMassProperties GetShapeVolumeProperties(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `OcctMassProperties`

### `GetShells`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetShells(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetSolids`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetSolids(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetSphereGeometry`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctSphereGeometry GetSphereGeometry(OcctModelShape face)
```

**参数**

- `face` — `OcctModelShape`

**返回值:** `OcctSphereGeometry`

### `GetSubshapes`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetSubshapes(OcctModelShape shape, OcctShapeType type)
```

**参数**

- `shape` — `OcctModelShape`
- `type` — `OcctShapeType`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetSurfaceInertiaProperties`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctInertiaProperties GetSurfaceInertiaProperties(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `OcctInertiaProperties`

### `GetTopologyCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int GetTopologyCount(OcctModelShape shape, OcctShapeType type)
```

**参数**

- `shape` — `OcctModelShape`
- `type` — `OcctShapeType`

**返回值:** `int`

### `GetTopologyCounts`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyDictionary<OcctShapeType, int> GetTopologyCounts(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `IReadOnlyDictionary<OcctShapeType, int>`

### `GetTorusGeometry`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctTorusGeometry GetTorusGeometry(OcctModelShape face)
```

**参数**

- `face` — `OcctModelShape`

**返回值:** `OcctTorusGeometry`

### `GetVertexPoint`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d GetVertexPoint(OcctModelShape vertex)
```

**参数**

- `vertex` — `OcctModelShape`

**返回值:** `OcctPoint3d`

### `GetVertices`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetVertices(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetVolumeInertiaProperties`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctInertiaProperties GetVolumeInertiaProperties(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `OcctInertiaProperties`

### `GetWireEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetWireEdges(OcctModelShape wire)
```

**参数**

- `wire` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `GetWires`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> GetWires(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `IReadOnlyList<OcctModelShape>`

### `Import`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape Import(string filePath)
```

**参数**

- `filePath` — `string`

**返回值:** `OcctModelShape`

### `ImportBrep`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape ImportBrep(string filePath)
```

**参数**

- `filePath` — `string`

**返回值:** `OcctModelShape`

### `ImportIges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape ImportIges(string filePath)
```

**参数**

- `filePath` — `string`

**返回值:** `OcctModelShape`

### `ImportStep`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape ImportStep(string filePath)
```

**参数**

- `filePath` — `string`

**返回值:** `OcctModelShape`

### `ImportStl`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape ImportStl(string filePath)
```

**参数**

- `filePath` — `string`

**返回值:** `OcctModelShape`

### `InspectShape`

Builds a structured inspection snapshot without making application-specific pass/fail decisions. Mesh statistics are generated only when explicitly requested.

```csharp
public OcctShapeInspectionReport InspectShape(OcctModelShape shape, OcctShapeInspectionOptions? options = null)
```

**参数**

- `shape` — `OcctModelShape`
- `options` — `OcctShapeInspectionOptions?` = null

**返回值:** `OcctShapeInspectionReport`

### `IntersectEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctEdgeIntersection> IntersectEdges(OcctModelShape firstEdge, OcctModelShape secondEdge, double tolerance = 1E-07)
```

**参数**

- `firstEdge` — `OcctModelShape`
- `secondEdge` — `OcctModelShape`
- `tolerance` — `double` = 1E-07

**返回值:** `IReadOnlyList<OcctEdgeIntersection>`

### `IntersectRay`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelRayHit> IntersectRay(OcctModelShape shape, OcctPoint3d origin, OcctVector3d direction, double minimumParameter = 0, double maximumParameter = 1000000000000, double tolerance = 1E-07)
```

**参数**

- `shape` — `OcctModelShape`
- `origin` — `OcctPoint3d`
- `direction` — `OcctVector3d`
- `minimumParameter` — `double` = 0
- `maximumParameter` — `double` = 1000000000000
- `tolerance` — `double` = 1E-07

**返回值:** `IReadOnlyList<OcctModelRayHit>`

### `IsPartnerShape`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsPartnerShape(OcctModelShape first, OcctModelShape second)
```

**参数**

- `first` — `OcctModelShape`
- `second` — `OcctModelShape`

**返回值:** `bool`

### `IsRemoved`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsRemoved(long operationId, OcctModelShape source)
```

**参数**

- `operationId` — `long`
- `source` — `OcctModelShape`

**返回值:** `bool`

### `IsSameShape`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsSameShape(OcctModelShape first, OcctModelShape second)
```

**参数**

- `first` — `OcctModelShape`
- `second` — `OcctModelShape`

**返回值:** `bool`

### `IsShapeClosed`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsShapeClosed(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `bool`

### `IsShapeValid`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsShapeValid(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `bool`

### `Loft`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelAlgorithmResult Loft(IEnumerable<OcctModelShape> sectionWires, bool makeSolid = true, bool ruled = false, double tolerance = 1E-06)
```

**参数**

- `sectionWires` — `IEnumerable<OcctModelShape>`
- `makeSolid` — `bool` = true
- `ruled` — `bool` = false
- `tolerance` — `double` = 1E-06

**返回值:** `OcctModelAlgorithmResult`

### `MakeArc`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeArc(OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees)
```

**参数**

- `center` — `OcctPoint3d`
- `normal` — `OcctVector3d`
- `xDirection` — `OcctVector3d`
- `radius` — `double`
- `startAngleDegrees` — `double`
- `endAngleDegrees` — `double`

**返回值:** `OcctModelShape`

### `MakeArc`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeArc(OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end)
```

**参数**

- `start` — `OcctPoint3d`
- `middle` — `OcctPoint3d`
- `end` — `OcctPoint3d`

**返回值:** `OcctModelShape`

### `MakeBezier`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeBezier(IEnumerable<OcctPoint3d> poles)
```

**参数**

- `poles` — `IEnumerable<OcctPoint3d>`

**返回值:** `OcctModelShape`

### `MakeBox`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeBox(double dx, double dy, double dz, double x = 0, double y = 0, double z = 0)
```

**参数**

- `dx` — `double`
- `dy` — `double`
- `dz` — `double`
- `x` — `double` = 0
- `y` — `double` = 0
- `z` — `double` = 0

**返回值:** `OcctModelShape`

### `MakeCircle`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeCircle(OcctPoint3d center, OcctVector3d normal, double radius)
```

**参数**

- `center` — `OcctPoint3d`
- `normal` — `OcctVector3d`
- `radius` — `double`

**返回值:** `OcctModelShape`

### `MakeCompound`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeCompound(IEnumerable<OcctModelShape> shapes)
```

**参数**

- `shapes` — `IEnumerable<OcctModelShape>`

**返回值:** `OcctModelShape`

### `MakeCone`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeCone(OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height)
```

**参数**

- `origin` — `OcctPoint3d`
- `axis` — `OcctVector3d`
- `radius1` — `double`
- `radius2` — `double`
- `height` — `double`

**返回值:** `OcctModelShape`

### `MakeCylinder`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeCylinder(OcctPoint3d origin, OcctVector3d axis, double radius, double height)
```

**参数**

- `origin` — `OcctPoint3d`
- `axis` — `OcctVector3d`
- `radius` — `double`
- `height` — `double`

**返回值:** `OcctModelShape`

### `MakeEllipse`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeEllipse(OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius)
```

**参数**

- `center` — `OcctPoint3d`
- `normal` — `OcctVector3d`
- `majorRadius` — `double`
- `minorRadius` — `double`

**返回值:** `OcctModelShape`

### `MakeFace`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeFace(OcctModelShape wire)
```

**参数**

- `wire` — `OcctModelShape`

**返回值:** `OcctModelShape`

### `MakeInterpolatedBSpline`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeInterpolatedBSpline(IEnumerable<OcctPoint3d> points, bool periodic = false, double tolerance = 1E-07)
```

**参数**

- `points` — `IEnumerable<OcctPoint3d>`
- `periodic` — `bool` = false
- `tolerance` — `double` = 1E-07

**返回值:** `OcctModelShape`

### `MakeLine`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeLine(OcctPoint3d start, OcctPoint3d end)
```

**参数**

- `start` — `OcctPoint3d`
- `end` — `OcctPoint3d`

**返回值:** `OcctModelShape`

### `MakePlanarFace`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakePlanarFace(OcctModelShape outerWire, IEnumerable<OcctModelShape> innerWires = null)
```

**参数**

- `outerWire` — `OcctModelShape`
- `innerWires` — `IEnumerable<OcctModelShape>` = null

**返回值:** `OcctModelShape`

### `MakePlaneFace`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakePlaneFace(double width, double height, OcctPoint3d? origin = null, OcctVector3d? xDirection = null, OcctVector3d? normal = null)
```

**参数**

- `width` — `double`
- `height` — `double`
- `origin` — `OcctPoint3d?` = null
- `xDirection` — `OcctVector3d?` = null
- `normal` — `OcctVector3d?` = null

**返回值:** `OcctModelShape`

### `MakePolyline`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakePolyline(IEnumerable<OcctPoint3d> points, bool closed = false)
```

**参数**

- `points` — `IEnumerable<OcctPoint3d>`
- `closed` — `bool` = false

**返回值:** `OcctModelShape`

### `MakeRectangleWire`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeRectangleWire(double width, double height, OcctPoint3d? origin = null, OcctVector3d? xDirection = null, OcctVector3d? normal = null)
```

**参数**

- `width` — `double`
- `height` — `double`
- `origin` — `OcctPoint3d?` = null
- `xDirection` — `OcctVector3d?` = null
- `normal` — `OcctVector3d?` = null

**返回值:** `OcctModelShape`

### `MakeRegularPolygon`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeRegularPolygon(double radius, int sideCount, bool makeFace = false, OcctPoint3d? center = null, OcctVector3d? normal = null, OcctVector3d? xDirection = null)
```

**参数**

- `radius` — `double`
- `sideCount` — `int`
- `makeFace` — `bool` = false
- `center` — `OcctPoint3d?` = null
- `normal` — `OcctVector3d?` = null
- `xDirection` — `OcctVector3d?` = null

**返回值:** `OcctModelShape`

### `MakeSolidFromShell`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeSolidFromShell(OcctModelShape shell)
```

**参数**

- `shell` — `OcctModelShape`

**返回值:** `OcctModelShape`

### `MakeSphere`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeSphere(OcctPoint3d center, double radius)
```

**参数**

- `center` — `OcctPoint3d`
- `radius` — `double`

**返回值:** `OcctModelShape`

### `MakeThickSolid`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelAlgorithmResult MakeThickSolid(OcctModelShape solid, IEnumerable<int> faceIndicesToRemove, double thickness, double tolerance = 0.0001)
```

**参数**

- `solid` — `OcctModelShape`
- `faceIndicesToRemove` — `IEnumerable<int>`
- `thickness` — `double`
- `tolerance` — `double` = 0.0001

**返回值:** `OcctModelAlgorithmResult`

### `MakeTorus`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeTorus(OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius)
```

**参数**

- `center` — `OcctPoint3d`
- `axis` — `OcctVector3d`
- `majorRadius` — `double`
- `minorRadius` — `double`

**返回值:** `OcctModelShape`

### `MakeVertex`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeVertex(OcctPoint3d point)
```

**参数**

- `point` — `OcctPoint3d`

**返回值:** `OcctModelShape`

### `MakeWedge`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeWedge(double dx, double dy, double dz, double ltx)
```

**参数**

- `dx` — `double`
- `dy` — `double`
- `dz` — `double`
- `ltx` — `double`

**返回值:** `OcctModelShape`

### `MakeWire`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MakeWire(IEnumerable<OcctModelShape> edges)
```

**参数**

- `edges` — `IEnumerable<OcctModelShape>`

**返回值:** `OcctModelShape`

### `MirrorPlane`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape MirrorPlane(OcctModelShape shape, OcctPoint3d planePoint, OcctVector3d planeNormal)
```

**参数**

- `shape` — `OcctModelShape`
- `planePoint` — `OcctPoint3d`
- `planeNormal` — `OcctVector3d`

**返回值:** `OcctModelShape`

### `OffsetShape`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelAlgorithmResult OffsetShape(OcctModelShape shape, double offset, double tolerance = 0.0001)
```

**参数**

- `shape` — `OcctModelShape`
- `offset` — `double`
- `tolerance` — `double` = 0.0001

**返回值:** `OcctModelAlgorithmResult`

### `OffsetWire`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape OffsetWire(OcctModelShape wire, double offset, double altitude = 0, OcctJoinType joinType = OcctJoinType.Arc, bool openResult = false)
```

**参数**

- `wire` — `OcctModelShape`
- `offset` — `double`
- `altitude` — `double` = 0
- `joinType` — `OcctJoinType` = OcctJoinType.Arc
- `openResult` — `bool` = false

**返回值:** `OcctModelShape`

### `Owns`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Owns(OcctModelShape shape)
```

**参数**

- `shape` — `OcctModelShape`

**返回值:** `bool`

### `ProjectPointOnEdge`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelProjectionResult ProjectPointOnEdge(OcctModelShape edge, OcctPoint3d point)
```

**参数**

- `edge` — `OcctModelShape`
- `point` — `OcctPoint3d`

**返回值:** `OcctModelProjectionResult`

### `ProjectPointOnFace`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelProjectionResult ProjectPointOnFace(OcctModelShape face, OcctPoint3d point)
```

**参数**

- `face` — `OcctModelShape`
- `point` — `OcctPoint3d`

**返回值:** `OcctModelProjectionResult`

### `ResolveTopologyReference`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctTopologyReferenceResult ResolveTopologyReference(OcctModelShape root, OcctTopologyReference reference, double matchingTolerance = 1E-06)
```

**参数**

- `root` — `OcctModelShape`
- `reference` — `OcctTopologyReference`
- `matchingTolerance` — `double` = 1E-06

**返回值:** `OcctTopologyReferenceResult`

### `ResolveTopologyReference`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctTopologyReferenceResult ResolveTopologyReference(OcctModelShape root, OcctTopologyReference reference, long operationId, OcctModelShape sourceShape, double matchingTolerance = 1E-06)
```

**参数**

- `root` — `OcctModelShape`
- `reference` — `OcctTopologyReference`
- `operationId` — `long`
- `sourceShape` — `OcctModelShape`
- `matchingTolerance` — `double` = 1E-06

**返回值:** `OcctTopologyReferenceResult`

### `Revolve`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelAlgorithmResult Revolve(OcctModelShape profile, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees = 360)
```

**参数**

- `profile` — `OcctModelShape`
- `axisPoint` — `OcctPoint3d`
- `axisDirection` — `OcctVector3d`
- `angleDegrees` — `double` = 360

**返回值:** `OcctModelAlgorithmResult`

### `Rotate`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape Rotate(OcctModelShape shape, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees)
```

**参数**

- `shape` — `OcctModelShape`
- `axisPoint` — `OcctPoint3d`
- `axisDirection` — `OcctVector3d`
- `angleDegrees` — `double`

**返回值:** `OcctModelShape`

### `Scale`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape Scale(OcctModelShape shape, OcctPoint3d center, double factor)
```

**参数**

- `shape` — `OcctModelShape`
- `center` — `OcctPoint3d`
- `factor` — `double`

**返回值:** `OcctModelShape`

### `Section`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelAlgorithmResult Section(OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options = null)
```

**参数**

- `left` — `OcctModelShape`
- `right` — `OcctModelShape`
- `options` — `OcctModelBooleanOptions?` = null

**返回值:** `OcctModelAlgorithmResult`

### `SetLocation`

Bridge 2.5 source-compatibility entry point. New code should use OcctNet.OcctModelingSession.SetShapeLocation(OcctNet.OcctModelShape,OcctNet.OcctModelLocation,System.Boolean).

```csharp
public OcctModelShape SetLocation(OcctModelShape shape, OcctModelLocation location, bool copyShape = true)
```

**参数**

- `shape` — `OcctModelShape`
- `location` — `OcctModelLocation`
- `copyShape` — `bool` = true

**返回值:** `OcctModelShape`

### `SetShapeLocation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape SetShapeLocation(OcctModelShape shape, OcctModelLocation location, bool copyShape = true)
```

**参数**

- `shape` — `OcctModelShape`
- `location` — `OcctModelLocation`
- `copyShape` — `bool` = true

**返回值:** `OcctModelShape`

### `Sew`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape Sew(IEnumerable<OcctModelShape> shapes, double tolerance = 1E-06)
```

**参数**

- `shapes` — `IEnumerable<OcctModelShape>`
- `tolerance` — `double` = 1E-06

**返回值:** `OcctModelShape`

### `Split`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelAlgorithmResult Split(IEnumerable<OcctModelShape> objects, IEnumerable<OcctModelShape> tools, OcctModelBooleanOptions? options = null)
```

**参数**

- `objects` — `IEnumerable<OcctModelShape>`
- `tools` — `IEnumerable<OcctModelShape>`
- `options` — `OcctModelBooleanOptions?` = null

**返回值:** `OcctModelAlgorithmResult`

### `Sweep`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelAlgorithmResult Sweep(OcctModelShape spineWire, OcctModelShape profile)
```

**参数**

- `spineWire` — `OcctModelShape`
- `profile` — `OcctModelShape`

**返回值:** `OcctModelAlgorithmResult`

### `Translate`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape Translate(OcctModelShape shape, OcctVector3d vector)
```

**参数**

- `shape` — `OcctModelShape`
- `vector` — `OcctVector3d`

**返回值:** `OcctModelShape`

### `Triangulate`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Triangulate(OcctModelShape shape, OcctModelMeshParameters? parameters = null)
```

**参数**

- `shape` — `OcctModelShape`
- `parameters` — `OcctModelMeshParameters?` = null

**返回值:** `void`

### `TrimEdge`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape TrimEdge(OcctModelShape edge, double firstParameter, double lastParameter)
```

**参数**

- `edge` — `OcctModelShape`
- `firstParameter` — `double`
- `lastParameter` — `double`

**返回值:** `OcctModelShape`

### `TryGetShape`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool TryGetShape(long id, out OcctModelShape shape)
```

**参数**

- `id` — `long`
- `shape` — `out OcctModelShape`

**返回值:** `bool`

### `UnifySameDomain`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelAlgorithmResult UnifySameDomain(OcctModelShape shape, bool unifyEdges = true, bool unifyFaces = true, bool concatenateBSplines = false)
```

**参数**

- `shape` — `OcctModelShape`
- `unifyEdges` — `bool` = true
- `unifyFaces` — `bool` = true
- `concatenateBSplines` — `bool` = false

**返回值:** `OcctModelAlgorithmResult`

## 字段 / 枚举值

无

