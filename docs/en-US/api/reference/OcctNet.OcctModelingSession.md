# OcctModelingSession

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

Headless OCCT modeling session. No HWND, AIS context, or viewer is required.

## Constructors

### `OcctModelingSession`

```csharp
public OcctModelingSession()
```

## Properties

### `Capabilities`

```csharp
public string Capabilities { get; }
```

### `IsDisposed`

```csharp
public bool IsDisposed { get; }
```

### `ShapeCount`

```csharp
public int ShapeCount { get; }
```

### `Shapes`

```csharp
public IReadOnlyList<OcctModelShape> Shapes { get; }
```

## Events

None.

## Methods

### `AnalyzeEdgeAdjacency`

Builds one native edge-to-face topology map and returns the adjacency count for every edge. Use this snapshot when several edge classifications are required for the same root shape.

```csharp
public OcctEdgeAdjacencyResult AnalyzeEdgeAdjacency(OcctModelShape root)
```

### `AnalyzeFaces`

Analyzes all faces in one native batch call and returns a stable managed snapshot.

```csharp
public OcctFaceAnalysisResult AnalyzeFaces(OcctModelShape root)
```

### `AnalyzeFreeBounds`

```csharp
public OcctFreeBoundsResult AnalyzeFreeBounds(OcctModelShape shape, double tolerance, bool splitClosed, bool splitOpen)
```

### `Boolean`

```csharp
public OcctModelAlgorithmResult Boolean(OcctBooleanOperation operation, OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options)
```

### `ChamferEdges`

```csharp
public OcctModelAlgorithmResult ChamferEdges(OcctModelShape shape, IEnumerable<int> edgeIndices, double distance)
```

### `ClassifyPoint`

```csharp
public OcctModelState ClassifyPoint(OcctModelShape solid, OcctPoint3d point, double tolerance)
```

### `Clear`

```csharp
public void Clear()
```

### `ClearTriangulation`

```csharp
public void ClearTriangulation(OcctModelShape shape)
```

### `Common`

```csharp
public OcctModelAlgorithmResult Common(OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options)
```

### `Copy`

```csharp
public OcctModelShape Copy(OcctModelShape shape)
```

### `CreateTopologyReference`

```csharp
public OcctTopologyReference CreateTopologyReference(OcctModelShape root, OcctModelShape subshape)
```

### `Cut`

```csharp
public OcctModelAlgorithmResult Cut(OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options)
```

### `Delete`

```csharp
public void Delete(OcctModelShape shape)
```

### `Dispose`

```csharp
public void Dispose()
```

### `EvaluateEdge`

```csharp
public OcctEdgeEvaluation EvaluateEdge(OcctModelShape edge, double normalizedParameter)
```

### `EvaluateEdgeAtParameter`

```csharp
public OcctModelCurveDifferential EvaluateEdgeAtParameter(OcctModelShape edge, double parameter)
```

### `EvaluateFace`

```csharp
public OcctFaceEvaluation EvaluateFace(OcctModelShape face, double u, double v)
```

### `EvaluateFaceDifferential`

```csharp
public OcctModelSurfaceDifferential EvaluateFaceDifferential(OcctModelShape face, double u, double v, double resolution)
```

### `Exists`

```csharp
public bool Exists(OcctModelShape shape)
```

### `ExportBrep`

```csharp
public void ExportBrep(OcctModelShape shape, string filePath)
```

### `ExportIges`

```csharp
public void ExportIges(OcctModelShape shape, string filePath)
```

### `ExportStep`

```csharp
public void ExportStep(OcctModelShape shape, string filePath)
```

### `ExportStl`

```csharp
public void ExportStl(OcctModelShape shape, string filePath, double linearDeflection, double angularDeflection, bool ascii)
```

### `Extrude`

```csharp
public OcctModelAlgorithmResult Extrude(OcctModelShape profile, OcctVector3d vector)
```

### `FilletEdges`

```csharp
public OcctModelAlgorithmResult FilletEdges(OcctModelShape shape, IEnumerable<int> edgeIndices, double radius)
```

### `FixShape`

```csharp
public OcctModelAlgorithmResult FixShape(OcctModelShape shape, double precision, double minTolerance, double maxTolerance)
```

### `Fuse`

```csharp
public OcctModelAlgorithmResult Fuse(OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options)
```

### `GetAdjacentFaces`

```csharp
public IReadOnlyList<OcctModelShape> GetAdjacentFaces(OcctModelShape root, OcctModelShape edge)
```

### `GetAncestors`

