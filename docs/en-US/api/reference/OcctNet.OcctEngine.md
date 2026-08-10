# OcctEngine

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Declaration

```csharp
public sealed class OcctEngine
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctEngine`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctEngine()
```

## Properties

### `FirstSelected`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape? FirstSelected { get; }
```

### `FirstSelectedObject`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IOcctObject FirstSelectedObject { get; }
```

### `IsDisplayBatchActive`

Returns true while one or more display update batches are active.

```csharp
public bool IsDisplayBatchActive { get; }
```

### `IsDisposed`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsDisposed { get; }
```

### `IsInitialized`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsInitialized { get; }
```

### `ObjectCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int ObjectCount { get; }
```

### `Objects`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<IOcctObject> Objects { get; }
```

### `OcctVersion`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public string OcctVersion { get; }
```

### `SelectedObjects`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<IOcctObject> SelectedObjects { get; }
```

### `ShapeCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int ShapeCount { get; }
```

### `Shapes`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctShape> Shapes { get; }
```

### `ViewScale`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double ViewScale { get; set; }
```

## Events

None

## Methods

### `AddAngleDimension`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctDimension AddAngleDimension(OcctShape firstEdge, OcctShape secondEdge, double flyout = 20, Color? color = null)
```

**Parameters**

- `firstEdge` — `OcctShape`
- `secondEdge` — `OcctShape`
- `flyout` — `double` = 20
- `color` — `Color?` = null

**Returns:** `OcctDimension`

### `AddBoss`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape AddBoss(OcctShape baseShape, OcctShape profile, OcctVector3d vector, bool hideInputs = true)
```

**Parameters**

- `baseShape` — `OcctShape`
- `profile` — `OcctShape`
- `vector` — `OcctVector3d`
- `hideInputs` — `bool` = true

**Returns:** `OcctShape`

### `AddDiameterDimension`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctDimension AddDiameterDimension(OcctShape circularShape, double flyout = 20, Color? color = null)
```

**Parameters**

- `circularShape` — `OcctShape`
- `flyout` — `double` = 20
- `color` — `Color?` = null

**Returns:** `OcctDimension`

### `AddLengthDimension`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctDimension AddLengthDimension(OcctShape edge, double flyout = 20, Color? color = null)
```

**Parameters**

- `edge` — `OcctShape`
- `flyout` — `double` = 20
- `color` — `Color?` = null

**Returns:** `OcctDimension`

### `AddPocket`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape AddPocket(OcctShape baseShape, OcctShape profile, OcctVector3d vector, bool hideInputs = true)
```

**Parameters**

- `baseShape` — `OcctShape`
- `profile` — `OcctShape`
- `vector` — `OcctVector3d`
- `hideInputs` — `bool` = true

**Returns:** `OcctShape`

### `AddRadiusDimension`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctDimension AddRadiusDimension(OcctShape circularShape, double flyout = 20, Color? color = null)
```

**Parameters**

- `circularShape` — `OcctShape`
- `flyout` — `double` = 20
- `color` — `Color?` = null

**Returns:** `OcctDimension`

### `AddText`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctText AddText(string text, OcctPoint3d position, double height = 16, Color? color = null, bool zoomable = true)
```

**Parameters**

- `text` — `string`
- `position` — `OcctPoint3d`
- `height` — `double` = 16
- `color` — `Color?` = null
- `zoomable` — `bool` = true

**Returns:** `OcctText`

### `ApplyLightingPreset`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ApplyLightingPreset(OcctLightingPreset preset)
```

**Parameters**

- `preset` — `OcctLightingPreset`

**Returns:** `void`

### `AutoZFit`

Recalculates the current camera Z range when automatic Z fitting is enabled.

```csharp
public void AutoZFit()
```

**Returns:** `void`

### `BeginDisplayBatch`

Defers Display, Redisplay and view redraw work until the returned scope is disposed. Use this when creating or changing several objects in one operation.

```csharp
public OcctDisplayBatch BeginDisplayBatch(bool fitAllOnDispose = false)
```

**Parameters**

- `fitAllOnDispose` — `bool` = false

**Returns:** `OcctDisplayBatch`

### `Boolean`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape Boolean(OcctBooleanOperation operation, OcctShape left, OcctShape right, bool hideInputs = true)
```

**Parameters**

- `operation` — `OcctBooleanOperation`
- `left` — `OcctShape`
- `right` — `OcctShape`
- `hideInputs` — `bool` = true

**Returns:** `OcctShape`

### `ChamferAllEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape ChamferAllEdges(OcctShape shape, double distance, bool hideInput = true)
```

**Parameters**

- `shape` — `OcctShape`
- `distance` — `double`
- `hideInput` — `bool` = true

**Returns:** `OcctShape`

### `ChamferEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape ChamferEdges(OcctShape shape, IEnumerable<int> edgeIndices, double distance, bool hideInput = true)
```

**Parameters**

- `shape` — `OcctShape`
- `edgeIndices` — `IEnumerable<int>`
- `distance` — `double`
- `hideInput` — `bool` = true

**Returns:** `OcctShape`

### `Clear`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Clear()
```

**Returns:** `void`

### `ClearSelection`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ClearSelection()
```

**Returns:** `void`

### `Common`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape Common(OcctShape left, OcctShape right, bool hideInputs = true)
```

**Parameters**

- `left` — `OcctShape`
- `right` — `OcctShape`
- `hideInputs` — `bool` = true

**Returns:** `OcctShape`

### `Copy`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape Copy(OcctShape shape, bool hideInput = false)
```

**Parameters**

- `shape` — `OcctShape`
- `hideInput` — `bool` = false

**Returns:** `OcctShape`

### `CopySelectedSubshape`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape CopySelectedSubshape()
```

**Returns:** `OcctShape`

### `CopySelectedSubshape`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape CopySelectedSubshape(int index)
```

**Parameters**

- `index` — `int`

**Returns:** `OcctShape`

### `Cut`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape Cut(OcctShape left, OcctShape right, bool hideInputs = true)
```

**Parameters**

- `left` — `OcctShape`
- `right` — `OcctShape`
- `hideInputs` — `bool` = true

**Returns:** `OcctShape`

### `Delete`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Delete(IEnumerable<IOcctObject> values)
```

**Parameters**

- `values` — `IEnumerable<IOcctObject>`

**Returns:** `void`

### `Delete`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Delete(IOcctObject value)
```

**Parameters**

- `value` — `IOcctObject`

**Returns:** `void`

### `Display`

