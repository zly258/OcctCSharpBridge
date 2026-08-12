# OcctEngine

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 声明

```csharp
public sealed class OcctEngine
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctEngine`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctEngine()
```

## 属性

### `FirstSelected`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape? FirstSelected { get; }
```

### `FirstSelectedObject`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IOcctObject FirstSelectedObject { get; }
```

### `IsDisplayBatchActive`

Returns true while one or more display update batches are active.

```csharp
public bool IsDisplayBatchActive { get; }
```

### `IsDisposed`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsDisposed { get; }
```

### `IsInitialized`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsInitialized { get; }
```

### `ObjectCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int ObjectCount { get; }
```

### `Objects`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<IOcctObject> Objects { get; }
```

### `OcctVersion`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public string OcctVersion { get; }
```

### `SelectedObjects`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<IOcctObject> SelectedObjects { get; }
```

### `ShapeCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int ShapeCount { get; }
```

### `Shapes`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctShape> Shapes { get; }
```

### `ViewScale`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double ViewScale { get; set; }
```

## 事件

无

## 方法

### `AddAngleDimension`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctDimension AddAngleDimension(OcctShape firstEdge, OcctShape secondEdge, double flyout = 20, Color? color = null)
```

**参数**

- `firstEdge` — `OcctShape`
- `secondEdge` — `OcctShape`
- `flyout` — `double` = 20
- `color` — `Color?` = null

**返回值:** `OcctDimension`

### `AddBoss`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape AddBoss(OcctShape baseShape, OcctShape profile, OcctVector3d vector, bool hideInputs = true)
```

**参数**

- `baseShape` — `OcctShape`
- `profile` — `OcctShape`
- `vector` — `OcctVector3d`
- `hideInputs` — `bool` = true

**返回值:** `OcctShape`

### `AddDiameterDimension`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctDimension AddDiameterDimension(OcctShape circularShape, double flyout = 20, Color? color = null)
```

**参数**

- `circularShape` — `OcctShape`
- `flyout` — `double` = 20
- `color` — `Color?` = null

**返回值:** `OcctDimension`

### `AddLengthDimension`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctDimension AddLengthDimension(OcctShape edge, double flyout = 20, Color? color = null)
```

**参数**

- `edge` — `OcctShape`
- `flyout` — `double` = 20
- `color` — `Color?` = null

**返回值:** `OcctDimension`

### `AddPocket`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape AddPocket(OcctShape baseShape, OcctShape profile, OcctVector3d vector, bool hideInputs = true)
```

**参数**

- `baseShape` — `OcctShape`
- `profile` — `OcctShape`
- `vector` — `OcctVector3d`
- `hideInputs` — `bool` = true

**返回值:** `OcctShape`

### `AddPoint`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint AddPoint(OcctPoint3d position, OcctPointMarker marker = OcctPointMarker.CirclePoint, double scale = 3, Color? color = null)
```

**参数**

- `position` — `OcctPoint3d`
- `marker` — `OcctPointMarker` = OcctPointMarker.CirclePoint
- `scale` — `double` = 3
- `color` — `Color?` = null

**返回值:** `OcctPoint`

### `AddRadiusDimension`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctDimension AddRadiusDimension(OcctShape circularShape, double flyout = 20, Color? color = null)
```

**参数**

- `circularShape` — `OcctShape`
- `flyout` — `double` = 20
- `color` — `Color?` = null

**返回值:** `OcctDimension`

### `AddText`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctText AddText(string text, OcctPoint3d position, double height = 16, Color? color = null, bool zoomable = true)
```

**参数**

- `text` — `string`
- `position` — `OcctPoint3d`
- `height` — `double` = 16
- `color` — `Color?` = null
- `zoomable` — `bool` = true

**返回值:** `OcctText`

### `ApplyLightingPreset`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ApplyLightingPreset(OcctLightingPreset preset)
```

**参数**

- `preset` — `OcctLightingPreset`

**返回值:** `void`

### `AutoZFit`

Recalculates the current camera Z range when automatic Z fitting is enabled.

```csharp
public void AutoZFit()
```

**返回值:** `void`

### `BeginDisplayBatch`

Defers Display, Redisplay and view redraw work until the returned scope is disposed. Use this when creating or changing several objects in one operation.

```csharp
public OcctDisplayBatch BeginDisplayBatch(bool fitAllOnDispose = false)
```

**参数**

- `fitAllOnDispose` — `bool` = false

**返回值:** `OcctDisplayBatch`

### `Boolean`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape Boolean(OcctBooleanOperation operation, OcctShape left, OcctShape right, bool hideInputs = true)
```

**参数**

- `operation` — `OcctBooleanOperation`
- `left` — `OcctShape`
- `right` — `OcctShape`
- `hideInputs` — `bool` = true

**返回值:** `OcctShape`

### `ChamferAllEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape ChamferAllEdges(OcctShape shape, double distance, bool hideInput = true)
```

**参数**

- `shape` — `OcctShape`
- `distance` — `double`
- `hideInput` — `bool` = true

**返回值:** `OcctShape`

### `ChamferEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape ChamferEdges(OcctShape shape, IEnumerable<int> edgeIndices, double distance, bool hideInput = true)
```

**参数**

- `shape` — `OcctShape`
- `edgeIndices` — `IEnumerable<int>`
- `distance` — `double`
- `hideInput` — `bool` = true

**返回值:** `OcctShape`

### `Clear`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Clear()
```

**返回值:** `void`

### `ClearSelection`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ClearSelection()
```

**返回值:** `void`

### `Common`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape Common(OcctShape left, OcctShape right, bool hideInputs = true)
```

**参数**

- `left` — `OcctShape`
- `right` — `OcctShape`
- `hideInputs` — `bool` = true

**返回值:** `OcctShape`

### `Copy`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape Copy(OcctShape shape, bool hideInput = false)
```

**参数**

- `shape` — `OcctShape`
- `hideInput` — `bool` = false

**返回值:** `OcctShape`

### `CopySelectedSubshape`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape CopySelectedSubshape()
```

**返回值:** `OcctShape`

### `CopySelectedSubshape`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape CopySelectedSubshape(int index)
```

**参数**

- `index` — `int`

**返回值:** `OcctShape`

### `Cut`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape Cut(OcctShape left, OcctShape right, bool hideInputs = true)
```

**参数**

- `left` — `OcctShape`
- `right` — `OcctShape`
- `hideInputs` — `bool` = true

**返回值:** `OcctShape`