```csharp
public IReadOnlyList<OcctModelShape> GetAncestors(OcctModelShape root, OcctModelShape child, OcctShapeType ancestorType)
```

### `GetBoundaryEdgeCandidates`

Returns edges that are referenced by exactly one distinct face in . These are useful free-boundary candidates, but periodic seam topology should be checked before treating every returned edge as an open geometric boundary.

```csharp
public IReadOnlyList<OcctModelShape> GetBoundaryEdgeCandidates(OcctModelShape root)
```

### `GetBSplineCurveData`

```csharp
public OcctBSplineCurveData GetBSplineCurveData(OcctModelShape edge)
```

### `GetBSplineSurfaceData`

```csharp
public OcctBSplineSurfaceData GetBSplineSurfaceData(OcctModelShape face)
```

### `GetCircleGeometry`

```csharp
public OcctCircleGeometry GetCircleGeometry(OcctModelShape edge)
```

### `GetCompounds`

```csharp
public IReadOnlyList<OcctModelShape> GetCompounds(OcctModelShape shape)
```

### `GetCompSolids`

```csharp
public IReadOnlyList<OcctModelShape> GetCompSolids(OcctModelShape shape)
```

### `GetConeGeometry`

```csharp
public OcctConeGeometry GetConeGeometry(OcctModelShape face)
```

### `GetCylinderGeometry`

```csharp
public OcctCylinderGeometry GetCylinderGeometry(OcctModelShape face)
```

### `GetEdgeCurvature`

```csharp
public OcctModelCurveCurvature GetEdgeCurvature(OcctModelShape edge, double parameter, double resolution)
```

### `GetEdgeCurveType`

```csharp
public OcctCurveType GetEdgeCurveType(OcctModelShape edge)
```

### `GetEdgeEndpoints`

```csharp
public ValueTuple<OcctPoint3d, OcctPoint3d> GetEdgeEndpoints(OcctModelShape edge)
```

### `GetEdgeParameterRange`

```csharp
public OcctModelParameterRange GetEdgeParameterRange(OcctModelShape edge)
```

### `GetEdges`

```csharp
public IReadOnlyList<OcctModelShape> GetEdges(OcctModelShape shape)
```

### `GetEdgesByAdjacentFaceCount`

```csharp
public IReadOnlyList<OcctModelShape> GetEdgesByAdjacentFaceCount(OcctModelShape root, int minimumFaceCount, int maximumFaceCount)
```

### `GetEdgeVertices`

```csharp
public IReadOnlyList<OcctModelShape> GetEdgeVertices(OcctModelShape edge)
```

### `GetEllipseGeometry`

```csharp
public OcctEllipseGeometry GetEllipseGeometry(OcctModelShape edge)
```

### `GetFaceCurvature`

```csharp
public OcctModelSurfaceCurvature GetFaceCurvature(OcctModelShape face, double u, double v, double resolution)
```

### `GetFaceEdges`

```csharp
public IReadOnlyList<OcctModelShape> GetFaceEdges(OcctModelShape face)
```

### `GetFaceMesh`

```csharp
public OcctMesh GetFaceMesh(OcctModelShape face)
```

### `GetFacePeriodicity`

```csharp
public OcctModelSurfacePeriodicity GetFacePeriodicity(OcctModelShape face)
```

### `GetFaces`

```csharp
public IReadOnlyList<OcctModelShape> GetFaces(OcctModelShape shape)
```

### `GetFaceSurfaceType`

```csharp
public OcctSurfaceType GetFaceSurfaceType(OcctModelShape face)
```

### `GetFaceUvBounds`

```csharp
public OcctUvBounds GetFaceUvBounds(OcctModelShape face)
```

### `GetFaceVertices`

```csharp
public IReadOnlyList<OcctModelShape> GetFaceVertices(OcctModelShape face)
```

### `GetGeneratedShapes`

```csharp
public IReadOnlyList<OcctModelShape> GetGeneratedShapes(long operationId, OcctModelShape source)
```

### `GetIncidentEdges`

```csharp
public IReadOnlyList<OcctModelShape> GetIncidentEdges(OcctModelShape root, OcctModelShape vertex)
```

### `GetIncidentFaces`

```csharp
public IReadOnlyList<OcctModelShape> GetIncidentFaces(OcctModelShape root, OcctModelShape vertex)
```

### `GetInnerWires`

```csharp
public IReadOnlyList<OcctModelShape> GetInnerWires(OcctModelShape face)
```

### `GetLinearInertiaProperties`

```csharp
public OcctInertiaProperties GetLinearInertiaProperties(OcctModelShape shape)
```

