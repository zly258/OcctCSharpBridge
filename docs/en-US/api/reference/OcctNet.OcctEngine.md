# OcctEngine

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Constructors

### `OcctEngine`

```csharp
public OcctEngine()
```

## Properties

### `FirstSelected`

```csharp
public OcctShape? FirstSelected { get; }
```

### `FirstSelectedObject`

```csharp
public IOcctObject FirstSelectedObject { get; }
```

### `IsDisplayBatchActive`

Returns true while one or more display update batches are active.

```csharp
public bool IsDisplayBatchActive { get; }
```

### `IsDisposed`

```csharp
public bool IsDisposed { get; }
```

### `IsInitialized`

```csharp
public bool IsInitialized { get; }
```

### `ObjectCount`

```csharp
public int ObjectCount { get; }
```

### `Objects`

```csharp
public IReadOnlyList<IOcctObject> Objects { get; }
```

### `OcctVersion`

```csharp
public string OcctVersion { get; }
```

### `SelectedObjects`

```csharp
public IReadOnlyList<IOcctObject> SelectedObjects { get; }
```

### `ShapeCount`

```csharp
public int ShapeCount { get; }
```

### `Shapes`

```csharp
public IReadOnlyList<OcctShape> Shapes { get; }
```

### `ViewClipPlaneLimit`

```csharp
public int ViewClipPlaneLimit { get; }
```

### `ViewScale`

```csharp
public double ViewScale { get; set; }
```

## Events

None.

## Methods

### `AddAngleDimension`

```csharp
public OcctDimension AddAngleDimension(OcctShape firstEdge, OcctShape secondEdge, double flyout, Color? color)
```

### `AddBoss`

```csharp
public OcctShape AddBoss(OcctShape baseShape, OcctShape profile, OcctVector3d vector, bool hideInputs)
```

### `AddDiameterDimension`

```csharp
public OcctDimension AddDiameterDimension(OcctShape circularShape, double flyout, Color? color)
```

### `AddLengthDimension`

```csharp
public OcctDimension AddLengthDimension(OcctShape edge, double flyout, Color? color)
```

### `AddManipulator`

```csharp
public OcctManipulator AddManipulator()
```

### `AddOverlayLine`

```csharp
public OcctOverlay AddOverlayLine(OcctPoint3d start, OcctPoint3d end, OcctOverlayLineStyle style)
```

### `AddOverlayMarker`

```csharp
public OcctOverlay AddOverlayMarker(OcctPoint3d position, OcctOverlayMarkerStyle style)
```

### `AddOverlayPolyline`

```csharp
public OcctOverlay AddOverlayPolyline(IReadOnlyList<OcctPoint3d> points, OcctOverlayLineStyle style)
```

### `AddOverlayText`

```csharp
public OcctOverlay AddOverlayText(string text, OcctPoint3d position, OcctOverlayTextStyle style)
```

### `AddPocket`

```csharp
public OcctShape AddPocket(OcctShape baseShape, OcctShape profile, OcctVector3d vector, bool hideInputs)
```

### `AddPoint`

```csharp
public OcctPoint AddPoint(OcctPoint3d position, OcctMarkerPixmap marker)
```

### `AddPoint`

```csharp
public OcctPoint AddPoint(OcctPoint3d position, OcctPointMarker marker, double scale, Color? color)
```

### `AddRadiusDimension`

```csharp
public OcctDimension AddRadiusDimension(OcctShape circularShape, double flyout, Color? color)
```

### `AddText`

```csharp
public OcctText AddText(string text, OcctPoint3d position, double height, Color? color, bool zoomable)
```

### `ApplyLightingPreset`

```csharp
public void ApplyLightingPreset(OcctLightingPreset preset)
```

### `AttachManipulator`

```csharp
public void AttachManipulator(OcctManipulator manipulator, IEnumerable<IOcctObject> objects, OcctManipulatorAttachOptions options)
```

### `AutoZFit`

Recalculates the current camera Z range when automatic Z fitting is enabled.

```csharp
public void AutoZFit()
```

### `BeginDisplayBatch`

Defers Display, Redisplay and view redraw work until the returned scope is disposed. Use this when creating or changing several objects in one operation.

```csharp
public OcctDisplayBatch BeginDisplayBatch(bool fitAllOnDispose)
```

### `Boolean`

```csharp
public OcctShape Boolean(OcctBooleanOperation operation, OcctShape left, OcctShape right, bool hideInputs)
```

### `ChamferAllEdges`

```csharp
public OcctShape ChamferAllEdges(OcctShape shape, double distance, bool hideInput)
```

### `ChamferEdges`

```csharp
public OcctShape ChamferEdges(OcctShape shape, IEnumerable<int> edgeIndices, double distance, bool hideInput)
```

### `Clear`

```csharp
public void Clear()
```

### `ClearObjectClipPlanes`

```csharp
public void ClearObjectClipPlanes(IOcctObject value)
```

### `ClearObjectHighlightStyle`

```csharp
public void ClearObjectHighlightStyle(IOcctObject value, bool dynamic)
```