### `Delete`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Delete(IEnumerable<IOcctObject> values)
```

**参数**

- `values` — `IEnumerable<IOcctObject>`

**返回值:** `void`

### `Delete`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Delete(IOcctObject value)
```

**参数**

- `value` — `IOcctObject`

**返回值:** `void`

### `Display`

Copies a headless modeling shape into this initialized AIS engine and displays it. The returned shape belongs to this OcctNet.OcctEngine instance.

```csharp
public OcctShape Display(OcctModelingSession model, OcctModelShape shape, bool fit = false)
```

**参数**

- `model` — `OcctModelingSession`
- `shape` — `OcctModelShape`
- `fit` — `bool` = false

**返回值:** `OcctShape`

### `Dispose`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Dispose()
```

**返回值:** `void`

### `DrillHole`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape DrillHole(OcctShape baseShape, OcctPoint3d origin, OcctVector3d axis, double radius, double depth, bool hideInput = true)
```

**参数**

- `baseShape` — `OcctShape`
- `origin` — `OcctPoint3d`
- `axis` — `OcctVector3d`
- `radius` — `double`
- `depth` — `double`
- `hideInput` — `bool` = true

**返回值:** `OcctShape`

### `DumpView`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void DumpView(string filePath)
```

**参数**

- `filePath` — `string`

**返回值:** `void`

### `EvaluateEdge`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctEdgeEvaluation EvaluateEdge(OcctShape edge, double normalizedParameter)
```

**参数**

- `edge` — `OcctShape`
- `normalizedParameter` — `double`

**返回值:** `OcctEdgeEvaluation`

### `EvaluateFace`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctFaceEvaluation EvaluateFace(OcctShape face, double u, double v)
```

**参数**

- `face` — `OcctShape`
- `u` — `double`
- `v` — `double`

**返回值:** `OcctFaceEvaluation`

### `Exists`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Exists(IOcctObject value)
```

**参数**

- `value` — `IOcctObject`

**返回值:** `bool`

### `ExportAllIges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ExportAllIges(string filePath)
```

**参数**

- `filePath` — `string`

**返回值:** `void`

### `ExportAllStep`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ExportAllStep(string filePath)
```

**参数**

- `filePath` — `string`

**返回值:** `void`

### `ExportBrep`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ExportBrep(OcctShape shape, string filePath)
```

**参数**

- `shape` — `OcctShape`
- `filePath` — `string`

**返回值:** `void`

### `ExportIges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ExportIges(OcctShape shape, string filePath)
```

**参数**

- `shape` — `OcctShape`
- `filePath` — `string`

**返回值:** `void`

### `ExportStep`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ExportStep(OcctShape shape, string filePath)
```

**参数**

- `shape` — `OcctShape`
- `filePath` — `string`

**返回值:** `void`

### `ExportStl`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ExportStl(OcctShape shape, string filePath, double linearDeflection = 0.1, double angularDeflection = 0.5, bool ascii = false)
```

**参数**

- `shape` — `OcctShape`
- `filePath` — `string`
- `linearDeflection` — `double` = 0.1
- `angularDeflection` — `double` = 0.5
- `ascii` — `bool` = false

**返回值:** `void`

### `Extrude`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape Extrude(OcctShape profile, OcctVector3d vector, bool hideInput = true)
```

**参数**

- `profile` — `OcctShape`
- `vector` — `OcctVector3d`
- `hideInput` — `bool` = true

**返回值:** `OcctShape`

### `FilletAllEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape FilletAllEdges(OcctShape shape, double radius, bool hideInput = true)
```

**参数**

- `shape` — `OcctShape`
- `radius` — `double`
- `hideInput` — `bool` = true

**返回值:** `OcctShape`

### `FilletEdges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape FilletEdges(OcctShape shape, IEnumerable<int> edgeIndices, double radius, bool hideInput = true)
```

**参数**

- `shape` — `OcctShape`
- `edgeIndices` — `IEnumerable<int>`
- `radius` — `double`
- `hideInput` — `bool` = true

**返回值:** `OcctShape`

### `Fit`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Fit(IEnumerable<OcctShape> shapes, double margin = 0.05)
```

**参数**

- `shapes` — `IEnumerable<OcctShape>`
- `margin` — `double` = 0.05

**返回值:** `void`

### `Fit`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Fit(OcctShape shape)
```

**参数**

- `shape` — `OcctShape`

**返回值:** `void`

### `FitAll`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void FitAll()
```

**返回值:** `void`

### `FitSelected`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void FitSelected(double margin = 0.05)
```

**参数**

- `margin` — `double` = 0.05

**返回值:** `void`

### `Fuse`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape Fuse(OcctShape left, OcctShape right, bool hideInputs = true)
```

**参数**

- `left` — `OcctShape`
- `right` — `OcctShape`
- `hideInputs` — `bool` = true

**返回值:** `OcctShape`

### `GetApplicationTag`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public string GetApplicationTag(IOcctObject value)
```

**参数**

- `value` — `IOcctObject`

**返回值:** `string`

### `GetAutoZFitSettings`

Returns the current automatic Z-range fitting settings.

```csharp
public OcctAutoZFitSettings GetAutoZFitSettings()
```

**返回值:** `OcctAutoZFitSettings`

### `GetCamera`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctCameraState GetCamera()
```

**返回值:** `OcctCameraState`

### `GetDefaultPolygonOffsets`

Returns the polygon offset configured on the Viewer default drawer.

```csharp
public OcctPolygonOffsetSettings GetDefaultPolygonOffsets()
```

**返回值:** `OcctPolygonOffsetSettings`

### `GetEdgeCurveType`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctCurveType GetEdgeCurveType(OcctShape edge)
```

**参数**

- `edge` — `OcctShape`

**返回值:** `OcctCurveType`

### `GetEdgeEndpoints`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public ValueTuple<OcctPoint3d, OcctPoint3d> GetEdgeEndpoints(OcctShape edge)
```

**参数**

- `edge` — `OcctShape`

**返回值:** `ValueTuple<OcctPoint3d, OcctPoint3d>`

### `GetFaceSurfaceType`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctSurfaceType GetFaceSurfaceType(OcctShape face)
```

**参数**

- `face` — `OcctShape`

**返回值:** `OcctSurfaceType`

### `GetFaceUvBounds`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctUvBounds GetFaceUvBounds(OcctShape face)
```

**参数**

- `face` — `OcctShape`

**返回值:** `OcctUvBounds`

### `GetLocalTransformation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctTransform3d GetLocalTransformation(IOcctObject value)
```

**参数**

- `value` — `IOcctObject`

**返回值:** `OcctTransform3d`

### `GetName`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public string GetName(IOcctObject value)
```

**参数**