### `GetLineGeometry`

```csharp
public OcctLineGeometry GetLineGeometry(OcctModelShape edge)
```

### `GetManifoldInteriorEdges`

```csharp
public IReadOnlyList<OcctModelShape> GetManifoldInteriorEdges(OcctModelShape root)
```

### `GetModifiedShapes`

```csharp
public IReadOnlyList<OcctModelShape> GetModifiedShapes(long operationId, OcctModelShape source)
```

### `GetNonManifoldEdges`

```csharp
public IReadOnlyList<OcctModelShape> GetNonManifoldEdges(OcctModelShape root)
```

### `GetOperationReport`

```csharp
public string GetOperationReport(long operationId)
```

### `GetOuterWire`

```csharp
public OcctModelShape GetOuterWire(OcctModelShape face)
```

### `GetPlaneGeometry`

```csharp
public OcctPlaneGeometry GetPlaneGeometry(OcctModelShape face)
```

### `GetShape`

```csharp
public OcctModelShape GetShape(long id)
```

### `GetShapeBounds`

```csharp
public OcctBounds GetShapeBounds(OcctModelShape shape)
```

### `GetShapeCheckReport`

```csharp
public string GetShapeCheckReport(OcctModelShape shape)
```

### `GetShapeDistance`

```csharp
public OcctDistanceResult GetShapeDistance(OcctModelShape first, OcctModelShape second)
```

### `GetShapeHash`

```csharp
public long GetShapeHash(OcctModelShape shape)
```

### `GetShapeLinearProperties`

```csharp
public OcctMassProperties GetShapeLinearProperties(OcctModelShape shape)
```

### `GetShapeLocation`

```csharp
public OcctModelLocation GetShapeLocation(OcctModelShape shape)
```

### `GetShapeMaximumTolerance`

```csharp
public double GetShapeMaximumTolerance(OcctModelShape shape)
```

### `GetShapeMesh`

```csharp
public OcctMesh GetShapeMesh(OcctModelShape shape, OcctModelMeshParameters? parameters)
```

### `GetShapeMeshData`

Builds one combined mesh while preserving the contiguous node and triangle ranges contributed by every source face.

```csharp
public OcctShapeMeshData GetShapeMeshData(OcctModelShape shape, OcctModelMeshParameters? parameters)
```

### `GetShapeOrientation`

```csharp
public OcctModelOrientation GetShapeOrientation(OcctModelShape shape)
```

### `GetShapeOrientedBounds`

```csharp
public OcctOrientedBounds GetShapeOrientedBounds(OcctModelShape shape, bool optimal)
```

### `GetShapeSurfaceProperties`

```csharp
public OcctMassProperties GetShapeSurfaceProperties(OcctModelShape shape)
```

### `GetShapeType`

```csharp
public OcctShapeType GetShapeType(OcctModelShape shape)
```

### `GetShapeVolumeProperties`

```csharp
public OcctMassProperties GetShapeVolumeProperties(OcctModelShape shape)
```

### `GetShells`

```csharp
public IReadOnlyList<OcctModelShape> GetShells(OcctModelShape shape)
```

### `GetSolids`

```csharp
public IReadOnlyList<OcctModelShape> GetSolids(OcctModelShape shape)
```

### `GetSphereGeometry`

```csharp
public OcctSphereGeometry GetSphereGeometry(OcctModelShape face)
```

### `GetSubshapes`

```csharp
public IReadOnlyList<OcctModelShape> GetSubshapes(OcctModelShape shape, OcctShapeType type)
```

### `GetSurfaceInertiaProperties`

```csharp
public OcctInertiaProperties GetSurfaceInertiaProperties(OcctModelShape shape)
```

### `GetTopologyCount`

```csharp
public int GetTopologyCount(OcctModelShape shape, OcctShapeType type)
```

### `GetTopologyCounts`

```csharp
public IReadOnlyDictionary<OcctShapeType, int> GetTopologyCounts(OcctModelShape shape)
```

### `GetTopologyHistorySummary`

```csharp
public OcctTopologyHistorySummary GetTopologyHistorySummary(long operationId, OcctModelShape source)
```

### `GetTorusGeometry`

```csharp
public OcctTorusGeometry GetTorusGeometry(OcctModelShape face)
```

### `GetVertexPoint`

```csharp
public OcctPoint3d GetVertexPoint(OcctModelShape vertex)
```

### `GetVertices`

```csharp
public IReadOnlyList<OcctModelShape> GetVertices(OcctModelShape shape)
```

### `GetVolumeInertiaProperties`