### `ClearSelection`

```csharp
public void ClearSelection()
```

### `ClearTransformPersistence`

```csharp
public void ClearTransformPersistence(IOcctObject value)
```

### `ClearViewClipPlanes`

```csharp
public void ClearViewClipPlanes()
```

### `Common`

```csharp
public OcctShape Common(OcctShape left, OcctShape right, bool hideInputs)
```

### `Copy`

```csharp
public OcctShape Copy(OcctShape shape, bool hideInput)
```

### `CopySelectedSubshape`

```csharp
public OcctShape CopySelectedSubshape()
```

### `CopySelectedSubshape`

```csharp
public OcctShape CopySelectedSubshape(int index)
```

### `Cut`

```csharp
public OcctShape Cut(OcctShape left, OcctShape right, bool hideInputs)
```

### `DeactivateManipulatorMode`

```csharp
public void DeactivateManipulatorMode(OcctManipulator manipulator)
```

### `Delete`

```csharp
public void Delete(IEnumerable<IOcctObject> values)
```

### `Delete`

```csharp
public void Delete(IOcctObject value)
```

### `DetachManipulator`

```csharp
public void DetachManipulator(OcctManipulator manipulator)
```

### `DetectAt`

```csharp
public IReadOnlyList<OcctSelectionHitDetail> DetectAt(int x, int y, int maxHits)
```

### `DetectAt`

```csharp
public IReadOnlyList<OcctSelectionHitDetail> DetectAt(int x, int y, OcctDetectionFilter filter, int maxHits)
```

### `Display`

Copies a headless modeling shape into this initialized AIS engine and displays it. The returned shape belongs to this instance.

```csharp
public OcctShape Display(OcctModelingSession model, OcctModelShape shape, bool fit)
```

### `Dispose`

```csharp
public void Dispose()
```

### `DrillHole`

```csharp
public OcctShape DrillHole(OcctShape baseShape, OcctPoint3d origin, OcctVector3d axis, double radius, double depth, bool hideInput)
```

### `DumpView`

```csharp
public void DumpView(string filePath)
```

### `EvaluateEdge`

```csharp
public OcctEdgeEvaluation EvaluateEdge(OcctShape edge, double normalizedParameter)
```

### `EvaluateEdge`

```csharp
public OcctEdgeEvaluation EvaluateEdge(OcctShape owner, int edgeIndex, double normalizedParameter)
```

### `EvaluateFace`

```csharp
public OcctFaceEvaluation EvaluateFace(OcctShape face, double u, double v)
```

### `EvaluateFace`

```csharp
public OcctFaceEvaluation EvaluateFace(OcctShape owner, int faceIndex, double u, double v)
```

### `Exists`

```csharp
public bool Exists(IOcctObject value)
```

### `ExportAllIges`

```csharp
public void ExportAllIges(string filePath)
```

### `ExportAllStep`

```csharp
public void ExportAllStep(string filePath)
```

### `ExportBrep`

```csharp
public void ExportBrep(OcctShape shape, string filePath)
```

### `ExportIges`

```csharp
public void ExportIges(OcctShape shape, string filePath)
```

### `ExportStep`

```csharp
public void ExportStep(OcctShape shape, string filePath)
```

### `ExportStl`

```csharp
public void ExportStl(OcctShape shape, string filePath, double linearDeflection, double angularDeflection, bool ascii)
```

### `Extrude`

```csharp
public OcctShape Extrude(OcctShape profile, OcctVector3d vector, bool hideInput)
```

### `FilletAllEdges`

```csharp
public OcctShape FilletAllEdges(OcctShape shape, double radius, bool hideInput)
```

### `FilletEdges`

```csharp
public OcctShape FilletEdges(OcctShape shape, IEnumerable<int> edgeIndices, double radius, bool hideInput)
```

### `Fit`

```csharp
public void Fit(IEnumerable<OcctShape> shapes, double margin)
```

### `Fit`

```csharp
public void Fit(OcctShape shape)
```

### `FitAll`

```csharp
public void FitAll()
```

### `FitSelected`

```csharp
public void FitSelected(double margin)
```

### `Fuse`

```csharp
public OcctShape Fuse(OcctShape left, OcctShape right, bool hideInputs)
```

### `GetApplicationTag`

```csharp
public string GetApplicationTag(IOcctObject value)
```

### `GetAutoHighlight`

```csharp
public bool GetAutoHighlight(IOcctObject value)
```

### `GetAutoZFitSettings`

Returns the current automatic Z-range fitting settings.

```csharp
public OcctAutoZFitSettings GetAutoZFitSettings()
```

### `GetCamera`

```csharp
public OcctCameraState GetCamera()
```

### `GetDefaultPolygonOffsets`

Returns the polygon offset configured on the Viewer default drawer.

```csharp
public OcctPolygonOffsetSettings GetDefaultPolygonOffsets()
```

### `GetDisplayModeOverride`

```csharp
public OcctDisplayMode? GetDisplayModeOverride(IOcctObject value)
```