- `value` — `IOcctObject`

**返回值:** `string`

### `GetObject`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IOcctObject GetObject(long id)
```

**参数**

- `id` — `long`

**返回值:** `IOcctObject`

### `GetObjectKind`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctObjectKind GetObjectKind(long id)
```

**参数**

- `id` — `long`

**返回值:** `OcctObjectKind`

### `GetPolygonOffsets`

Returns the effective polygon offset for a Viewer object.

```csharp
public OcctPolygonOffsetSettings GetPolygonOffsets(IOcctObject value)
```

**参数**

- `value` — `IOcctObject`

**返回值:** `OcctPolygonOffsetSettings`

### `GetSceneGravityPoint`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d GetSceneGravityPoint()
```

**返回值:** `OcctPoint3d`

### `GetSelectedHits`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctSelectionHit> GetSelectedHits()
```

**返回值:** `IReadOnlyList<OcctSelectionHit>`

### `GetSelectedObjects`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<IOcctObject> GetSelectedObjects()
```

**返回值:** `IReadOnlyList<IOcctObject>`

### `GetShape`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape GetShape(long id)
```

**参数**

- `id` — `long`

**返回值:** `OcctShape`

### `GetShapeBounds`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctBounds GetShapeBounds(OcctShape shape)
```

**参数**

- `shape` — `OcctShape`

**返回值:** `OcctBounds`

### `GetShapeDistance`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctDistanceResult GetShapeDistance(OcctShape first, OcctShape second)
```

**参数**

- `first` — `OcctShape`
- `second` — `OcctShape`

**返回值:** `OcctDistanceResult`

### `GetShapeHash`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public long GetShapeHash(OcctShape shape)
```

**参数**

- `shape` — `OcctShape`

**返回值:** `long`

### `GetShapeLinearProperties`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctMassProperties GetShapeLinearProperties(OcctShape shape)
```

**参数**

- `shape` — `OcctShape`

**返回值:** `OcctMassProperties`

### `GetShapeSurfaceProperties`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctMassProperties GetShapeSurfaceProperties(OcctShape shape)
```

**参数**

- `shape` — `OcctShape`

**返回值:** `OcctMassProperties`

### `GetShapeType`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShapeType GetShapeType(OcctShape shape)
```

**参数**

- `shape` — `OcctShape`

**返回值:** `OcctShapeType`

### `GetShapeVolumeProperties`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctMassProperties GetShapeVolumeProperties(OcctShape shape)
```

**参数**

- `shape` — `OcctShape`

**返回值:** `OcctMassProperties`

### `GetSubshapeAt`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape GetSubshapeAt(OcctShape shape, OcctShapeType type, int index)
```

**参数**

- `shape` — `OcctShape`
- `type` — `OcctShapeType`
- `index` — `int`

**返回值:** `OcctShape`

### `GetSubshapes`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctShape> GetSubshapes(OcctShape shape, OcctShapeType type)
```

**参数**

- `shape` — `OcctShape`
- `type` — `OcctShapeType`

**返回值:** `IReadOnlyList<OcctShape>`

### `GetTopologyCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int GetTopologyCount(OcctShape shape, OcctShapeType type)
```

**参数**

- `shape` — `OcctShape`
- `type` — `OcctShapeType`

**返回值:** `int`

### `GetVertexPoint`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d GetVertexPoint(OcctShape vertex)
```

**参数**

- `vertex` — `OcctShape`

**返回值:** `OcctPoint3d`

### `GetViewportState`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctViewportState GetViewportState()
```

**返回值:** `OcctViewportState`

### `HasLocalTransformation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool HasLocalTransformation(IOcctObject value)
```

**参数**

- `value` — `IOcctObject`

**返回值:** `bool`

### `HideAll`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void HideAll()
```

**返回值:** `void`

### `HideSelected`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void HideSelected()
```

**返回值:** `void`

### `HideSelectionRectangle`

Removes the OCCT-native rubber-band selection overlay.

```csharp
public void HideSelectionRectangle()
```

**返回值:** `void`

### `Highlight`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Highlight(IOcctObject value)
```

**参数**

- `value` — `IOcctObject`

**返回值:** `void`

### `Import`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape Import(string filePath)
```

**参数**

- `filePath` — `string`

**返回值:** `OcctShape`

### `ImportBrep`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape ImportBrep(string filePath)
```

**参数**

- `filePath` — `string`

**返回值:** `OcctShape`

### `ImportIges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape ImportIges(string filePath)
```

**参数**

- `filePath` — `string`

**返回值:** `OcctShape`

### `ImportStep`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape ImportStep(string filePath)
```

**参数**

- `filePath` — `string`

**返回值:** `OcctShape`

### `ImportStepDocument`

Imports a STEP file through STEPCAFControl/XDE and returns its assembly occurrence tree. The existing OcctNet.OcctEngine.ImportStep(System.String) API remains available for source compatibility.

```csharp
public OcctAssemblyDocument ImportStepDocument(string filePath)
```

**参数**

- `filePath` — `string`

**返回值:** `OcctAssemblyDocument`

### `ImportStl`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape ImportStl(string filePath)
```

**参数**

- `filePath` — `string`

**返回值:** `OcctShape`

### `Initialize`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Initialize(IntPtr windowHandle)
```

**参数**

- `windowHandle` — `IntPtr`

**返回值:** `void`

### `InvertSelection`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void InvertSelection()
```

**返回值:** `void`

### `IsSelectable`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsSelectable(IOcctObject value)
```

**参数**

- `value` — `IOcctObject`

**返回值:** `bool`

### `IsSelected`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsSelected(IOcctObject value)
```

**参数**

- `value` — `IOcctObject`

**返回值:** `bool`

### `IsShapeValid`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsShapeValid(OcctShape shape)
```

**参数**

- `shape` — `OcctShape`

**返回值:** `bool`

### `IsVisible`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsVisible(IOcctObject value)
```

**参数**

- `value` — `IOcctObject`

**返回值:** `bool`

### `Loft`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape Loft(IEnumerable<OcctShape> sectionWires, bool makeSolid = true, bool ruled = false, double tolerance = 1E-06, bool hideInputs = true)
```

**参数**

- `sectionWires` — `IEnumerable<OcctShape>`
- `makeSolid` — `bool` = true
- `ruled` — `bool` = false
- `tolerance` — `double` = 1E-06
- `hideInputs` — `bool` = true

**返回值:** `OcctShape`

### `MakeAngleAnnotationShape`

Creates a result-only BRep angular annotation, including vector text and arrows.