Copies a headless modeling shape into this initialized AIS engine and displays it. The returned shape belongs to this OcctNet.OcctEngine instance.

```csharp
public OcctShape Display(OcctModelingSession model, OcctModelShape shape, bool fit = false)
```

**Parameters**

- `model` — `OcctModelingSession`
- `shape` — `OcctModelShape`
- `fit` — `bool` = false

**Returns:** `OcctShape`

### `Dispose`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Dispose()
```

**Returns:** `void`

### `DrillHole`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape DrillHole(OcctShape baseShape, OcctPoint3d origin, OcctVector3d axis, double radius, double depth, bool hideInput = true)
```

**Parameters**

- `baseShape` — `OcctShape`
- `origin` — `OcctPoint3d`
- `axis` — `OcctVector3d`
- `radius` — `double`
- `depth` — `double`
- `hideInput` — `bool` = true

**Returns:** `OcctShape`

### `DumpView`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void DumpView(string filePath)
```

**Parameters**

- `filePath` — `string`

**Returns:** `void`

### `EvaluateEdge`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctEdgeEvaluation EvaluateEdge(OcctShape edge, double normalizedParameter)
```

**Parameters**

- `edge` — `OcctShape`
- `normalizedParameter` — `double`

**Returns:** `OcctEdgeEvaluation`

### `EvaluateFace`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctFaceEvaluation EvaluateFace(OcctShape face, double u, double v)
```

**Parameters**

- `face` — `OcctShape`
- `u` — `double`
- `v` — `double`

**Returns:** `OcctFaceEvaluation`

### `Exists`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Exists(IOcctObject value)
```

**Parameters**

- `value` — `IOcctObject`

**Returns:** `bool`

### `ExportAllIges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ExportAllIges(string filePath)
```

**Parameters**

- `filePath` — `string`

**Returns:** `void`

### `ExportAllStep`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ExportAllStep(string filePath)
```

**Parameters**

- `filePath` — `string`

**Returns:** `void`

### `ExportBrep`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ExportBrep(OcctShape shape, string filePath)
```

**Parameters**

- `shape` — `OcctShape`
- `filePath` — `string`

**Returns:** `void`

### `ExportIges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ExportIges(OcctShape shape, string filePath)
```

**Parameters**

- `shape` — `OcctShape`
- `filePath` — `string`

**Returns:** `void`

### `ExportStep`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ExportStep(OcctShape shape, string filePath)
```

**Parameters**

- `shape` — `OcctShape`
- `filePath` — `string`

**Returns:** `void`

### `ExportStl`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ExportStl(OcctShape shape, string filePath, double linearDeflection = 0.1, double angularDeflection = 0.5, bool ascii = false)
```

**Parameters**

- `shape` — `OcctShape`
- `filePath` — `string`
- `linearDeflection` — `double` = 0.1
- `angularDeflection` — `double` = 0.5
- `ascii` — `bool` = false

**Returns:** `void`

### `Extrude`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape Extrude(OcctShape profile, OcctVector3d vector, bool hideInput = true)
```

**Parameters**

- `profile` — `OcctShape`
- `vector` — `OcctVector3d`
- `hideInput` — `bool` = true

**Returns:** `OcctShape`

### `FilletAllEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape FilletAllEdges(OcctShape shape, double radius, bool hideInput = true)
```

**Parameters**

- `shape` — `OcctShape`
- `radius` — `double`
- `hideInput` — `bool` = true

**Returns:** `OcctShape`

### `FilletEdges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape FilletEdges(OcctShape shape, IEnumerable<int> edgeIndices, double radius, bool hideInput = true)
```

**Parameters**

- `shape` — `OcctShape`
- `edgeIndices` — `IEnumerable<int>`
- `radius` — `double`
- `hideInput` — `bool` = true

**Returns:** `OcctShape`

### `Fit`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Fit(IEnumerable<OcctShape> shapes, double margin = 0.05)
```

**Parameters**

- `shapes` — `IEnumerable<OcctShape>`
- `margin` — `double` = 0.05

**Returns:** `void`

### `Fit`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Fit(OcctShape shape)
```

**Parameters**

- `shape` — `OcctShape`

**Returns:** `void`

### `FitAll`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void FitAll()
```

**Returns:** `void`

### `FitSelected`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void FitSelected(double margin = 0.05)
```

**Parameters**

- `margin` — `double` = 0.05

**Returns:** `void`

### `Fuse`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape Fuse(OcctShape left, OcctShape right, bool hideInputs = true)
```

**Parameters**

- `left` — `OcctShape`
- `right` — `OcctShape`
- `hideInputs` — `bool` = true

**Returns:** `OcctShape`

### `GetApplicationTag`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public string GetApplicationTag(IOcctObject value)
```

**Parameters**

- `value` — `IOcctObject`

**Returns:** `string`

### `GetAutoZFitSettings`

Returns the current automatic Z-range fitting settings.

```csharp
public OcctAutoZFitSettings GetAutoZFitSettings()
```

**Returns:** `OcctAutoZFitSettings`

### `GetCamera`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctCameraState GetCamera()
```

**Returns:** `OcctCameraState`

### `GetDefaultPolygonOffsets`

Returns the polygon offset configured on the Viewer default drawer.

```csharp
public OcctPolygonOffsetSettings GetDefaultPolygonOffsets()
```

**Returns:** `OcctPolygonOffsetSettings`

### `GetEdgeCurveType`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctCurveType GetEdgeCurveType(OcctShape edge)
```

**Parameters**

- `edge` — `OcctShape`

**Returns:** `OcctCurveType`

### `GetEdgeEndpoints`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public ValueTuple<OcctPoint3d, OcctPoint3d> GetEdgeEndpoints(OcctShape edge)
```

**Parameters**

- `edge` — `OcctShape`

**Returns:** `ValueTuple<OcctPoint3d, OcctPoint3d>`

### `GetFaceSurfaceType`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctSurfaceType GetFaceSurfaceType(OcctShape face)
```

**Parameters**

- `face` — `OcctShape`

**Returns:** `OcctSurfaceType`

### `GetFaceUvBounds`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctUvBounds GetFaceUvBounds(OcctShape face)
```

**Parameters**

- `face` — `OcctShape`

**Returns:** `OcctUvBounds`

### `GetLocalTransformation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctTransform3d GetLocalTransformation(IOcctObject value)
```

**Parameters**

- `value` — `IOcctObject`

**Returns:** `OcctTransform3d`

### `GetName`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public string GetName(IOcctObject value)
```

**Parameters**

- `value` — `IOcctObject`