### `GetDisplayPriority`

```csharp
public int GetDisplayPriority(IOcctObject value)
```

### `GetEdgeCurveType`

```csharp
public OcctCurveType GetEdgeCurveType(OcctShape edge)
```

### `GetEdgeEndpoints`

```csharp
public ValueTuple<OcctPoint3d, OcctPoint3d> GetEdgeEndpoints(OcctShape edge)
```

### `GetEdgeEndpoints`

```csharp
public ValueTuple<OcctPoint3d, OcctPoint3d> GetEdgeEndpoints(OcctShape owner, int edgeIndex)
```

### `GetFaceCenter`

```csharp
public OcctPoint3d GetFaceCenter(OcctShape owner, int faceIndex)
```

### `GetFaceSurfaceType`

```csharp
public OcctSurfaceType GetFaceSurfaceType(OcctShape face)
```

### `GetFaceUvBounds`

```csharp
public OcctUvBounds GetFaceUvBounds(OcctShape face)
```

### `GetInfiniteState`

```csharp
public bool GetInfiniteState(IOcctObject value)
```

### `GetLocalTransformation`

```csharp
public OcctTransform3d GetLocalTransformation(IOcctObject value)
```

### `GetManipulatorState`

```csharp
public OcctManipulatorState GetManipulatorState(OcctManipulator manipulator)
```

### `GetManipulatorTargets`

```csharp
public IReadOnlyList<IOcctObject> GetManipulatorTargets(OcctManipulator manipulator)
```

### `GetName`

```csharp
public string GetName(IOcctObject value)
```

### `GetObject`

```csharp
public IOcctObject GetObject(long id)
```

### `GetObjectKind`

```csharp
public OcctObjectKind GetObjectKind(long id)
```

### `GetPolygonOffsets`

Returns the effective polygon offset for a Viewer object.

```csharp
public OcctPolygonOffsetSettings GetPolygonOffsets(IOcctObject value)
```

### `GetSceneGravityPoint`

```csharp
public OcctPoint3d GetSceneGravityPoint()
```

### `GetSelectedHits`

```csharp
public IReadOnlyList<OcctSelectionHit> GetSelectedHits()
```

### `GetSelectedObjects`

```csharp
public IReadOnlyList<IOcctObject> GetSelectedObjects()
```

### `GetShape`

```csharp
public OcctShape GetShape(long id)
```

### `GetShapeBounds`

```csharp
public OcctBounds GetShapeBounds(OcctShape shape)
```

### `GetShapeDistance`

```csharp
public OcctDistanceResult GetShapeDistance(OcctShape first, OcctShape second)
```

### `GetShapeHash`

```csharp
public long GetShapeHash(OcctShape shape)
```

### `GetShapeLinearProperties`

```csharp
public OcctMassProperties GetShapeLinearProperties(OcctShape shape)
```

### `GetShapeSurfaceProperties`

```csharp
public OcctMassProperties GetShapeSurfaceProperties(OcctShape shape)
```

### `GetShapeType`

```csharp
public OcctShapeType GetShapeType(OcctShape shape)
```

### `GetShapeVolumeProperties`

```csharp
public OcctMassProperties GetShapeVolumeProperties(OcctShape shape)
```

### `GetSubshapeAt`

```csharp
public OcctShape GetSubshapeAt(OcctShape shape, OcctShapeType type, int index)
```

### `GetSubshapes`

```csharp
public IReadOnlyList<OcctShape> GetSubshapes(OcctShape shape, OcctShapeType type)
```

### `GetTopologyCount`

```csharp
public int GetTopologyCount(OcctShape shape, OcctShapeType type)
```

### `GetTransformPersistence`

```csharp
public OcctTransformPersistenceState GetTransformPersistence(IOcctObject value)
```

### `GetVertexPoint`

```csharp
public OcctPoint3d GetVertexPoint(OcctShape owner, int vertexIndex)
```

### `GetVertexPoint`

```csharp
public OcctPoint3d GetVertexPoint(OcctShape vertex)
```

### `GetViewportState`

```csharp
public OcctViewportState GetViewportState()
```

### `GetZLayer`

```csharp
public OcctZLayer GetZLayer(IOcctObject value)
```

### `HasLocalTransformation`

```csharp
public bool HasLocalTransformation(IOcctObject value)
```

### `HideAll`

```csharp
public void HideAll()
```

### `HideSelected`

```csharp
public void HideSelected()
```

### `HideSelectionRectangle`

Removes the OCCT-native rubber-band selection overlay.

```csharp
public void HideSelectionRectangle()
```

### `Highlight`

```csharp
public void Highlight(IOcctObject value)
```

### `Import`

```csharp
public OcctShape Import(string filePath)
```

### `ImportBrep`

```csharp
public OcctShape ImportBrep(string filePath)
```

### `ImportIges`

```csharp
public OcctShape ImportIges(string filePath)
```

### `ImportStep`

```csharp
public OcctShape ImportStep(string filePath)
```

### `ImportStepDocument`