```csharp
public OcctShape MakeAngleAnnotationShape(OcctShape firstEdge, OcctShape secondEdge, double radius = 30, double textHeight = 8, double arrowSize = 5, string fontName = null)
```

**参数**

- `firstEdge` — `OcctShape`
- `secondEdge` — `OcctShape`
- `radius` — `double` = 30
- `textHeight` — `double` = 8
- `arrowSize` — `double` = 5
- `fontName` — `string` = null

**返回值:** `OcctShape`

### `MakeArc`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeArc(OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees)
```

**参数**

- `center` — `OcctPoint3d`
- `normal` — `OcctVector3d`
- `xDirection` — `OcctVector3d`
- `radius` — `double`
- `startAngleDegrees` — `double`
- `endAngleDegrees` — `double`

**返回值:** `OcctShape`

### `MakeArc`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeArc(OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end)
```

**参数**

- `start` — `OcctPoint3d`
- `middle` — `OcctPoint3d`
- `end` — `OcctPoint3d`

**返回值:** `OcctShape`

### `MakeBezier`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeBezier(IEnumerable<OcctPoint3d> poles)
```

**参数**

- `poles` — `IEnumerable<OcctPoint3d>`

**返回值:** `OcctShape`

### `MakeBox`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeBox(double dx, double dy, double dz, double x = 0, double y = 0, double z = 0)
```

**参数**

- `dx` — `double`
- `dy` — `double`
- `dz` — `double`
- `x` — `double` = 0
- `y` — `double` = 0
- `z` — `double` = 0

**返回值:** `OcctShape`

### `MakeCircle`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeCircle(OcctPoint3d center, OcctVector3d normal, double radius)
```

**参数**

- `center` — `OcctPoint3d`
- `normal` — `OcctVector3d`
- `radius` — `double`

**返回值:** `OcctShape`

### `MakeCompound`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeCompound(IEnumerable<OcctShape> shapes, bool hideInputs = false)
```

**参数**

- `shapes` — `IEnumerable<OcctShape>`
- `hideInputs` — `bool` = false

**返回值:** `OcctShape`

### `MakeCone`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeCone(double radius1, double radius2, double height, double x = 0, double y = 0, double z = 0)
```

**参数**

- `radius1` — `double`
- `radius2` — `double`
- `height` — `double`
- `x` — `double` = 0
- `y` — `double` = 0
- `z` — `double` = 0

**返回值:** `OcctShape`

### `MakeCone`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeCone(OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height)
```

**参数**

- `origin` — `OcctPoint3d`
- `axis` — `OcctVector3d`
- `radius1` — `double`
- `radius2` — `double`
- `height` — `double`

**返回值:** `OcctShape`

### `MakeCylinder`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeCylinder(double radius, double height, double x = 0, double y = 0, double z = 0)
```

**参数**

- `radius` — `double`
- `height` — `double`
- `x` — `double` = 0
- `y` — `double` = 0
- `z` — `double` = 0

**返回值:** `OcctShape`

### `MakeCylinder`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeCylinder(OcctPoint3d origin, OcctVector3d axis, double radius, double height)
```

**参数**

- `origin` — `OcctPoint3d`
- `axis` — `OcctVector3d`
- `radius` — `double`
- `height` — `double`

**返回值:** `OcctShape`

### `MakeDiameterAnnotationShape`

Creates a result-only BRep diameter annotation, including vector text and arrows.

```csharp
public OcctShape MakeDiameterAnnotationShape(OcctShape circularEdge, double flyout = 20, double textHeight = 8, double arrowSize = 5, string fontName = null)
```

**参数**

- `circularEdge` — `OcctShape`
- `flyout` — `double` = 20
- `textHeight` — `double` = 8
- `arrowSize` — `double` = 5
- `fontName` — `string` = null

**返回值:** `OcctShape`

### `MakeEllipse`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeEllipse(OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius)
```

**参数**

- `center` — `OcctPoint3d`
- `normal` — `OcctVector3d`
- `majorRadius` — `double`
- `minorRadius` — `double`

**返回值:** `OcctShape`

### `MakeFace`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeFace(OcctShape wire, bool onlyPlane = true)
```

**参数**

- `wire` — `OcctShape`
- `onlyPlane` — `bool` = true

**返回值:** `OcctShape`

### `MakeInterpolatedBSpline`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeInterpolatedBSpline(IEnumerable<OcctPoint3d> points, bool periodic = false, double tolerance = 1E-07)
```

**参数**

- `points` — `IEnumerable<OcctPoint3d>`
- `periodic` — `bool` = false
- `tolerance` — `double` = 1E-07

**返回值:** `OcctShape`

### `MakeLengthAnnotationShape`

Creates a result-only BRep linear annotation, including vector text and arrows.

```csharp
public OcctShape MakeLengthAnnotationShape(OcctShape edge, double flyout = 20, double textHeight = 8, double arrowSize = 5, string fontName = null)
```

**参数**

- `edge` — `OcctShape`
- `flyout` — `double` = 20
- `textHeight` — `double` = 8
- `arrowSize` — `double` = 5
- `fontName` — `string` = null

**返回值:** `OcctShape`

### `MakeLine`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeLine(OcctPoint3d start, OcctPoint3d end)
```

**参数**

- `start` — `OcctPoint3d`
- `end` — `OcctPoint3d`

**返回值:** `OcctShape`

### `MakePlaneFace`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakePlaneFace(double width, double height, OcctPoint3d? origin = null, OcctVector3d? xDirection = null, OcctVector3d? normal = null)
```

**参数**

- `width` — `double`
- `height` — `double`
- `origin` — `OcctPoint3d?` = null
- `xDirection` — `OcctVector3d?` = null
- `normal` — `OcctVector3d?` = null

**返回值:** `OcctShape`

### `MakePolyline`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakePolyline(IEnumerable<OcctPoint3d> points, bool closed = false)
```

**参数**

- `points` — `IEnumerable<OcctPoint3d>`
- `closed` — `bool` = false

**返回值:** `OcctShape`

### `MakeRadiusAnnotationShape`

Creates a result-only BRep radius annotation, including vector text and an arrow.

```csharp
public OcctShape MakeRadiusAnnotationShape(OcctShape circularEdge, double flyout = 20, double textHeight = 8, double arrowSize = 5, string fontName = null)
```

**参数**

- `circularEdge` — `OcctShape`
- `flyout` — `double` = 20
- `textHeight` — `double` = 8
- `arrowSize` — `double` = 5
- `fontName` — `string` = null

**返回值:** `OcctShape`