**Returns:** `string`

### `GetObject`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IOcctObject GetObject(long id)
```

**Parameters**

- `id` — `long`

**Returns:** `IOcctObject`

### `GetObjectKind`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctObjectKind GetObjectKind(long id)
```

**Parameters**

- `id` — `long`

**Returns:** `OcctObjectKind`

### `GetPolygonOffsets`

Returns the effective polygon offset for a Viewer object.

```csharp
public OcctPolygonOffsetSettings GetPolygonOffsets(IOcctObject value)
```

**Parameters**

- `value` — `IOcctObject`

**Returns:** `OcctPolygonOffsetSettings`

### `GetSceneGravityPoint`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d GetSceneGravityPoint()
```

**Returns:** `OcctPoint3d`

### `GetSelectedHits`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctSelectionHit> GetSelectedHits()
```

**Returns:** `IReadOnlyList<OcctSelectionHit>`

### `GetSelectedObjects`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<IOcctObject> GetSelectedObjects()
```

**Returns:** `IReadOnlyList<IOcctObject>`

### `GetShape`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape GetShape(long id)
```

**Parameters**

- `id` — `long`

**Returns:** `OcctShape`

### `GetShapeBounds`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctBounds GetShapeBounds(OcctShape shape)
```

**Parameters**

- `shape` — `OcctShape`

**Returns:** `OcctBounds`

### `GetShapeDistance`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctDistanceResult GetShapeDistance(OcctShape first, OcctShape second)
```

**Parameters**

- `first` — `OcctShape`
- `second` — `OcctShape`

**Returns:** `OcctDistanceResult`

### `GetShapeHash`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public long GetShapeHash(OcctShape shape)
```

**Parameters**

- `shape` — `OcctShape`

**Returns:** `long`

### `GetShapeLinearProperties`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctMassProperties GetShapeLinearProperties(OcctShape shape)
```

**Parameters**

- `shape` — `OcctShape`

**Returns:** `OcctMassProperties`

### `GetShapeSurfaceProperties`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctMassProperties GetShapeSurfaceProperties(OcctShape shape)
```

**Parameters**

- `shape` — `OcctShape`

**Returns:** `OcctMassProperties`

### `GetShapeType`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShapeType GetShapeType(OcctShape shape)
```

**Parameters**

- `shape` — `OcctShape`

**Returns:** `OcctShapeType`

### `GetShapeVolumeProperties`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctMassProperties GetShapeVolumeProperties(OcctShape shape)
```

**Parameters**

- `shape` — `OcctShape`

**Returns:** `OcctMassProperties`

### `GetSubshapeAt`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape GetSubshapeAt(OcctShape shape, OcctShapeType type, int index)
```

**Parameters**

- `shape` — `OcctShape`
- `type` — `OcctShapeType`
- `index` — `int`

**Returns:** `OcctShape`

### `GetSubshapes`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctShape> GetSubshapes(OcctShape shape, OcctShapeType type)
```

**Parameters**

- `shape` — `OcctShape`
- `type` — `OcctShapeType`

**Returns:** `IReadOnlyList<OcctShape>`

### `GetTopologyCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int GetTopologyCount(OcctShape shape, OcctShapeType type)
```

**Parameters**

- `shape` — `OcctShape`
- `type` — `OcctShapeType`

**Returns:** `int`

### `GetVertexPoint`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d GetVertexPoint(OcctShape vertex)
```

**Parameters**

- `vertex` — `OcctShape`

**Returns:** `OcctPoint3d`

### `GetViewportState`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctViewportState GetViewportState()
```

**Returns:** `OcctViewportState`

### `HasLocalTransformation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool HasLocalTransformation(IOcctObject value)
```

**Parameters**

- `value` — `IOcctObject`

**Returns:** `bool`

### `HideAll`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void HideAll()
```

**Returns:** `void`

### `HideSelected`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void HideSelected()
```

**Returns:** `void`

### `HideSelectionRectangle`

Removes the OCCT-native rubber-band selection overlay.

```csharp
public void HideSelectionRectangle()
```

**Returns:** `void`

### `Highlight`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Highlight(IOcctObject value)
```

**Parameters**

- `value` — `IOcctObject`

**Returns:** `void`

### `Import`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape Import(string filePath)
```

**Parameters**

- `filePath` — `string`

**Returns:** `OcctShape`

### `ImportBrep`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape ImportBrep(string filePath)
```

**Parameters**

- `filePath` — `string`

**Returns:** `OcctShape`

### `ImportIges`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape ImportIges(string filePath)
```

**Parameters**

- `filePath` — `string`

**Returns:** `OcctShape`

### `ImportStep`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape ImportStep(string filePath)
```

**Parameters**

- `filePath` — `string`

**Returns:** `OcctShape`

### `ImportStl`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape ImportStl(string filePath)
```

**Parameters**

- `filePath` — `string`

**Returns:** `OcctShape`

### `Initialize`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Initialize(IntPtr windowHandle)
```

**Parameters**

- `windowHandle` — `IntPtr`

**Returns:** `void`

### `InvertSelection`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void InvertSelection()
```

**Returns:** `void`

### `IsSelectable`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsSelectable(IOcctObject value)
```

**Parameters**

- `value` — `IOcctObject`

**Returns:** `bool`

### `IsSelected`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsSelected(IOcctObject value)
```

**Parameters**

- `value` — `IOcctObject`

**Returns:** `bool`

### `IsShapeValid`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsShapeValid(OcctShape shape)
```

**Parameters**

- `shape` — `OcctShape`

**Returns:** `bool`

### `IsVisible`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsVisible(IOcctObject value)
```

**Parameters**

- `value` — `IOcctObject`

**Returns:** `bool`

### `Loft`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape Loft(IEnumerable<OcctShape> sectionWires, bool makeSolid = true, bool ruled = false, double tolerance = 1E-06, bool hideInputs = true)
```

**Parameters**

- `sectionWires` — `IEnumerable<OcctShape>`
- `makeSolid` — `bool` = true
- `ruled` — `bool` = false
- `tolerance` — `double` = 1E-06
- `hideInputs` — `bool` = true

**Returns:** `OcctShape`

### `MakeAngleAnnotationShape`

Creates a result-only BRep angular annotation, including vector text and arrows.

```csharp
public OcctShape MakeAngleAnnotationShape(OcctShape firstEdge, OcctShape secondEdge, double radius = 30, double textHeight = 8, double arrowSize = 5, string fontName = null)
```

**Parameters**