Imports a STEP file through STEPCAFControl/XDE and returns its assembly occurrence tree. The existing API remains available for source compatibility.

```csharp
public OcctAssemblyDocument ImportStepDocument(string filePath)
```

### `ImportStl`

```csharp
public OcctShape ImportStl(string filePath)
```

### `Initialize`

```csharp
public void Initialize(IntPtr windowHandle)
```

### `InvertSelection`

```csharp
public void InvertSelection()
```

### `IsSelectable`

```csharp
public bool IsSelectable(IOcctObject value)
```

### `IsSelected`

```csharp
public bool IsSelected(IOcctObject value)
```

### `IsShapeValid`

```csharp
public bool IsShapeValid(OcctShape shape)
```

### `IsVisible`

```csharp
public bool IsVisible(IOcctObject value)
```

### `Loft`

```csharp
public OcctShape Loft(IEnumerable<OcctShape> sectionWires, bool makeSolid, bool ruled, double tolerance, bool hideInputs)
```

### `MakeAngleAnnotationShape`

Creates a result-only BRep angular annotation, including vector text and arrows.

```csharp
public OcctShape MakeAngleAnnotationShape(OcctShape firstEdge, OcctShape secondEdge, double radius, double textHeight, double arrowSize, string fontName)
```

### `MakeArc`

```csharp
public OcctShape MakeArc(OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees)
```

### `MakeArc`

```csharp
public OcctShape MakeArc(OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end)
```

### `MakeBezier`

```csharp
public OcctShape MakeBezier(IEnumerable<OcctPoint3d> poles)
```

### `MakeBox`

```csharp
public OcctShape MakeBox(double dx, double dy, double dz, double x, double y, double z)
```

### `MakeCircle`

```csharp
public OcctShape MakeCircle(OcctPoint3d center, OcctVector3d normal, double radius)
```

### `MakeCompound`

```csharp
public OcctShape MakeCompound(IEnumerable<OcctShape> shapes, bool hideInputs)
```

### `MakeCone`

```csharp
public OcctShape MakeCone(double radius1, double radius2, double height, double x, double y, double z)
```

### `MakeCone`

```csharp
public OcctShape MakeCone(OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height)
```

### `MakeCylinder`

```csharp
public OcctShape MakeCylinder(double radius, double height, double x, double y, double z)
```

### `MakeCylinder`

```csharp
public OcctShape MakeCylinder(OcctPoint3d origin, OcctVector3d axis, double radius, double height)
```

### `MakeDiameterAnnotationShape`

Creates a result-only BRep diameter annotation, including vector text and arrows.

```csharp
public OcctShape MakeDiameterAnnotationShape(OcctShape circularEdge, double flyout, double textHeight, double arrowSize, string fontName)
```

### `MakeEllipse`

```csharp
public OcctShape MakeEllipse(OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius)
```

### `MakeFace`

```csharp
public OcctShape MakeFace(OcctShape wire, bool onlyPlane)
```

### `MakeInterpolatedBSpline`

```csharp
public OcctShape MakeInterpolatedBSpline(IEnumerable<OcctPoint3d> points, bool periodic, double tolerance)
```

### `MakeLengthAnnotationShape`

Creates a result-only BRep linear annotation, including vector text and arrows.

```csharp
public OcctShape MakeLengthAnnotationShape(OcctShape edge, double flyout, double textHeight, double arrowSize, string fontName)
```

### `MakeLine`

```csharp
public OcctShape MakeLine(OcctPoint3d start, OcctPoint3d end)
```

### `MakePlaneFace`

```csharp
public OcctShape MakePlaneFace(double width, double height, OcctPoint3d? origin, OcctVector3d? xDirection, OcctVector3d? normal)
```

### `MakePolyline`

```csharp
public OcctShape MakePolyline(IEnumerable<OcctPoint3d> points, bool closed)
```

### `MakeRadiusAnnotationShape`

Creates a result-only BRep radius annotation, including vector text and an arrow.

```csharp
public OcctShape MakeRadiusAnnotationShape(OcctShape circularEdge, double flyout, double textHeight, double arrowSize, string fontName)
```

### `MakeRectangleWire`

```csharp
public OcctShape MakeRectangleWire(double width, double height, OcctPoint3d? origin, OcctVector3d? xDirection, OcctVector3d? normal)
```

### `MakeRegularPolygon`

```csharp
public OcctShape MakeRegularPolygon(double radius, int sideCount, bool makeFace, OcctPoint3d? center, OcctVector3d? normal, OcctVector3d? xDirection)
```

### `MakeSolidFromShell`

```csharp
public OcctShape MakeSolidFromShell(OcctShape shell, bool hideInput)
```

### `MakeSphere`

```csharp
public OcctShape MakeSphere(double radius, double x, double y, double z)
```

### `MakeTextShape`

Creates vector BRep text that remains geometrically sharp at any zoom level.

```csharp
public OcctShape MakeTextShape(string text, OcctPoint3d position, double height, double extrusionDepth, string fontName, OcctVector3d? normal, OcctVector3d? xDirection, bool bold, bool italic)
```