### `MakeRectangleWire`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeRectangleWire(double width, double height, OcctPoint3d? origin = null, OcctVector3d? xDirection = null, OcctVector3d? normal = null)
```

**参数**

- `width` — `double`
- `height` — `double`
- `origin` — `OcctPoint3d?` = null
- `xDirection` — `OcctVector3d?` = null
- `normal` — `OcctVector3d?` = null

**返回值:** `OcctShape`

### `MakeRegularPolygon`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeRegularPolygon(double radius, int sideCount, bool makeFace = false, OcctPoint3d? center = null, OcctVector3d? normal = null, OcctVector3d? xDirection = null)
```

**参数**

- `radius` — `double`
- `sideCount` — `int`
- `makeFace` — `bool` = false
- `center` — `OcctPoint3d?` = null
- `normal` — `OcctVector3d?` = null
- `xDirection` — `OcctVector3d?` = null

**返回值:** `OcctShape`

### `MakeSolidFromShell`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeSolidFromShell(OcctShape shell, bool hideInput = false)
```

**参数**

- `shell` — `OcctShape`
- `hideInput` — `bool` = false

**返回值:** `OcctShape`

### `MakeSphere`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeSphere(double radius, double x = 0, double y = 0, double z = 0)
```

**参数**

- `radius` — `double`
- `x` — `double` = 0
- `y` — `double` = 0
- `z` — `double` = 0

**返回值:** `OcctShape`

### `MakeTextShape`

Creates vector BRep text that remains geometrically sharp at any zoom level.

```csharp
public OcctShape MakeTextShape(string text, OcctPoint3d position, double height = 16, double extrusionDepth = 0, string fontName = null, OcctVector3d? normal = null, OcctVector3d? xDirection = null, bool bold = false, bool italic = false)
```

**参数**

- `text` — `string`
- `position` — `OcctPoint3d`
- `height` — `double` = 16
- `extrusionDepth` — `double` = 0
- `fontName` — `string` = null
- `normal` — `OcctVector3d?` = null
- `xDirection` — `OcctVector3d?` = null
- `bold` — `bool` = false
- `italic` — `bool` = false

**返回值:** `OcctShape`

### `MakeThickSolid`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeThickSolid(OcctShape solid, int faceIndexToRemove, double thickness, double tolerance = 0.0001, bool hideInput = true)
```

**参数**

- `solid` — `OcctShape`
- `faceIndexToRemove` — `int`
- `thickness` — `double`
- `tolerance` — `double` = 0.0001
- `hideInput` — `bool` = true

**返回值:** `OcctShape`

### `MakeTorus`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeTorus(double majorRadius, double minorRadius, OcctPoint3d? center = null, OcctVector3d? axis = null)
```

**参数**

- `majorRadius` — `double`
- `minorRadius` — `double`
- `center` — `OcctPoint3d?` = null
- `axis` — `OcctVector3d?` = null

**返回值:** `OcctShape`

### `MakeVertex`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeVertex(OcctPoint3d point)
```

**参数**

- `point` — `OcctPoint3d`

**返回值:** `OcctShape`

### `MakeWedge`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeWedge(double dx, double dy, double dz, double ltx)
```

**参数**

- `dx` — `double`
- `dy` — `double`
- `dz` — `double`
- `ltx` — `double`

**返回值:** `OcctShape`

### `MakeWire`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MakeWire(IEnumerable<OcctShape> edges, bool hideInputs = false)
```

**参数**

- `edges` — `IEnumerable<OcctShape>`
- `hideInputs` — `bool` = false

**返回值:** `OcctShape`

### `MirrorPlane`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape MirrorPlane(OcctShape shape, OcctPoint3d planePoint, OcctVector3d planeNormal, bool hideInput = false)
```

**参数**

- `shape` — `OcctShape`
- `planePoint` — `OcctPoint3d`
- `planeNormal` — `OcctVector3d`
- `hideInput` — `bool` = false

**返回值:** `OcctShape`

### `MoveTo`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void MoveTo(int x, int y)
```

**参数**

- `x` — `int`
- `y` — `int`

**返回值:** `void`

### `Offset`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape Offset(OcctShape shape, double offset, double tolerance = 0.0001, bool hideInput = true)
```

**参数**

- `shape` — `OcctShape`
- `offset` — `double`
- `tolerance` — `double` = 0.0001
- `hideInput` — `bool` = true

**返回值:** `OcctShape`

### `Owns`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Owns(IOcctObject value)
```

**参数**

- `value` — `IOcctObject`

**返回值:** `bool`

### `Pan`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Pan(int deltaX, int deltaY)
```

**参数**

- `deltaX` — `int`
- `deltaY` — `int`

**返回值:** `void`

### `Redisplay`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Redisplay(IEnumerable<IOcctObject> values)
```

**参数**

- `values` — `IEnumerable<IOcctObject>`

**返回值:** `void`

### `Redisplay`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Redisplay(IOcctObject value)
```

**参数**

- `value` — `IOcctObject`

**返回值:** `void`

### `Redraw`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Redraw()
```

**返回值:** `void`

### `ResetLocalTransformation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ResetLocalTransformation(IOcctObject value)
```

**参数**

- `value` — `IOcctObject`

**返回值:** `void`

### `ResetPolygonOffsets`

Restores a Viewer object's polygon offset to the current default drawer values.

```csharp
public void ResetPolygonOffsets(IOcctObject value)
```

**参数**

- `value` — `IOcctObject`

**返回值:** `void`

### `ResetSceneLighting`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ResetSceneLighting()
```

**返回值:** `void`

### `ResetView`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ResetView()
```

**返回值:** `void`

### `ResetViewMapping`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ResetViewMapping()
```

**返回值:** `void`

### `ResetViewOrientation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ResetViewOrientation()
```

**返回值:** `void`

### `Resize`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Resize()
```

**返回值:** `void`

### `ResizeSurface`

Synchronizes the OCCT render surface with the native window size without drawing a frame. UI adapters can coalesce repeated resize notifications and call OcctNet.OcctEngine.Redraw once.

```csharp
public void ResizeSurface()
```

**返回值:** `void`

### `Revolve`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape Revolve(OcctShape profile, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees = 360, bool hideInput = true)
```

**参数**

- `profile` — `OcctShape`
- `axisPoint` — `OcctPoint3d`
- `axisDirection` — `OcctVector3d`
- `angleDegrees` — `double` = 360
- `hideInput` — `bool` = true

**返回值:** `OcctShape`

### `Rotate`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape Rotate(OcctShape shape, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, bool hideInput = false)
```

**参数**

- `shape` — `OcctShape`
- `axisPoint` — `OcctPoint3d`
- `axisDirection` — `OcctVector3d`
- `angleDegrees` — `double`
- `hideInput` — `bool` = false