- `firstEdge` — `OcctShape`
- `secondEdge` — `OcctShape`
- `radius` — `double` = 30
- `textHeight` — `double` = 8
- `arrowSize` — `double` = 5
- `fontName` — `string` = null

**Returns:** `OcctShape`

### `MakeArc`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeArc(OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees)
```

**Parameters**

- `center` — `OcctPoint3d`
- `normal` — `OcctVector3d`
- `xDirection` — `OcctVector3d`
- `radius` — `double`
- `startAngleDegrees` — `double`
- `endAngleDegrees` — `double`

**Returns:** `OcctShape`

### `MakeArc`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeArc(OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end)
```

**Parameters**

- `start` — `OcctPoint3d`
- `middle` — `OcctPoint3d`
- `end` — `OcctPoint3d`

**Returns:** `OcctShape`

### `MakeBezier`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeBezier(IEnumerable<OcctPoint3d> poles)
```

**Parameters**

- `poles` — `IEnumerable<OcctPoint3d>`

**Returns:** `OcctShape`

### `MakeBox`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeBox(double dx, double dy, double dz, double x = 0, double y = 0, double z = 0)
```

**Parameters**

- `dx` — `double`
- `dy` — `double`
- `dz` — `double`
- `x` — `double` = 0
- `y` — `double` = 0
- `z` — `double` = 0

**Returns:** `OcctShape`

### `MakeCircle`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeCircle(OcctPoint3d center, OcctVector3d normal, double radius)
```

**Parameters**

- `center` — `OcctPoint3d`
- `normal` — `OcctVector3d`
- `radius` — `double`

**Returns:** `OcctShape`

### `MakeCompound`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeCompound(IEnumerable<OcctShape> shapes, bool hideInputs = false)
```

**Parameters**

- `shapes` — `IEnumerable<OcctShape>`
- `hideInputs` — `bool` = false

**Returns:** `OcctShape`

### `MakeCone`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeCone(double radius1, double radius2, double height, double x = 0, double y = 0, double z = 0)
```

**Parameters**

- `radius1` — `double`
- `radius2` — `double`
- `height` — `double`
- `x` — `double` = 0
- `y` — `double` = 0
- `z` — `double` = 0

**Returns:** `OcctShape`

### `MakeCone`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeCone(OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height)
```

**Parameters**

- `origin` — `OcctPoint3d`
- `axis` — `OcctVector3d`
- `radius1` — `double`
- `radius2` — `double`
- `height` — `double`

**Returns:** `OcctShape`

### `MakeCylinder`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeCylinder(double radius, double height, double x = 0, double y = 0, double z = 0)
```

**Parameters**

- `radius` — `double`
- `height` — `double`
- `x` — `double` = 0
- `y` — `double` = 0
- `z` — `double` = 0

**Returns:** `OcctShape`

### `MakeCylinder`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeCylinder(OcctPoint3d origin, OcctVector3d axis, double radius, double height)
```

**Parameters**

- `origin` — `OcctPoint3d`
- `axis` — `OcctVector3d`
- `radius` — `double`
- `height` — `double`

**Returns:** `OcctShape`

### `MakeDiameterAnnotationShape`

Creates a result-only BRep diameter annotation, including vector text and arrows.

```csharp
public OcctShape MakeDiameterAnnotationShape(OcctShape circularEdge, double flyout = 20, double textHeight = 8, double arrowSize = 5, string fontName = null)
```

**Parameters**

- `circularEdge` — `OcctShape`
- `flyout` — `double` = 20
- `textHeight` — `double` = 8
- `arrowSize` — `double` = 5
- `fontName` — `string` = null

**Returns:** `OcctShape`

### `MakeEllipse`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeEllipse(OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius)
```

**Parameters**

- `center` — `OcctPoint3d`
- `normal` — `OcctVector3d`
- `majorRadius` — `double`
- `minorRadius` — `double`

**Returns:** `OcctShape`

### `MakeFace`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeFace(OcctShape wire, bool onlyPlane = true)
```

**Parameters**

- `wire` — `OcctShape`
- `onlyPlane` — `bool` = true

**Returns:** `OcctShape`

### `MakeInterpolatedBSpline`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeInterpolatedBSpline(IEnumerable<OcctPoint3d> points, bool periodic = false, double tolerance = 1E-07)
```

**Parameters**

- `points` — `IEnumerable<OcctPoint3d>`
- `periodic` — `bool` = false
- `tolerance` — `double` = 1E-07

**Returns:** `OcctShape`

### `MakeLengthAnnotationShape`

Creates a result-only BRep linear annotation, including vector text and arrows.

```csharp
public OcctShape MakeLengthAnnotationShape(OcctShape edge, double flyout = 20, double textHeight = 8, double arrowSize = 5, string fontName = null)
```

**Parameters**

- `edge` — `OcctShape`
- `flyout` — `double` = 20
- `textHeight` — `double` = 8
- `arrowSize` — `double` = 5
- `fontName` — `string` = null

**Returns:** `OcctShape`

### `MakeLine`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeLine(OcctPoint3d start, OcctPoint3d end)
```

**Parameters**

- `start` — `OcctPoint3d`
- `end` — `OcctPoint3d`

**Returns:** `OcctShape`

### `MakePlaneFace`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakePlaneFace(double width, double height, OcctPoint3d? origin = null, OcctVector3d? xDirection = null, OcctVector3d? normal = null)
```

**Parameters**

- `width` — `double`
- `height` — `double`
- `origin` — `OcctPoint3d?` = null
- `xDirection` — `OcctVector3d?` = null
- `normal` — `OcctVector3d?` = null

**Returns:** `OcctShape`

### `MakePolyline`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakePolyline(IEnumerable<OcctPoint3d> points, bool closed = false)
```

**Parameters**

- `points` — `IEnumerable<OcctPoint3d>`
- `closed` — `bool` = false

**Returns:** `OcctShape`

### `MakeRadiusAnnotationShape`

Creates a result-only BRep radius annotation, including vector text and an arrow.

```csharp
public OcctShape MakeRadiusAnnotationShape(OcctShape circularEdge, double flyout = 20, double textHeight = 8, double arrowSize = 5, string fontName = null)
```

**Parameters**

- `circularEdge` — `OcctShape`
- `flyout` — `double` = 20
- `textHeight` — `double` = 8
- `arrowSize` — `double` = 5
- `fontName` — `string` = null

**Returns:** `OcctShape`