### `MakeThickSolid`

```csharp
public OcctShape MakeThickSolid(OcctShape solid, int faceIndexToRemove, double thickness, double tolerance, bool hideInput)
```

### `MakeTorus`

```csharp
public OcctShape MakeTorus(double majorRadius, double minorRadius, OcctPoint3d? center, OcctVector3d? axis)
```

### `MakeVertex`

```csharp
public OcctShape MakeVertex(OcctPoint3d point)
```

### `MakeWedge`

```csharp
public OcctShape MakeWedge(double dx, double dy, double dz, double ltx)
```

### `MakeWire`

```csharp
public OcctShape MakeWire(IEnumerable<OcctShape> edges, bool hideInputs)
```

### `MirrorPlane`

```csharp
public OcctShape MirrorPlane(OcctShape shape, OcctPoint3d planePoint, OcctVector3d planeNormal, bool hideInput)
```

### `MoveTo`

```csharp
public void MoveTo(int x, int y)
```

### `Offset`

```csharp
public OcctShape Offset(OcctShape shape, double offset, double tolerance, bool hideInput)
```

### `Owns`

```csharp
public bool Owns(IOcctObject value)
```

### `Pan`

```csharp
public void Pan(int deltaX, int deltaY)
```

### `Redisplay`

```csharp
public void Redisplay(IEnumerable<IOcctObject> values)
```

### `Redisplay`

```csharp
public void Redisplay(IOcctObject value)
```

### `Redraw`

```csharp
public void Redraw()
```

### `ResetDisplayMode`

```csharp
public void ResetDisplayMode(IOcctObject value)
```

### `ResetLocalTransformation`

```csharp
public void ResetLocalTransformation(IOcctObject value)
```

### `ResetPolygonOffsets`

Restores a Viewer object's polygon offset to the current default drawer values.

```csharp
public void ResetPolygonOffsets(IOcctObject value)
```

### `ResetSceneLighting`

```csharp
public void ResetSceneLighting()
```

### `ResetView`

```csharp
public void ResetView()
```

### `ResetViewMapping`

```csharp
public void ResetViewMapping()
```

### `ResetViewOrientation`

```csharp
public void ResetViewOrientation()
```

### `Resize`

```csharp
public void Resize()
```

### `ResizeSurface`

Synchronizes the OCCT render surface with the native window size without drawing a frame. UI adapters can coalesce repeated resize notifications and call once.

```csharp
public void ResizeSurface()
```

### `Revolve`

```csharp
public OcctShape Revolve(OcctShape profile, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, bool hideInput)
```

### `Rotate`

```csharp
public OcctShape Rotate(OcctShape shape, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, bool hideInput)
```

### `Rotation`

```csharp
public void Rotation(int x, int y)
```

### `Scale`

```csharp
public OcctShape Scale(OcctShape shape, OcctPoint3d center, double factor, bool hideInput)
```

### `ScreenToPlane`

```csharp
public OcctPoint3d ScreenToPlane(int x, int y, OcctPoint3d planePoint, OcctVector3d planeNormal)
```

### `ScreenToRay`

```csharp
public OcctProjectionRay ScreenToRay(int x, int y)
```

### `ScreenToWorld`

```csharp
public OcctPoint3d ScreenToWorld(int x, int y)
```

### `Section`

```csharp
public OcctShape Section(OcctShape left, OcctShape right, bool hideInputs)
```

### `Select`

```csharp
public void Select(int x, int y, bool appendSelection)
```

### `SelectAllVisible`

```csharp
public void SelectAllVisible()
```

### `SelectObject`

```csharp
public void SelectObject(IOcctObject value, bool appendSelection)
```

### `SelectObjects`

```csharp
public void SelectObjects(IEnumerable<IOcctObject> values, bool appendSelection)
```

### `SelectRectangle`

```csharp
public void SelectRectangle(int x1, int y1, int x2, int y2, bool appendSelection, bool allowOverlap)
```

### `SetAntialiasing`

```csharp
public void SetAntialiasing(bool enabled)
```

### `SetApplicationTag`

```csharp
public void SetApplicationTag(IOcctObject value, string applicationTag)
```

### `SetAutoHighlight`

```csharp
public void SetAutoHighlight(IOcctObject value, bool enabled)
```

### `SetAutomaticHighlight`

```csharp
public void SetAutomaticHighlight(bool enabled)
```

### `SetAutoZFitMode`

Enables or disables automatic adjustment of the camera Z range. This improves depth precision and prevents clipping, but it does not separate two coplanar objects.

```csharp
public void SetAutoZFitMode(bool enabled, double scaleFactor)
```

### `SetBackground`

```csharp
public void SetBackground(Color color)
```

### `SetCamera`

```csharp
public void SetCamera(OcctCameraState state)
```

### `SetColor`

```csharp
public void SetColor(IEnumerable<IOcctObject> values, Color color)
```

### `SetColor`