**返回值:** `OcctShape`

### `Rotation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Rotation(int x, int y)
```

**参数**

- `x` — `int`
- `y` — `int`

**返回值:** `void`

### `Scale`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape Scale(OcctShape shape, OcctPoint3d center, double factor, bool hideInput = false)
```

**参数**

- `shape` — `OcctShape`
- `center` — `OcctPoint3d`
- `factor` — `double`
- `hideInput` — `bool` = false

**返回值:** `OcctShape`

### `ScreenToPlane`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d ScreenToPlane(int x, int y, OcctPoint3d planePoint, OcctVector3d planeNormal)
```

**参数**

- `x` — `int`
- `y` — `int`
- `planePoint` — `OcctPoint3d`
- `planeNormal` — `OcctVector3d`

**返回值:** `OcctPoint3d`

### `ScreenToRay`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctProjectionRay ScreenToRay(int x, int y)
```

**参数**

- `x` — `int`
- `y` — `int`

**返回值:** `OcctProjectionRay`

### `ScreenToWorld`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d ScreenToWorld(int x, int y)
```

**参数**

- `x` — `int`
- `y` — `int`

**返回值:** `OcctPoint3d`

### `Section`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape Section(OcctShape left, OcctShape right, bool hideInputs = false)
```

**参数**

- `left` — `OcctShape`
- `right` — `OcctShape`
- `hideInputs` — `bool` = false

**返回值:** `OcctShape`

### `Select`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Select(int x, int y, bool appendSelection = false)
```

**参数**

- `x` — `int`
- `y` — `int`
- `appendSelection` — `bool` = false

**返回值:** `void`

### `SelectAllVisible`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SelectAllVisible()
```

**返回值:** `void`

### `SelectObject`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SelectObject(IOcctObject value, bool appendSelection = false)
```

**参数**

- `value` — `IOcctObject`
- `appendSelection` — `bool` = false

**返回值:** `void`

### `SelectObjects`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SelectObjects(IEnumerable<IOcctObject> values, bool appendSelection = false)
```

**参数**

- `values` — `IEnumerable<IOcctObject>`
- `appendSelection` — `bool` = false

**返回值:** `void`

### `SelectRectangle`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SelectRectangle(int x1, int y1, int x2, int y2, bool appendSelection = false, bool allowOverlap = false)
```

**参数**

- `x1` — `int`
- `y1` — `int`
- `x2` — `int`
- `y2` — `int`
- `appendSelection` — `bool` = false
- `allowOverlap` — `bool` = false

**返回值:** `void`

### `SetAntialiasing`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetAntialiasing(bool enabled)
```

**参数**

- `enabled` — `bool`

**返回值:** `void`

### `SetApplicationTag`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetApplicationTag(IOcctObject value, string applicationTag)
```

**参数**

- `value` — `IOcctObject`
- `applicationTag` — `string`

**返回值:** `void`

### `SetAutoZFitMode`

Enables or disables automatic adjustment of the camera Z range. This improves depth precision and prevents clipping, but it does not separate two coplanar objects.

```csharp
public void SetAutoZFitMode(bool enabled, double scaleFactor = 1)
```

**参数**

- `enabled` — `bool`
- `scaleFactor` — `double` = 1

**返回值:** `void`

### `SetAutomaticHighlight`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetAutomaticHighlight(bool enabled)
```

**参数**

- `enabled` — `bool`

**返回值:** `void`

### `SetBackground`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetBackground(Color color)
```

**参数**

- `color` — `Color`

**返回值:** `void`

### `SetCamera`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetCamera(OcctCameraState state)
```

**参数**

- `state` — `OcctCameraState`

**返回值:** `void`

### `SetColor`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetColor(IEnumerable<IOcctObject> values, Color color)
```

**参数**

- `values` — `IEnumerable<IOcctObject>`
- `color` — `Color`

**返回值:** `void`

### `SetColor`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetColor(IOcctObject value, Color color)
```

**参数**

- `value` — `IOcctObject`
- `color` — `Color`

**返回值:** `void`

### `SetComputedHlr`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetComputedHlr(bool enabled)
```

**参数**

- `enabled` — `bool`

**返回值:** `void`

### `SetDefaultMaterial`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetDefaultMaterial(OcctMaterial material, bool applyExisting = false)
```

**参数**

- `material` — `OcctMaterial`
- `applyExisting` — `bool` = false

**返回值:** `void`

### `SetDefaultPolygonOffsets`

Changes the default polygon offset inherited by future Viewer objects. OCCT's recommended shaded-view baseline is Fill, factor 1, units 1.

```csharp
public void SetDefaultPolygonOffsets(OcctPolygonOffsetMode mode, double factor = 1, double units = 1, bool applyExisting = false)
```

**参数**

- `mode` — `OcctPolygonOffsetMode`
- `factor` — `double` = 1
- `units` — `double` = 1
- `applyExisting` — `bool` = false

**返回值:** `void`

### `SetDimensionFlyout`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetDimensionFlyout(OcctDimension dimension, double flyout)
```

**参数**

- `dimension` — `OcctDimension`
- `flyout` — `double`

**返回值:** `void`

### `SetDisplayMode`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetDisplayMode(IEnumerable<IOcctObject> values, OcctDisplayMode displayMode)
```

**参数**

- `values` — `IEnumerable<IOcctObject>`
- `displayMode` — `OcctDisplayMode`

**返回值:** `void`

### `SetDisplayMode`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetDisplayMode(IOcctObject value, OcctDisplayMode displayMode)
```

**参数**

- `value` — `IOcctObject`
- `displayMode` — `OcctDisplayMode`

**返回值:** `void`

### `SetDisplayMode`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetDisplayMode(OcctDisplayMode displayMode)
```

**参数**

- `displayMode` — `OcctDisplayMode`

**返回值:** `void`

### `SetDisplayPrecision`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetDisplayPrecision(double deviationCoefficient, double deviationAngleDegrees, bool applyExisting = true)
```

**参数**

- `deviationCoefficient` — `double`
- `deviationAngleDegrees` — `double`
- `applyExisting` — `bool` = true

**返回值:** `void`

### `SetFaceBoundariesVisible`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetFaceBoundariesVisible(bool visible, bool applyExisting = true)
```

**参数**

- `visible` — `bool`
- `applyExisting` — `bool` = true

**返回值:** `void`

### `SetFrustumCulling`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetFrustumCulling(bool enabled)
```

**参数**

- `enabled` — `bool`

**返回值:** `void`

### `SetGradientBackground`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetGradientBackground(Color first, Color second, OcctGradientFillMethod fillMethod = OcctGradientFillMethod.Vertical)
```