### `MakeRectangleWire`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeRectangleWire(double width, double height, OcctPoint3d? origin = null, OcctVector3d? xDirection = null, OcctVector3d? normal = null)
```

**Parameters**

- `width` — `double`
- `height` — `double`
- `origin` — `OcctPoint3d?` = null
- `xDirection` — `OcctVector3d?` = null
- `normal` — `OcctVector3d?` = null

**Returns:** `OcctShape`

### `MakeRegularPolygon`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeRegularPolygon(double radius, int sideCount, bool makeFace = false, OcctPoint3d? center = null, OcctVector3d? normal = null, OcctVector3d? xDirection = null)
```

**Parameters**

- `radius` — `double`
- `sideCount` — `int`
- `makeFace` — `bool` = false
- `center` — `OcctPoint3d?` = null
- `normal` — `OcctVector3d?` = null
- `xDirection` — `OcctVector3d?` = null

**Returns:** `OcctShape`

### `MakeSolidFromShell`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeSolidFromShell(OcctShape shell, bool hideInput = false)
```

**Parameters**

- `shell` — `OcctShape`
- `hideInput` — `bool` = false

**Returns:** `OcctShape`

### `MakeSphere`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeSphere(double radius, double x = 0, double y = 0, double z = 0)
```

**Parameters**

- `radius` — `double`
- `x` — `double` = 0
- `y` — `double` = 0
- `z` — `double` = 0

**Returns:** `OcctShape`

### `MakeTextShape`

Creates vector BRep text that remains geometrically sharp at any zoom level.

```csharp
public OcctShape MakeTextShape(string text, OcctPoint3d position, double height = 16, double extrusionDepth = 0, string fontName = null, OcctVector3d? normal = null, OcctVector3d? xDirection = null, bool bold = false, bool italic = false)
```

**Parameters**

- `text` — `string`
- `position` — `OcctPoint3d`
- `height` — `double` = 16
- `extrusionDepth` — `double` = 0
- `fontName` — `string` = null
- `normal` — `OcctVector3d?` = null
- `xDirection` — `OcctVector3d?` = null
- `bold` — `bool` = false
- `italic` — `bool` = false

**Returns:** `OcctShape`

### `MakeThickSolid`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeThickSolid(OcctShape solid, int faceIndexToRemove, double thickness, double tolerance = 0.0001, bool hideInput = true)
```

**Parameters**

- `solid` — `OcctShape`
- `faceIndexToRemove` — `int`
- `thickness` — `double`
- `tolerance` — `double` = 0.0001
- `hideInput` — `bool` = true

**Returns:** `OcctShape`

### `MakeTorus`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeTorus(double majorRadius, double minorRadius, OcctPoint3d? center = null, OcctVector3d? axis = null)
```

**Parameters**

- `majorRadius` — `double`
- `minorRadius` — `double`
- `center` — `OcctPoint3d?` = null
- `axis` — `OcctVector3d?` = null

**Returns:** `OcctShape`

### `MakeVertex`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeVertex(OcctPoint3d point)
```

**Parameters**

- `point` — `OcctPoint3d`

**Returns:** `OcctShape`

### `MakeWedge`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeWedge(double dx, double dy, double dz, double ltx)
```

**Parameters**

- `dx` — `double`
- `dy` — `double`
- `dz` — `double`
- `ltx` — `double`

**Returns:** `OcctShape`

### `MakeWire`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MakeWire(IEnumerable<OcctShape> edges, bool hideInputs = false)
```

**Parameters**

- `edges` — `IEnumerable<OcctShape>`
- `hideInputs` — `bool` = false

**Returns:** `OcctShape`

### `MirrorPlane`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape MirrorPlane(OcctShape shape, OcctPoint3d planePoint, OcctVector3d planeNormal, bool hideInput = false)
```

**Parameters**

- `shape` — `OcctShape`
- `planePoint` — `OcctPoint3d`
- `planeNormal` — `OcctVector3d`
- `hideInput` — `bool` = false

**Returns:** `OcctShape`

### `MoveTo`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void MoveTo(int x, int y)
```

**Parameters**

- `x` — `int`
- `y` — `int`

**Returns:** `void`

### `Offset`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape Offset(OcctShape shape, double offset, double tolerance = 0.0001, bool hideInput = true)
```

**Parameters**

- `shape` — `OcctShape`
- `offset` — `double`
- `tolerance` — `double` = 0.0001
- `hideInput` — `bool` = true

**Returns:** `OcctShape`

### `Owns`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Owns(IOcctObject value)
```

**Parameters**

- `value` — `IOcctObject`

**Returns:** `bool`

### `Pan`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Pan(int deltaX, int deltaY)
```

**Parameters**

- `deltaX` — `int`
- `deltaY` — `int`

**Returns:** `void`

### `Redisplay`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Redisplay(IEnumerable<IOcctObject> values)
```

**Parameters**

- `values` — `IEnumerable<IOcctObject>`

**Returns:** `void`

### `Redisplay`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Redisplay(IOcctObject value)
```

**Parameters**

- `value` — `IOcctObject`

**Returns:** `void`

### `Redraw`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Redraw()
```

**Returns:** `void`

### `ResetLocalTransformation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ResetLocalTransformation(IOcctObject value)
```

**Parameters**

- `value` — `IOcctObject`

**Returns:** `void`

### `ResetPolygonOffsets`

Restores a Viewer object's polygon offset to the current default drawer values.

```csharp
public void ResetPolygonOffsets(IOcctObject value)
```

**Parameters**

- `value` — `IOcctObject`

**Returns:** `void`

### `ResetSceneLighting`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ResetSceneLighting()
```

**Returns:** `void`

### `ResetView`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ResetView()
```

**Returns:** `void`

### `ResetViewMapping`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ResetViewMapping()
```

**Returns:** `void`

### `ResetViewOrientation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ResetViewOrientation()
```

**Returns:** `void`

### `Resize`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Resize()
```

**Returns:** `void`

### `Revolve`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape Revolve(OcctShape profile, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees = 360, bool hideInput = true)
```

**Parameters**

- `profile` — `OcctShape`
- `axisPoint` — `OcctPoint3d`
- `axisDirection` — `OcctVector3d`
- `angleDegrees` — `double` = 360
- `hideInput` — `bool` = true

**Returns:** `OcctShape`

### `Rotate`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape Rotate(OcctShape shape, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, bool hideInput = false)
```

**Parameters**

- `shape` — `OcctShape`
- `axisPoint` — `OcctPoint3d`
- `axisDirection` — `OcctVector3d`
- `angleDegrees` — `double`
- `hideInput` — `bool` = false

**Returns:** `OcctShape`