```csharp
public OcctInertiaProperties GetVolumeInertiaProperties(OcctModelShape shape)
```

### `GetWireEdges`

```csharp
public IReadOnlyList<OcctModelShape> GetWireEdges(OcctModelShape wire)
```

### `GetWires`

```csharp
public IReadOnlyList<OcctModelShape> GetWires(OcctModelShape shape)
```

### `Import`

```csharp
public OcctModelShape Import(string filePath)
```

### `ImportBrep`

```csharp
public OcctModelShape ImportBrep(string filePath)
```

### `ImportIges`

```csharp
public OcctModelShape ImportIges(string filePath)
```

### `ImportStep`

```csharp
public OcctModelShape ImportStep(string filePath)
```

### `ImportStl`

```csharp
public OcctModelShape ImportStl(string filePath)
```

### `InspectShape`

Builds a structured inspection snapshot without making application-specific pass/fail decisions. Mesh statistics are generated only when explicitly requested.

```csharp
public OcctShapeInspectionReport InspectShape(OcctModelShape shape, OcctShapeInspectionOptions? options)
```

### `IntersectEdges`

```csharp
public IReadOnlyList<OcctEdgeIntersection> IntersectEdges(OcctModelShape firstEdge, OcctModelShape secondEdge, double tolerance)
```

### `IntersectRay`

```csharp
public IReadOnlyList<OcctModelRayHit> IntersectRay(OcctModelShape shape, OcctPoint3d origin, OcctVector3d direction, double minimumParameter, double maximumParameter, double tolerance)
```

### `IsPartnerShape`

```csharp
public bool IsPartnerShape(OcctModelShape first, OcctModelShape second)
```

### `IsRemoved`

```csharp
public bool IsRemoved(long operationId, OcctModelShape source)
```

### `IsSameShape`

```csharp
public bool IsSameShape(OcctModelShape first, OcctModelShape second)
```

### `IsShapeClosed`

```csharp
public bool IsShapeClosed(OcctModelShape shape)
```

### `IsShapeValid`

```csharp
public bool IsShapeValid(OcctModelShape shape)
```

### `Loft`

```csharp
public OcctModelAlgorithmResult Loft(IEnumerable<OcctModelShape> sectionWires, bool makeSolid, bool ruled, double tolerance)
```

### `MakeArc`

```csharp
public OcctModelShape MakeArc(OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees)
```

### `MakeArc`

```csharp
public OcctModelShape MakeArc(OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end)
```

### `MakeBezier`

```csharp
public OcctModelShape MakeBezier(IEnumerable<OcctPoint3d> poles)
```

### `MakeBox`

```csharp
public OcctModelShape MakeBox(double dx, double dy, double dz, double x, double y, double z)
```

### `MakeCircle`

```csharp
public OcctModelShape MakeCircle(OcctPoint3d center, OcctVector3d normal, double radius)
```

### `MakeCompound`

```csharp
public OcctModelShape MakeCompound(IEnumerable<OcctModelShape> shapes)
```

### `MakeCone`

```csharp
public OcctModelShape MakeCone(OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height)
```

### `MakeCylinder`

```csharp
public OcctModelShape MakeCylinder(OcctPoint3d origin, OcctVector3d axis, double radius, double height)
```

### `MakeEllipse`

```csharp
public OcctModelShape MakeEllipse(OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius)
```

### `MakeFace`

```csharp
public OcctModelShape MakeFace(OcctModelShape wire)
```

### `MakeInterpolatedBSpline`

```csharp
public OcctModelShape MakeInterpolatedBSpline(IEnumerable<OcctPoint3d> points, bool periodic, double tolerance)
```

### `MakeLine`

```csharp
public OcctModelShape MakeLine(OcctPoint3d start, OcctPoint3d end)
```

### `MakePlanarFace`

```csharp
public OcctModelShape MakePlanarFace(OcctModelShape outerWire, IEnumerable<OcctModelShape> innerWires)
```

### `MakePlaneFace`

```csharp
public OcctModelShape MakePlaneFace(double width, double height, OcctPoint3d? origin, OcctVector3d? xDirection, OcctVector3d? normal)
```

### `MakePolyline`

```csharp
public OcctModelShape MakePolyline(IEnumerable<OcctPoint3d> points, bool closed)
```

### `MakeRectangleWire`

```csharp
public OcctModelShape MakeRectangleWire(double width, double height, OcctPoint3d? origin, OcctVector3d? xDirection, OcctVector3d? normal)
```

### `MakeRegularPolygon`