**参数**

- `first` — `Color`
- `second` — `Color`
- `fillMethod` — `OcctGradientFillMethod` = OcctGradientFillMethod.Vertical

**返回值:** `void`

### `SetHoverHighlightColor`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetHoverHighlightColor(Color color)
```

**参数**

- `color` — `Color`

**返回值:** `void`

### `SetImmediateUpdate`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetImmediateUpdate(bool enabled)
```

**参数**

- `enabled` — `bool`

**返回值:** `void`

### `SetLineWidth`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetLineWidth(IEnumerable<IOcctObject> values, double width)
```

**参数**

- `values` — `IEnumerable<IOcctObject>`
- `width` — `double`

**返回值:** `void`

### `SetLineWidth`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetLineWidth(IOcctObject value, double width)
```

**参数**

- `value` — `IOcctObject`
- `width` — `double`

**返回值:** `void`

### `SetLocalTransformation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetLocalTransformation(IOcctObject value, OcctTransform3d transform)
```

**参数**

- `value` — `IOcctObject`
- `transform` — `OcctTransform3d`

**返回值:** `void`

### `SetLocalTransformations`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetLocalTransformations(IReadOnlyList<OcctObjectTransformUpdate> updates)
```

**参数**

- `updates` — `IReadOnlyList<OcctObjectTransformUpdate>`

**返回值:** `void`

### `SetMaterial`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetMaterial(IEnumerable<IOcctObject> values, OcctMaterial material)
```

**参数**

- `values` — `IEnumerable<IOcctObject>`
- `material` — `OcctMaterial`

**返回值:** `void`

### `SetMaterial`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetMaterial(IOcctObject value, OcctMaterial material)
```

**参数**

- `value` — `IOcctObject`
- `material` — `OcctMaterial`

**返回值:** `void`

### `SetMsaaSamples`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetMsaaSamples(int samples)
```

**参数**

- `samples` — `int`

**返回值:** `void`

### `SetName`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetName(IOcctObject value, string name)
```

**参数**

- `value` — `IOcctObject`
- `name` — `string`

**返回值:** `void`

### `SetPerspectiveFieldOfView`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetPerspectiveFieldOfView(double degrees)
```

**参数**

- `degrees` — `double`

**返回值:** `void`

### `SetPointPosition`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetPointPosition(OcctPoint point, OcctPoint3d position)
```

**参数**

- `point` — `OcctPoint`
- `position` — `OcctPoint3d`

**返回值:** `void`

### `SetPointStyle`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetPointStyle(OcctPoint point, OcctPointMarker marker, double scale, Color color)
```

**参数**

- `point` — `OcctPoint`
- `marker` — `OcctPointMarker`
- `scale` — `double`
- `color` — `Color`

**返回值:** `void`

### `SetPolygonOffsets`

Sets a per-object polygon offset. Use a negative Fill offset to draw a coplanar overlay in front of its reference object, or a larger positive value to push it behind.

```csharp
public void SetPolygonOffsets(IOcctObject value, OcctPolygonOffsetMode mode, double factor = 1, double units = 1)
```

**参数**

- `value` — `IOcctObject`
- `mode` — `OcctPolygonOffsetMode`
- `factor` — `double` = 1
- `units` — `double` = 1

**返回值:** `void`

### `SetProjection`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetProjection(OcctProjectionType projection)
```

**参数**

- `projection` — `OcctProjectionType`

**返回值:** `void`

### `SetRenderResolution`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetRenderResolution(double dpi)
```

**参数**

- `dpi` — `double`

**返回值:** `void`

### `SetRenderResolutionScale`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetRenderResolutionScale(double scale)
```

**参数**

- `scale` — `double`

**返回值:** `void`

### `SetRenderingMethod`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetRenderingMethod(OcctRenderingMethod method)
```

**参数**

- `method` — `OcctRenderingMethod`

**返回值:** `void`

### `SetSceneLighting`

Bridge 2.5 source-compatibility entry point. New code should construct OcctNet.OcctSceneLightingSettings explicitly.

```csharp
public void SetSceneLighting(double ambientIntensity, double directionalIntensity, OcctVector3d direction, bool headlight)
```

**参数**

- `ambientIntensity` — `double`
- `directionalIntensity` — `double`
- `direction` — `OcctVector3d`
- `headlight` — `bool`

**返回值:** `void`

### `SetSceneLighting`

Bridge 2.5 source-compatibility entry point. New code should construct OcctNet.OcctSceneLightingSettings explicitly.

```csharp
public void SetSceneLighting(OcctSceneLightingSettings settings)
```

**参数**

- `settings` — `OcctSceneLightingSettings`

**返回值:** `void`

### `SetSelectable`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetSelectable(IEnumerable<IOcctObject> values, bool selectable)
```

**参数**

- `values` — `IEnumerable<IOcctObject>`
- `selectable` — `bool`

**返回值:** `void`

### `SetSelectable`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetSelectable(IOcctObject value, bool selectable)
```

**参数**

- `value` — `IOcctObject`
- `selectable` — `bool`

**返回值:** `void`

### `SetSelection`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetSelection(IEnumerable<IOcctObject> values, OcctSelectionOperation operation = OcctSelectionOperation.Replace)
```

**参数**

- `values` — `IEnumerable<IOcctObject>`
- `operation` — `OcctSelectionOperation` = OcctSelectionOperation.Replace

**返回值:** `void`

### `SetSelectionHighlightColor`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetSelectionHighlightColor(Color color)
```

**参数**

- `color` — `Color`

**返回值:** `void`

### `SetSelectionMode`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetSelectionMode(OcctSelectionMode mode)
```

**参数**

- `mode` — `OcctSelectionMode`

**返回值:** `void`

### `SetSelectionTolerance`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetSelectionTolerance(int pixelTolerance)
```

**参数**

- `pixelTolerance` — `int`

**返回值:** `void`

### `SetShadowsEnabled`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetShadowsEnabled(bool enabled)
```

**参数**

- `enabled` — `bool`

**返回值:** `void`

### `SetText`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetText(OcctText textObject, string text)
```

**参数**

- `textObject` — `OcctText`
- `text` — `string`

**返回值:** `void`

### `SetTextAngle`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetTextAngle(OcctText textObject, double angleDegrees)
```

**参数**

- `textObject` — `OcctText`
- `angleDegrees` — `double`

**返回值:** `void`

### `SetTextFont`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetTextFont(OcctText textObject, string fontName)
```

**参数**

- `textObject` — `OcctText`
- `fontName` — `string`

**返回值:** `void`

### `SetTextHeight`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetTextHeight(OcctText textObject, double height)
```