### `Rotation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Rotation(int x, int y)
```

**Parameters**

- `x` — `int`
- `y` — `int`

**Returns:** `void`

### `Scale`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape Scale(OcctShape shape, OcctPoint3d center, double factor, bool hideInput = false)
```

**Parameters**

- `shape` — `OcctShape`
- `center` — `OcctPoint3d`
- `factor` — `double`
- `hideInput` — `bool` = false

**Returns:** `OcctShape`

### `ScreenToPlane`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d ScreenToPlane(int x, int y, OcctPoint3d planePoint, OcctVector3d planeNormal)
```

**Parameters**

- `x` — `int`
- `y` — `int`
- `planePoint` — `OcctPoint3d`
- `planeNormal` — `OcctVector3d`

**Returns:** `OcctPoint3d`

### `ScreenToRay`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctProjectionRay ScreenToRay(int x, int y)
```

**Parameters**

- `x` — `int`
- `y` — `int`

**Returns:** `OcctProjectionRay`

### `ScreenToWorld`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d ScreenToWorld(int x, int y)
```

**Parameters**

- `x` — `int`
- `y` — `int`

**Returns:** `OcctPoint3d`

### `Section`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape Section(OcctShape left, OcctShape right, bool hideInputs = false)
```

**Parameters**

- `left` — `OcctShape`
- `right` — `OcctShape`
- `hideInputs` — `bool` = false

**Returns:** `OcctShape`

### `Select`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Select(int x, int y, bool appendSelection = false)
```

**Parameters**

- `x` — `int`
- `y` — `int`
- `appendSelection` — `bool` = false

**Returns:** `void`

### `SelectAllVisible`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SelectAllVisible()
```

**Returns:** `void`

### `SelectObject`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SelectObject(IOcctObject value, bool appendSelection = false)
```

**Parameters**

- `value` — `IOcctObject`
- `appendSelection` — `bool` = false

**Returns:** `void`

### `SelectObjects`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SelectObjects(IEnumerable<IOcctObject> values, bool appendSelection = false)
```

**Parameters**

- `values` — `IEnumerable<IOcctObject>`
- `appendSelection` — `bool` = false

**Returns:** `void`

### `SelectRectangle`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SelectRectangle(int x1, int y1, int x2, int y2, bool appendSelection = false, bool allowOverlap = false)
```

**Parameters**

- `x1` — `int`
- `y1` — `int`
- `x2` — `int`
- `y2` — `int`
- `appendSelection` — `bool` = false
- `allowOverlap` — `bool` = false

**Returns:** `void`

### `SetAntialiasing`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetAntialiasing(bool enabled)
```

**Parameters**

- `enabled` — `bool`

**Returns:** `void`

### `SetApplicationTag`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetApplicationTag(IOcctObject value, string applicationTag)
```

**Parameters**

- `value` — `IOcctObject`
- `applicationTag` — `string`

**Returns:** `void`

### `SetAutoZFitMode`

Enables or disables automatic adjustment of the camera Z range. This improves depth precision and prevents clipping, but it does not separate two coplanar objects.

```csharp
public void SetAutoZFitMode(bool enabled, double scaleFactor = 1)
```

**Parameters**

- `enabled` — `bool`
- `scaleFactor` — `double` = 1

**Returns:** `void`

### `SetAutomaticHighlight`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetAutomaticHighlight(bool enabled)
```

**Parameters**

- `enabled` — `bool`

**Returns:** `void`

### `SetBackground`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetBackground(Color color)
```

**Parameters**

- `color` — `Color`

**Returns:** `void`

### `SetCamera`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetCamera(OcctCameraState state)
```

**Parameters**

- `state` — `OcctCameraState`

**Returns:** `void`

### `SetColor`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetColor(IEnumerable<IOcctObject> values, Color color)
```

**Parameters**

- `values` — `IEnumerable<IOcctObject>`
- `color` — `Color`

**Returns:** `void`

### `SetColor`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetColor(IOcctObject value, Color color)
```

**Parameters**

- `value` — `IOcctObject`
- `color` — `Color`

**Returns:** `void`

### `SetComputedHlr`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetComputedHlr(bool enabled)
```

**Parameters**

- `enabled` — `bool`

**Returns:** `void`

### `SetDefaultMaterial`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetDefaultMaterial(OcctMaterial material, bool applyExisting = false)
```

**Parameters**

- `material` — `OcctMaterial`
- `applyExisting` — `bool` = false

**Returns:** `void`

### `SetDefaultPolygonOffsets`

Changes the default polygon offset inherited by future Viewer objects. OCCT's recommended shaded-view baseline is Fill, factor 1, units 1.

```csharp
public void SetDefaultPolygonOffsets(OcctPolygonOffsetMode mode, double factor = 1, double units = 1, bool applyExisting = false)
```

**Parameters**

- `mode` — `OcctPolygonOffsetMode`
- `factor` — `double` = 1
- `units` — `double` = 1
- `applyExisting` — `bool` = false

**Returns:** `void`

### `SetDimensionFlyout`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetDimensionFlyout(OcctDimension dimension, double flyout)
```

**Parameters**

- `dimension` — `OcctDimension`
- `flyout` — `double`

**Returns:** `void`

### `SetDisplayMode`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetDisplayMode(IEnumerable<IOcctObject> values, OcctDisplayMode displayMode)
```

**Parameters**

- `values` — `IEnumerable<IOcctObject>`
- `displayMode` — `OcctDisplayMode`

**Returns:** `void`

### `SetDisplayMode`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetDisplayMode(IOcctObject value, OcctDisplayMode displayMode)
```

**Parameters**

- `value` — `IOcctObject`
- `displayMode` — `OcctDisplayMode`

**Returns:** `void`

### `SetDisplayMode`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetDisplayMode(OcctDisplayMode displayMode)
```

**Parameters**

- `displayMode` — `OcctDisplayMode`

**Returns:** `void`

### `SetDisplayPrecision`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetDisplayPrecision(double deviationCoefficient, double deviationAngleDegrees, bool applyExisting = true)
```

**Parameters**

- `deviationCoefficient` — `double`
- `deviationAngleDegrees` — `double`
- `applyExisting` — `bool` = true

**Returns:** `void`

### `SetFaceBoundariesVisible`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetFaceBoundariesVisible(bool visible, bool applyExisting = true)
```

**Parameters**

- `visible` — `bool`
- `applyExisting` — `bool` = true

**Returns:** `void`