```csharp
public void SetColor(IOcctObject value, Color color)
```

### `SetComputedHlr`

```csharp
public void SetComputedHlr(bool enabled)
```

### `SetDefaultFaceBoundaryStyle`

```csharp
public void SetDefaultFaceBoundaryStyle(bool visible, Color color, double width, bool applyExisting)
```

### `SetDefaultMaterial`

```csharp
public void SetDefaultMaterial(OcctMaterial material, bool applyExisting)
```

### `SetDefaultPolygonOffsets`

Changes the default polygon offset inherited by future Viewer objects. OCCT's recommended shaded-view baseline is Fill, factor 1, units 1.

```csharp
public void SetDefaultPolygonOffsets(OcctPolygonOffsetMode mode, double factor, double units, bool applyExisting)
```

### `SetDimensionFlyout`

```csharp
public void SetDimensionFlyout(OcctDimension dimension, double flyout)
```

### `SetDisplayMode`

```csharp
public void SetDisplayMode(IEnumerable<IOcctObject> values, OcctDisplayMode displayMode)
```

### `SetDisplayMode`

```csharp
public void SetDisplayMode(IOcctObject value, OcctDisplayMode displayMode)
```

### `SetDisplayMode`

```csharp
public void SetDisplayMode(OcctDisplayMode displayMode)
```

### `SetDisplayPrecision`

```csharp
public void SetDisplayPrecision(double deviationCoefficient, double deviationAngleDegrees, bool applyExisting)
```

### `SetDisplayPriority`

```csharp
public void SetDisplayPriority(IEnumerable<IOcctObject> values, int priority)
```

### `SetDisplayPriority`

```csharp
public void SetDisplayPriority(IOcctObject value, int priority)
```

### `SetFaceBoundariesVisible`

```csharp
public void SetFaceBoundariesVisible(bool visible, bool applyExisting)
```

### `SetFaceBoundaryStyle`

```csharp
public void SetFaceBoundaryStyle(IEnumerable<OcctShape> shapes, bool visible, Color color, double width)
```

### `SetFaceBoundaryStyle`

```csharp
public void SetFaceBoundaryStyle(OcctShape shape, bool visible, Color color, double width)
```

### `SetFrustumCulling`

```csharp
public void SetFrustumCulling(bool enabled)
```

### `SetGradientBackground`

```csharp
public void SetGradientBackground(Color first, Color second, OcctGradientFillMethod fillMethod)
```

### `SetHighlightStyle`

```csharp
public void SetHighlightStyle(OcctHighlightStyleKind kind, OcctHighlightStyle style)
```

### `SetHoverHighlightColor`

```csharp
public void SetHoverHighlightColor(Color color)
```

### `SetImmediateUpdate`

```csharp
public void SetImmediateUpdate(bool enabled)
```

### `SetInfiniteState`

```csharp
public void SetInfiniteState(IOcctObject value, bool infinite)
```

### `SetLineWidth`

```csharp
public void SetLineWidth(IEnumerable<IOcctObject> values, double width)
```

### `SetLineWidth`

```csharp
public void SetLineWidth(IOcctObject value, double width)
```

### `SetLocalTransformation`

```csharp
public void SetLocalTransformation(IOcctObject value, OcctTransform3d transform)
```

### `SetLocalTransformations`

```csharp
public void SetLocalTransformations(IReadOnlyList<OcctObjectTransformUpdate> updates)
```

### `SetManipulatorGap`

```csharp
public void SetManipulatorGap(OcctManipulator manipulator, double gap)
```

### `SetManipulatorModeActivationOnDetection`

```csharp
public void SetManipulatorModeActivationOnDetection(OcctManipulator manipulator, bool enabled)
```

### `SetManipulatorModeEnabled`

```csharp
public void SetManipulatorModeEnabled(OcctManipulator manipulator, OcctManipulatorMode mode, bool enabled)
```

### `SetManipulatorPart`

```csharp
public void SetManipulatorPart(OcctManipulator manipulator, OcctManipulatorMode mode, bool enabled, int? axisIndex)
```

### `SetManipulatorPosition`

```csharp
public void SetManipulatorPosition(OcctManipulator manipulator, OcctPoint3d origin, OcctVector3d normal, OcctVector3d xDirection)
```

### `SetManipulatorSize`

```csharp
public void SetManipulatorSize(OcctManipulator manipulator, double size)
```

### `SetManipulatorSkin`

```csharp
public void SetManipulatorSkin(OcctManipulator manipulator, OcctManipulatorSkin skin)
```

### `SetManipulatorZoomPersistence`

```csharp
public void SetManipulatorZoomPersistence(OcctManipulator manipulator, bool enabled)
```

### `SetMaterial`

```csharp
public void SetMaterial(IEnumerable<IOcctObject> values, OcctMaterial material)
```

### `SetMaterial`

```csharp
public void SetMaterial(IOcctObject value, OcctMaterial material)
```

### `SetMsaaSamples`

```csharp
public void SetMsaaSamples(int samples)
```

### `SetName`