**参数**

- `textObject` — `OcctText`
- `height` — `double`

**返回值:** `void`

### `SetTextPosition`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetTextPosition(OcctText textObject, OcctPoint3d position)
```

**参数**

- `textObject` — `OcctText`
- `position` — `OcctPoint3d`

**返回值:** `void`

### `SetTextZoomable`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetTextZoomable(OcctText textObject, bool zoomable)
```

**参数**

- `textObject` — `OcctText`
- `zoomable` — `bool`

**返回值:** `void`

### `SetTransparency`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetTransparency(IEnumerable<IOcctObject> values, double transparency)
```

**参数**

- `values` — `IEnumerable<IOcctObject>`
- `transparency` — `double`

**返回值:** `void`

### `SetTransparency`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetTransparency(IOcctObject value, double transparency)
```

**参数**

- `value` — `IOcctObject`
- `transparency` — `double`

**返回值:** `void`

### `SetTriedronVisible`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetTriedronVisible(bool visible)
```

**参数**

- `visible` — `bool`

**返回值:** `void`

### `SetView`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetView(OcctViewOrientation orientation)
```

**参数**

- `orientation` — `OcctViewOrientation`

**返回值:** `void`

### `SetViewCubeLanguage`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetViewCubeLanguage(OcctViewCubeLanguage language)
```

**参数**

- `language` — `OcctViewCubeLanguage`

**返回值:** `void`

### `SetViewCubeVisible`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetViewCubeVisible(bool visible)
```

**参数**

- `visible` — `bool`

**返回值:** `void`

### `SetVisible`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetVisible(IEnumerable<IOcctObject> values, bool visible)
```

**参数**

- `values` — `IEnumerable<IOcctObject>`
- `visible` — `bool`

**返回值:** `void`

### `SetVisible`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetVisible(IOcctObject value, bool visible)
```

**参数**

- `value` — `IOcctObject`
- `visible` — `bool`

**返回值:** `void`

### `SetZUpView`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void SetZUpView(OcctZUpViewOrientation orientation, bool fitAll = true)
```

**参数**

- `orientation` — `OcctZUpViewOrientation`
- `fitAll` — `bool` = true

**返回值:** `void`

### `Sew`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape Sew(IEnumerable<OcctShape> shapes, double tolerance = 1E-06, bool hideInputs = false)
```

**参数**

- `shapes` — `IEnumerable<OcctShape>`
- `tolerance` — `double` = 1E-06
- `hideInputs` — `bool` = false

**返回值:** `OcctShape`

### `ShowAll`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ShowAll()
```

**返回值:** `void`

### `ShowSelectionRectangle`

Displays or updates an OCCT-native 2D rubber-band rectangle in the top overlay layer. Coordinates use window client pixels with the origin at the upper-left corner.

```csharp
public void ShowSelectionRectangle(int x1, int y1, int x2, int y2, Color lineColor, Color fillColor, double fillTransparency = 0.82, double lineWidth = 1)
```

**参数**

- `x1` — `int`
- `y1` — `int`
- `x2` — `int`
- `y2` — `int`
- `lineColor` — `Color`
- `fillColor` — `Color`
- `fillTransparency` — `double` = 0.82
- `lineWidth` — `double` = 1

**返回值:** `void`

### `StartRotation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void StartRotation(int x, int y)
```

**参数**

- `x` — `int`
- `y` — `int`

**返回值:** `void`

### `Sweep`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape Sweep(OcctShape spineWire, OcctShape profile, bool hideInputs = true)
```

**参数**

- `spineWire` — `OcctShape`
- `profile` — `OcctShape`
- `hideInputs` — `bool` = true

**返回值:** `OcctShape`

### `Translate`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape Translate(OcctShape shape, OcctVector3d vector, bool hideInput = false)
```

**参数**

- `shape` — `OcctShape`
- `vector` — `OcctVector3d`
- `hideInput` — `bool` = false

**返回值:** `OcctShape`

### `TryGetDetectedHit`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool TryGetDetectedHit(out OcctSelectionHit hit)
```

**参数**

- `hit` — `out OcctSelectionHit`

**返回值:** `bool`

### `TryGetObject`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool TryGetObject(long id, out IOcctObject value)
```

**参数**

- `id` — `long`
- `value` — `out IOcctObject`

**返回值:** `bool`

### `TryGetObjectByApplicationTag`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool TryGetObjectByApplicationTag(string applicationTag, out IOcctObject value)
```

**参数**

- `applicationTag` — `string`
- `value` — `out IOcctObject`

**返回值:** `bool`

### `TryGetShape`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool TryGetShape(long id, out OcctShape shape)
```

**参数**

- `id` — `long`
- `shape` — `out OcctShape`

**返回值:** `bool`

### `TryScreenToPlane`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool TryScreenToPlane(int x, int y, OcctPoint3d planePoint, OcctVector3d planeNormal, out OcctPoint3d result)
```

**参数**

- `x` — `int`
- `y` — `int`
- `planePoint` — `OcctPoint3d`
- `planeNormal` — `OcctVector3d`
- `result` — `out OcctPoint3d`

**返回值:** `bool`

### `Unhighlight`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Unhighlight(IOcctObject value)
```

**参数**

- `value` — `IOcctObject`

**返回值:** `void`

### `UpdateShape`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void UpdateShape(OcctShape viewerShape, OcctModelingSession sourceSession, OcctModelShape sourceShape, OcctShapeUpdateOptions options = OcctShapeUpdateOptions.PreserveAll)
```

**参数**

- `viewerShape` — `OcctShape`
- `sourceSession` — `OcctModelingSession`
- `sourceShape` — `OcctModelShape`
- `options` — `OcctShapeUpdateOptions` = OcctShapeUpdateOptions.PreserveAll

**返回值:** `void`

### `WindowFit`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void WindowFit(int x1, int y1, int x2, int y2)
```

**参数**

- `x1` — `int`
- `y1` — `int`
- `x2` — `int`
- `y2` — `int`

**返回值:** `void`

### `WorldToScreen`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public Point WorldToScreen(OcctPoint3d point)
```

**参数**

- `point` — `OcctPoint3d`

**返回值:** `Point`

### `Zoom`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Zoom(double factor)
```

**参数**

- `factor` — `double`

**返回值:** `void`

### `ZoomAtPoint`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void ZoomAtPoint(int x, int y, double delta)
```

**参数**

- `x` — `int`
- `y` — `int`
- `delta` — `double`

**返回值:** `void`

## 字段 / 枚举值

无