### `SetFrustumCulling`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetFrustumCulling(bool enabled)
```

**Parameters**

- `enabled` — `bool`

**Returns:** `void`

### `SetGradientBackground`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetGradientBackground(Color first, Color second, OcctGradientFillMethod fillMethod = OcctGradientFillMethod.Vertical)
```

**Parameters**

- `first` — `Color`
- `second` — `Color`
- `fillMethod` — `OcctGradientFillMethod` = OcctGradientFillMethod.Vertical

**Returns:** `void`

### `SetHoverHighlightColor`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetHoverHighlightColor(Color color)
```

**Parameters**

- `color` — `Color`

**Returns:** `void`

### `SetImmediateUpdate`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetImmediateUpdate(bool enabled)
```

**Parameters**

- `enabled` — `bool`

**Returns:** `void`

### `SetLineWidth`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetLineWidth(IEnumerable<IOcctObject> values, double width)
```

**Parameters**

- `values` — `IEnumerable<IOcctObject>`
- `width` — `double`

**Returns:** `void`

### `SetLineWidth`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetLineWidth(IOcctObject value, double width)
```

**Parameters**

- `value` — `IOcctObject`
- `width` — `double`

**Returns:** `void`

### `SetLocalTransformation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetLocalTransformation(IOcctObject value, OcctTransform3d transform)
```

**Parameters**

- `value` — `IOcctObject`
- `transform` — `OcctTransform3d`

**Returns:** `void`

### `SetLocalTransformations`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetLocalTransformations(IReadOnlyList<OcctObjectTransformUpdate> updates)
```

**Parameters**

- `updates` — `IReadOnlyList<OcctObjectTransformUpdate>`

**Returns:** `void`

### `SetMaterial`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetMaterial(IEnumerable<IOcctObject> values, OcctMaterial material)
```

**Parameters**

- `values` — `IEnumerable<IOcctObject>`
- `material` — `OcctMaterial`

**Returns:** `void`

### `SetMaterial`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetMaterial(IOcctObject value, OcctMaterial material)
```

**Parameters**

- `value` — `IOcctObject`
- `material` — `OcctMaterial`

**Returns:** `void`

### `SetMsaaSamples`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetMsaaSamples(int samples)
```

**Parameters**

- `samples` — `int`

**Returns:** `void`

### `SetName`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetName(IOcctObject value, string name)
```

**Parameters**

- `value` — `IOcctObject`
- `name` — `string`

**Returns:** `void`

### `SetPerspectiveFieldOfView`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetPerspectiveFieldOfView(double degrees)
```

**Parameters**

- `degrees` — `double`

**Returns:** `void`

### `SetPolygonOffsets`

Sets a per-object polygon offset. Use a negative Fill offset to draw a coplanar overlay in front of its reference object, or a larger positive value to push it behind.

```csharp
public void SetPolygonOffsets(IOcctObject value, OcctPolygonOffsetMode mode, double factor = 1, double units = 1)
```

**Parameters**

- `value` — `IOcctObject`
- `mode` — `OcctPolygonOffsetMode`
- `factor` — `double` = 1
- `units` — `double` = 1

**Returns:** `void`

### `SetProjection`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetProjection(OcctProjectionType projection)
```

**Parameters**

- `projection` — `OcctProjectionType`

**Returns:** `void`

### `SetRenderResolution`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetRenderResolution(double dpi)
```

**Parameters**

- `dpi` — `double`

**Returns:** `void`

### `SetRenderResolutionScale`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetRenderResolutionScale(double scale)
```

**Parameters**

- `scale` — `double`

**Returns:** `void`

### `SetRenderingMethod`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetRenderingMethod(OcctRenderingMethod method)
```

**Parameters**

- `method` — `OcctRenderingMethod`

**Returns:** `void`

### `SetSceneLighting`

Bridge 2.5 source-compatibility entry point. New code should construct OcctNet.OcctSceneLightingSettings explicitly.

```csharp
public void SetSceneLighting(double ambientIntensity, double directionalIntensity, OcctVector3d direction, bool headlight)
```

**Parameters**

- `ambientIntensity` — `double`
- `directionalIntensity` — `double`
- `direction` — `OcctVector3d`
- `headlight` — `bool`

**Returns:** `void`

### `SetSceneLighting`

Bridge 2.5 source-compatibility entry point. New code should construct OcctNet.OcctSceneLightingSettings explicitly.

```csharp
public void SetSceneLighting(OcctSceneLightingSettings settings)
```

**Parameters**

- `settings` — `OcctSceneLightingSettings`

**Returns:** `void`

### `SetSelectable`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetSelectable(IEnumerable<IOcctObject> values, bool selectable)
```

**Parameters**

- `values` — `IEnumerable<IOcctObject>`
- `selectable` — `bool`

**Returns:** `void`

### `SetSelectable`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetSelectable(IOcctObject value, bool selectable)
```

**Parameters**

- `value` — `IOcctObject`
- `selectable` — `bool`

**Returns:** `void`

### `SetSelection`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetSelection(IEnumerable<IOcctObject> values, OcctSelectionOperation operation = OcctSelectionOperation.Replace)
```

**Parameters**

- `values` — `IEnumerable<IOcctObject>`
- `operation` — `OcctSelectionOperation` = OcctSelectionOperation.Replace

**Returns:** `void`

### `SetSelectionHighlightColor`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetSelectionHighlightColor(Color color)
```

**Parameters**

- `color` — `Color`

**Returns:** `void`

### `SetSelectionMode`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetSelectionMode(OcctSelectionMode mode)
```

**Parameters**

- `mode` — `OcctSelectionMode`

**Returns:** `void`

### `SetSelectionTolerance`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetSelectionTolerance(int pixelTolerance)
```

**Parameters**

- `pixelTolerance` — `int`

**Returns:** `void`

### `SetShadowsEnabled`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetShadowsEnabled(bool enabled)
```

**Parameters**

- `enabled` — `bool`

**Returns:** `void`

### `SetText`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetText(OcctText textObject, string text)
```

**Parameters**

- `textObject` — `OcctText`
- `text` — `string`

**Returns:** `void`

### `SetTextAngle`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetTextAngle(OcctText textObject, double angleDegrees)
```

**Parameters**

- `textObject` — `OcctText`
- `angleDegrees` — `double`

**Returns:** `void`

### `SetTextFont`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetTextFont(OcctText textObject, string fontName)
```

**Parameters**

- `textObject` — `OcctText`
- `fontName` — `string`

**Returns:** `void`

### `SetTextHeight`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetTextHeight(OcctText textObject, double height)
```

**Parameters**

- `textObject` — `OcctText`
- `height` — `double`

**Returns:** `void`