```csharp
public void SetName(IOcctObject value, string name)
```

### `SetObjectClipPlanes`

```csharp
public void SetObjectClipPlanes(IOcctObject value, IReadOnlyList<OcctViewClipPlane> planes)
```

### `SetObjectHighlightStyle`

```csharp
public void SetObjectHighlightStyle(IOcctObject value, bool dynamic, OcctHighlightStyle style)
```

### `SetOverlayLineStyle`

```csharp
public void SetOverlayLineStyle(OcctOverlay overlay, OcctOverlayLineStyle style)
```

### `SetOverlayMarkerStyle`

```csharp
public void SetOverlayMarkerStyle(OcctOverlay overlay, OcctOverlayMarkerStyle style)
```

### `SetOverlayTextStyle`

```csharp
public void SetOverlayTextStyle(OcctOverlay overlay, OcctOverlayTextStyle style)
```

### `SetPerspectiveFieldOfView`

```csharp
public void SetPerspectiveFieldOfView(double degrees)
```

### `SetPointPosition`

```csharp
public void SetPointPosition(OcctPoint point, OcctPoint3d position)
```

### `SetPointStyle`

```csharp
public void SetPointStyle(OcctPoint point, OcctMarkerPixmap marker)
```

### `SetPointStyle`

```csharp
public void SetPointStyle(OcctPoint point, OcctPointMarker marker, double scale, Color color)
```

### `SetPolygonOffsets`

Sets a per-object polygon offset. Use a negative Fill offset to draw a coplanar overlay in front of its reference object, or a larger positive value to push it behind.

```csharp
public void SetPolygonOffsets(IOcctObject value, OcctPolygonOffsetMode mode, double factor, double units)
```

### `SetProjection`

```csharp
public void SetProjection(OcctProjectionType projection)
```

### `SetRenderingMethod`

```csharp
public void SetRenderingMethod(OcctRenderingMethod method)
```

### `SetRenderResolution`

```csharp
public void SetRenderResolution(double dpi)
```

### `SetRenderResolutionScale`

```csharp
public void SetRenderResolutionScale(double scale)
```

### `SetSceneLighting`

Bridge 2.5 source-compatibility entry point. New code should construct explicitly.

```csharp
public void SetSceneLighting(double ambientIntensity, double directionalIntensity, OcctVector3d direction, bool headlight)
```

### `SetSceneLighting`

Bridge 2.5 source-compatibility entry point. New code should construct explicitly.

```csharp
public void SetSceneLighting(OcctSceneLightingSettings settings)
```

### `SetSelectable`

```csharp
public void SetSelectable(IEnumerable<IOcctObject> values, bool selectable)
```

### `SetSelectable`

```csharp
public void SetSelectable(IOcctObject value, bool selectable)
```

### `SetSelection`

```csharp
public void SetSelection(IEnumerable<IOcctObject> values, OcctSelectionOperation operation)
```

### `SetSelectionHighlightColor`

```csharp
public void SetSelectionHighlightColor(Color color)
```

### `SetSelectionMode`

```csharp
public void SetSelectionMode(OcctSelectionMode mode)
```

### `SetSelectionModeActive`

```csharp
public void SetSelectionModeActive(IOcctObject value, OcctSelectionMode mode, bool active, OcctSelectionModeConcurrency concurrency, bool force)
```

### `SetSelectionSensitivity`

```csharp
public void SetSelectionSensitivity(IOcctObject value, OcctSelectionMode mode, int sensitivity)
```

### `SetSelectionTolerance`

```csharp
public void SetSelectionTolerance(int pixelTolerance)
```

### `SetShadowsEnabled`

```csharp
public void SetShadowsEnabled(bool enabled)
```

### `SetText`

```csharp
public void SetText(OcctText textObject, string text)
```

### `SetTextAngle`

```csharp
public void SetTextAngle(OcctText textObject, double angleDegrees)
```

### `SetTextFont`

```csharp
public void SetTextFont(OcctText textObject, string fontName)
```

### `SetTextHeight`

```csharp
public void SetTextHeight(OcctText textObject, double height)
```

### `SetTextPosition`

```csharp
public void SetTextPosition(OcctText textObject, OcctPoint3d position)
```

### `SetTextZoomable`

```csharp
public void SetTextZoomable(OcctText textObject, bool zoomable)
```

### `SetTransformPersistence`

```csharp
public void SetTransformPersistence(IOcctObject value, OcctTransformPersistenceMode mode, OcctCornerPosition position, int offsetX, int offsetY)
```

### `SetTransformPersistence`

```csharp
public void SetTransformPersistence(IOcctObject value, OcctTransformPersistenceMode mode, OcctPoint3d anchor)
```

### `SetTransparency`

```csharp
public void SetTransparency(IEnumerable<IOcctObject> values, double transparency)
```

### `SetTransparency`

```csharp
public void SetTransparency(IOcctObject value, double transparency)
```

### `SetTriedron`

```csharp
public void SetTriedron(OcctTriedronOptions options)
```

### `SetTriedronVisible`

