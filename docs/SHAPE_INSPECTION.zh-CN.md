# 批量 Face 分析与 Shape 检查

OcctCSharpBridge 针对大型 STEP/BIM 模型新增两层检查接口：

- `AnalyzeFaces()`：一次 Native 批量获取常用 Face 几何/拓扑元数据；
- `InspectShape()`：把现有 Shape、拓扑、邻接、Face、自由边界以及可选 Mesh 统计组合成一个结构化 Managed 快照。

这两个接口都**不内置业务层“合格/不合格”判断**。容差阈值、允许的曲面类型和模型质量规则仍由上层项目决定。

## 批量 Face 分析

```csharp
using var model = new OcctModelingSession();
var shape = model.ImportStep(path);

var analysis = model.AnalyzeFaces(shape);

Console.WriteLine($"Faces: {analysis.FaceCount}");
Console.WriteLine($"Area: {analysis.TotalArea}");
Console.WriteLine($"Maximum face tolerance: {analysis.MaximumTolerance}");

foreach (var face in analysis.Faces)
{
    Console.WriteLine($"{face.SurfaceType}: {face.Area}");
}
```

每个 `OcctFaceAnalysisInfo` 包含：

- 源 `Face`；
- `SurfaceType`；
- 拓扑 `Orientation`；
- 曲面 `Area`；
- Face `Tolerance`；
- `UvBounds`；
- AABB `Bounds`；
- 去重后的 `EdgeCount`；
- `WireCount`；
- `IsAnalytic`、`IsFreeform` 快捷判断。

`OcctFaceAnalysisResult` 还提供总面积、最大 Face 容差、SurfaceType 数量统计和 `GetFacesBySurfaceType()`。

Native 层只遍历一次全部 Face，然后填充连续数组。与“每个 Face 分别调用 SurfaceType、UV、Bounds、Area、Tolerance、局部拓扑”相比，可以显著减少大型模型上的 P/Invoke 往返。

## 结构化 Shape 检查

```csharp
var report = model.InspectShape(shape);

Console.WriteLine(report.IsValid);
Console.WriteLine(report.IsClosed);
Console.WriteLine(report.MaximumTolerance);
Console.WriteLine(report.TopologyCounts[OcctShapeType.Face]);
Console.WriteLine(report.EdgeAdjacency.NonManifoldEdges.Count);
Console.WriteLine(report.FaceAnalysis.SurfaceTypeCounts.Count);
```

`OcctShapeInspectionReport` 包含：

- Shape 类型、有效性、闭合性和最大容差；
- OCCT Check Report 文本；
- Shape Bounds；
- 拓扑数量；
- 批量 Edge→Face 邻接快照；
- 批量 Face 分析快照；
- 可选严格自由边界分析；
- 可选 Mesh 节点数、三角形数和有网格 Face 数量。

它是**事实数据快照**，不是质量规则。例如 Sheet Body 本身可以合法存在自由边界；某个容差值在毫米模型中可能不可接受，但在其他单位或来源模型中未必如此。

## Options 与副作用

默认配置刻意保持轻量：

```csharp
var options = OcctShapeInspectionOptions.Default;
// IncludeFreeBounds = true
// FreeBoundaryTolerance = 1e-7
// GenerateMeshStatistics = false
```

自由边界默认启用，因为不要求三角网格；Mesh Statistics 默认关闭，因为启用后会走正常 `Triangulate()` 路径，并可能创建/刷新 OCCT Triangulation Cache。

确实需要 Mesh 统计时显式开启：

```csharp
var report = model.InspectShape(shape, new OcctShapeInspectionOptions
{
    IncludeFreeBounds = true,
    FreeBoundaryTolerance = 1e-6,
    GenerateMeshStatistics = true,
    MeshParameters = OcctModelMeshParameters.Default
});
```

如果业务后续本身就需要完整 Mesh 与 Face 来源映射，应直接调用 `GetShapeMeshData()` 并保留结果，而不是只为统计数量生成 Mesh 后立即丢弃。

## 建议工程流程

大型导入模型可以采用：

1. 先调用不生成 Mesh 的 `InspectShape()` 获取轻量结构化快照；
2. 检查 `EdgeAdjacency`、`FreeBounds`、SurfaceType 与 Tolerance；
3. 在应用层应用项目自己的合格规则；
4. 只有显示、拾取、Mesh 分析、导出等确实需要时再三角化；
5. 保存原始检查数据，而不仅保存一个 Boolean 结果，便于后续审模和追溯。

## ABI 组织

批量 Face 分析独立维护在：

- `OcctModelingFaceAnalysis.h`
- `OcctModelingFaceAnalysis.cpp`

只增加一个 ABI 3 增量函数：

- `occt_model_shape_face_analysis`

`InspectShape()` 本身属于 Managed 组合接口，不额外增加 Native ABI。

## 验证

云端 CI 会验证 C ABI/PInvoke、Managed 编译和 Smoke 源码兼容，但云端没有项目真实 OCCT SDK。真实执行由 `tests/OcctNet.Smoke/ShapeInspectionSmoke.cs` 覆盖，需要在安装 OCCT 7.9.0 的 Windows 环境执行：

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 根目录>"
```