### `SetTextPosition`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetTextPosition(OcctText textObject, OcctPoint3d position)
```

**Parameters**

- `textObject` — `OcctText`
- `position` — `OcctPoint3d`

**Returns:** `void`

### `SetTextZoomable`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetTextZoomable(OcctText textObject, bool zoomable)
```

**Parameters**

- `textObject` — `OcctText`
- `zoomable` — `bool`

**Returns:** `void`

### `SetTransparency`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetTransparency(IEnumerable<IOcctObject> values, double transparency)
```

**Parameters**

- `values` — `IEnumerable<IOcctObject>`
- `transparency` — `double`

**Returns:** `void`

### `SetTransparency`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetTransparency(IOcctObject value, double transparency)
```

**Parameters**

- `value` — `IOcctObject`
- `transparency` — `double`

**Returns:** `void`

### `SetTriedronVisible`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetTriedronVisible(bool visible)
```

**Parameters**

- `visible` — `bool`

**Returns:** `void`

### `SetView`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetView(OcctViewOrientation orientation)
```

**Parameters**

- `orientation` — `OcctViewOrientation`

**Returns:** `void`

### `SetViewCubeLanguage`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetViewCubeLanguage(OcctViewCubeLanguage language)
```

**Parameters**

- `language` — `OcctViewCubeLanguage`

**Returns:** `void`

### `SetViewCubeVisible`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetViewCubeVisible(bool visible)
```

**Parameters**

- `visible` — `bool`

**Returns:** `void`

### `SetVisible`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetVisible(IEnumerable<IOcctObject> values, bool visible)
```

**Parameters**

- `values` — `IEnumerable<IOcctObject>`
- `visible` — `bool`

**Returns:** `void`

### `SetVisible`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetVisible(IOcctObject value, bool visible)
```

**Parameters**

- `value` — `IOcctObject`
- `visible` — `bool`

**Returns:** `void`

### `SetZUpView`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void SetZUpView(OcctZUpViewOrientation orientation, bool fitAll = true)
```

**Parameters**

- `orientation` — `OcctZUpViewOrientation`
- `fitAll` — `bool` = true

**Returns:** `void`

### `Sew`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape Sew(IEnumerable<OcctShape> shapes, double tolerance = 1E-06, bool hideInputs = false)
```

**Parameters**

- `shapes` — `IEnumerable<OcctShape>`
- `tolerance` — `double` = 1E-06
- `hideInputs` — `bool` = false

**Returns:** `OcctShape`

### `ShowAll`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ShowAll()
```

**Returns:** `void`

### `ShowSelectionRectangle`

Displays or updates an OCCT-native 2D rubber-band rectangle in the top overlay layer. Coordinates use window client pixels with the origin at the upper-left corner.

```csharp
public void ShowSelectionRectangle(int x1, int y1, int x2, int y2, Color lineColor, Color fillColor, double fillTransparency = 0.82, double lineWidth = 1)
```

**Parameters**

- `x1` — `int`
- `y1` — `int`
- `x2` — `int`
- `y2` — `int`
- `lineColor` — `Color`
- `fillColor` — `Color`
- `fillTransparency` — `double` = 0.82
- `lineWidth` — `double` = 1

**Returns:** `void`

### `StartRotation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void StartRotation(int x, int y)
```

**Parameters**

- `x` — `int`
- `y` — `int`

**Returns:** `void`

### `Sweep`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape Sweep(OcctShape spineWire, OcctShape profile, bool hideInputs = true)
```

**Parameters**

- `spineWire` — `OcctShape`
- `profile` — `OcctShape`
- `hideInputs` — `bool` = true

**Returns:** `OcctShape`

### `Translate`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape Translate(OcctShape shape, OcctVector3d vector, bool hideInput = false)
```

**Parameters**

- `shape` — `OcctShape`
- `vector` — `OcctVector3d`
- `hideInput` — `bool` = false

**Returns:** `OcctShape`

### `TryGetDetectedHit`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool TryGetDetectedHit(out OcctSelectionHit hit)
```

**Parameters**

- `hit` — `out OcctSelectionHit`

**Returns:** `bool`

### `TryGetObject`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool TryGetObject(long id, out IOcctObject value)
```

**Parameters**

- `id` — `long`
- `value` — `out IOcctObject`

**Returns:** `bool`

### `TryGetObjectByApplicationTag`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool TryGetObjectByApplicationTag(string applicationTag, out IOcctObject value)
```

**Parameters**

- `applicationTag` — `string`
- `value` — `out IOcctObject`

**Returns:** `bool`

### `TryGetShape`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool TryGetShape(long id, out OcctShape shape)
```

**Parameters**

- `id` — `long`
- `shape` — `out OcctShape`

**Returns:** `bool`

### `TryScreenToPlane`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool TryScreenToPlane(int x, int y, OcctPoint3d planePoint, OcctVector3d planeNormal, out OcctPoint3d result)
```

**Parameters**

- `x` — `int`
- `y` — `int`
- `planePoint` — `OcctPoint3d`
- `planeNormal` — `OcctVector3d`
- `result` — `out OcctPoint3d`

**Returns:** `bool`

### `Unhighlight`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Unhighlight(IOcctObject value)
```

**Parameters**

- `value` — `IOcctObject`

**Returns:** `void`

### `UpdateShape`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void UpdateShape(OcctShape viewerShape, OcctModelingSession sourceSession, OcctModelShape sourceShape, OcctShapeUpdateOptions options = OcctShapeUpdateOptions.PreserveAll)
```

**Parameters**

- `viewerShape` — `OcctShape`
- `sourceSession` — `OcctModelingSession`
- `sourceShape` — `OcctModelShape`
- `options` — `OcctShapeUpdateOptions` = OcctShapeUpdateOptions.PreserveAll

**Returns:** `void`

### `WindowFit`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void WindowFit(int x1, int y1, int x2, int y2)
```

**Parameters**

- `x1` — `int`
- `y1` — `int`
- `x2` — `int`
- `y2` — `int`

**Returns:** `void`

### `WorldToScreen`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public Point WorldToScreen(OcctPoint3d point)
```

**Parameters**

- `point` — `OcctPoint3d`

**Returns:** `Point`

### `Zoom`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Zoom(double factor)
```

**Parameters**

- `factor` — `double`

**Returns:** `void`

### `ZoomAtPoint`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void ZoomAtPoint(int x, int y, double delta)
```

**Parameters**

- `x` — `int`
- `y` — `int`
- `delta` — `double`

**Returns:** `void`

## Fields / Enum Values

None