```csharp
public void SetTriedronVisible(bool visible)
```

### `SetView`

```csharp
public void SetView(OcctViewOrientation orientation)
```

### `SetViewClipPlanes`

```csharp
public void SetViewClipPlanes(IReadOnlyList<OcctViewClipPlane> planes)
```

### `SetViewCubeLanguage`

```csharp
public void SetViewCubeLanguage(OcctViewCubeLanguage language)
```

### `SetViewCubeOptions`

```csharp
public void SetViewCubeOptions(OcctViewCubeOptions options)
```

### `SetViewCubeVisible`

```csharp
public void SetViewCubeVisible(bool visible)
```

### `SetVisible`

```csharp
public void SetVisible(IEnumerable<IOcctObject> values, bool visible)
```

### `SetVisible`

```csharp
public void SetVisible(IOcctObject value, bool visible)
```

### `SetZLayer`

```csharp
public void SetZLayer(IEnumerable<IOcctObject> values, OcctZLayer layer)
```

### `SetZLayer`

```csharp
public void SetZLayer(IOcctObject value, OcctZLayer layer)
```

### `SetZUpView`

```csharp
public void SetZUpView(OcctZUpViewOrientation orientation, bool fitAll)
```

### `Sew`

```csharp
public OcctShape Sew(IEnumerable<OcctShape> shapes, double tolerance, bool hideInputs)
```

### `ShowAll`

```csharp
public void ShowAll()
```

### `ShowSelectionRectangle`

Displays or updates an OCCT-native 2D rubber-band rectangle in the top overlay layer. Coordinates use window client pixels with the origin at the upper-left corner.

```csharp
public void ShowSelectionRectangle(int x1, int y1, int x2, int y2, Color lineColor, Color fillColor, double fillTransparency, double lineWidth)
```

### `StartManipulatorTransform`

```csharp
public void StartManipulatorTransform(OcctManipulator manipulator, int x, int y)
```

### `StartRotation`

```csharp
public void StartRotation(int x, int y)
```

### `StopManipulatorTransform`

```csharp
public void StopManipulatorTransform(OcctManipulator manipulator, bool apply)
```

### `Sweep`

```csharp
public OcctShape Sweep(OcctShape spineWire, OcctShape profile, bool hideInputs)
```

### `Translate`

```csharp
public OcctShape Translate(OcctShape shape, OcctVector3d vector, bool hideInput)
```

### `TryGetDetectedHit`

```csharp
public bool TryGetDetectedHit(OcctSelectionHit hit)
```

### `TryGetDetectedHitDetail`

```csharp
public bool TryGetDetectedHitDetail(OcctSelectionHitDetail hit)
```

### `TryGetDetectionCandidate`

```csharp
public bool TryGetDetectionCandidate(int x, int y, OcctDetectionFilter filter, int cycleIndex, OcctSelectionHitDetail hit, int maxHits)
```

### `TryGetObject`

```csharp
public bool TryGetObject(long id, IOcctObject value)
```

### `TryGetObjectByApplicationTag`

```csharp
public bool TryGetObjectByApplicationTag(string applicationTag, IOcctObject value)
```

### `TryGetShape`

```csharp
public bool TryGetShape(long id, OcctShape shape)
```

### `TryScreenToPlane`

```csharp
public bool TryScreenToPlane(int x, int y, OcctPoint3d planePoint, OcctVector3d planeNormal, OcctPoint3d result)
```

### `Unhighlight`

```csharp
public void Unhighlight(IOcctObject value)
```

### `UpdateManipulatorTransform`

```csharp
public void UpdateManipulatorTransform(OcctManipulator manipulator, int x, int y)
```

### `UpdateOverlayLine`

```csharp
public void UpdateOverlayLine(OcctOverlay overlay, OcctPoint3d start, OcctPoint3d end)
```

### `UpdateOverlayMarker`

```csharp
public void UpdateOverlayMarker(OcctOverlay overlay, OcctPoint3d position)
```

### `UpdateOverlayPolyline`

```csharp
public void UpdateOverlayPolyline(OcctOverlay overlay, IReadOnlyList<OcctPoint3d> points)
```

### `UpdateOverlayText`

```csharp
public void UpdateOverlayText(OcctOverlay overlay, string text, OcctPoint3d position)
```

### `UpdatePoints`

```csharp
public void UpdatePoints(IReadOnlyList<OcctPointStateUpdate> updates)
```

### `UpdateShape`

```csharp
public void UpdateShape(OcctShape viewerShape, OcctModelingSession sourceSession, OcctModelShape sourceShape, OcctShapeUpdateOptions options)
```

### `WindowFit`

```csharp
public void WindowFit(int x1, int y1, int x2, int y2)
```

### `WorldToScreen`

```csharp
public Point WorldToScreen(OcctPoint3d point)
```

### `Zoom`

```csharp
public void Zoom(double factor)
```

### `ZoomAtPoint`

```csharp
public void ZoomAtPoint(int x, int y, double delta)
```

## Fields / Enum Values

None.