```csharp
public OcctModelShape MakeRegularPolygon(double radius, int sideCount, bool makeFace, OcctPoint3d? center, OcctVector3d? normal, OcctVector3d? xDirection)
```

### `MakeSolidFromShell`

```csharp
public OcctModelShape MakeSolidFromShell(OcctModelShape shell)
```

### `MakeSphere`

```csharp
public OcctModelShape MakeSphere(OcctPoint3d center, double radius)
```

### `MakeThickSolid`

```csharp
public OcctModelAlgorithmResult MakeThickSolid(OcctModelShape solid, IEnumerable<int> faceIndicesToRemove, double thickness, double tolerance)
```

### `MakeTorus`

```csharp
public OcctModelShape MakeTorus(OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius)
```

### `MakeVertex`

```csharp
public OcctModelShape MakeVertex(OcctPoint3d point)
```

### `MakeWedge`

```csharp
public OcctModelShape MakeWedge(double dx, double dy, double dz, double ltx)
```

### `MakeWire`

```csharp
public OcctModelShape MakeWire(IEnumerable<OcctModelShape> edges)
```

### `MirrorPlane`

```csharp
public OcctModelShape MirrorPlane(OcctModelShape shape, OcctPoint3d planePoint, OcctVector3d planeNormal)
```

### `OffsetShape`

```csharp
public OcctModelAlgorithmResult OffsetShape(OcctModelShape shape, double offset, double tolerance)
```

### `OffsetWire`

```csharp
public OcctModelShape OffsetWire(OcctModelShape wire, double offset, double altitude, OcctJoinType joinType, bool openResult)
```

### `Owns`

```csharp
public bool Owns(OcctModelShape shape)
```

### `ProjectPointOnEdge`

```csharp
public OcctModelProjectionResult ProjectPointOnEdge(OcctModelShape edge, OcctPoint3d point)
```

### `ProjectPointOnFace`

```csharp
public OcctModelProjectionResult ProjectPointOnFace(OcctModelShape face, OcctPoint3d point)
```

### `ResolveTopologyReference`

```csharp
public OcctTopologyReferenceResult ResolveTopologyReference(OcctModelShape root, OcctTopologyReference reference, double matchingTolerance)
```

### `ResolveTopologyReference`

```csharp
public OcctTopologyReferenceResult ResolveTopologyReference(OcctModelShape root, OcctTopologyReference reference, long operationId, OcctModelShape sourceShape, double matchingTolerance)
```

### `Revolve`

```csharp
public OcctModelAlgorithmResult Revolve(OcctModelShape profile, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees)
```

### `Rotate`

```csharp
public OcctModelShape Rotate(OcctModelShape shape, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees)
```

### `Scale`

```csharp
public OcctModelShape Scale(OcctModelShape shape, OcctPoint3d center, double factor)
```

### `Section`

```csharp
public OcctModelAlgorithmResult Section(OcctModelShape left, OcctModelShape right, OcctModelBooleanOptions? options)
```

### `SetLocation`

Bridge 2.5 source-compatibility entry point. New code should use .

```csharp
public OcctModelShape SetLocation(OcctModelShape shape, OcctModelLocation location, bool copyShape)
```

### `SetShapeLocation`

```csharp
public OcctModelShape SetShapeLocation(OcctModelShape shape, OcctModelLocation location, bool copyShape)
```

### `Sew`

```csharp
public OcctModelShape Sew(IEnumerable<OcctModelShape> shapes, double tolerance)
```

### `Split`

```csharp
public OcctModelAlgorithmResult Split(IEnumerable<OcctModelShape> objects, IEnumerable<OcctModelShape> tools, OcctModelBooleanOptions? options)
```

### `Sweep`

```csharp
public OcctModelAlgorithmResult Sweep(OcctModelShape spineWire, OcctModelShape profile)
```

### `Translate`

```csharp
public OcctModelShape Translate(OcctModelShape shape, OcctVector3d vector)
```

### `Triangulate`

```csharp
public void Triangulate(OcctModelShape shape, OcctModelMeshParameters? parameters)
```

### `TrimEdge`

```csharp
public OcctModelShape TrimEdge(OcctModelShape edge, double firstParameter, double lastParameter)
```

### `TryGetShape`

```csharp
public bool TryGetShape(long id, OcctModelShape shape)
```

### `UnifySameDomain`

```csharp
public OcctModelAlgorithmResult UnifySameDomain(OcctModelShape shape, bool unifyEdges, bool unifyFaces, bool concatenateBSplines)
```

## Fields / Enum Values

None.

