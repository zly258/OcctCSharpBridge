# 拓扑邻接与自由边界分析

OcctCSharpBridge 针对 CAD/BIM 模型质量检查提供三层能力：单个子拓扑查询、**批量 Edge→Face 邻接分析**和更严格的 **OCCT 自由边界分析**。

## 单个邻接查询

只检查一个选中子拓扑时，可直接使用：

```csharp
var adjacentFaces = model.GetAdjacentFaces(rootShape, edge);
var incidentEdges = model.GetIncidentEdges(rootShape, vertex);
var incidentFaces = model.GetIncidentFaces(rootShape, vertex);
```

这组接口适合交互式检查或少量查询。

## 批量邻接分析

同一个 Root Shape 需要检查大量 Edge 时，优先只调用一次 `AnalyzeEdgeAdjacency()`：

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

`OcctEdgeAdjacencyInfo` 提供：

- `Edge`；
- `AdjacentFaceCount`；
- `IsIsolated`；
- `IsBoundaryCandidate`；
- `IsManifoldInterior`；
- `IsNonManifold`。

`OcctEdgeAdjacencyResult` 保存完整不可变快照，并提供预分类集合及 `GetEdgesByAdjacentFaceCount(min,max)`。

Native 层只为整个 Root Shape 构建一次 `TopExp::MapShapesAndUniqueAncestors()` Edge→Face 索引；同一个 Face 即使因 Seam 等拓扑情况多次引用 Edge，也只按一个**不同祖先 Face**计数。Managed 层采用一次数量查询 + 一次数组填充，不再为每一条 Edge 重复跨 P/Invoke 并重建祖先映射。

现有快捷接口已经自动复用批量路径，调用方式保持不变：

```csharp
model.GetBoundaryEdgeCandidates(rootShape);
model.GetManifoldInteriorEdges(rootShape);
model.GetNonManifoldEdges(rootShape);
model.GetEdgesByAdjacentFaceCount(rootShape, minimum, maximum);
```

分类规则为：

- 0 个相邻 Face：孤立 Edge；
- 1 个不同相邻 Face：边界候选；
- 2 个不同相邻 Face：普通流形内部边；
- 3 个及以上不同相邻 Face：非流形候选。

### 为什么仍然叫 Candidate

“只有一个不同祖先 Face”是非常有用的快速筛选信号，但周期曲面 Seam Edge、导入模型的特殊拓扑等情况仍可能让邻接计数不足以作为最终几何结论。因此 `GetBoundaryEdgeCandidates()` 和 `BoundaryCandidates` 继续明确使用 Candidate 语义。

## 严格自由边界分析

真正判断 Shell 是否有开口、缝隙或是否需要 Sewing/Healing 时，使用 `AnalyzeFreeBounds()`：

```csharp
var result = model.AnalyzeFreeBounds(
    shape,
    tolerance: 1e-6,
    splitClosed: true,
    splitOpen: true);

foreach (var wire in result.ClosedWires)
{
    // 闭合自由边界，例如孔洞或开口周边。
}

foreach (var wire in result.OpenWires)
{
    // 开放自由边界链，可用于 Shell 缝隙诊断。
}
```

`OcctFreeBoundsResult` 保存本次 Tolerance、Closed/Open Wire、数量和快捷判断属性；返回 Wire 与原 Shape 仍属于同一个 `OcctModelingSession`。

## Tolerance

自由边界 Tolerance 与模型单位有关。默认值为 `1e-7`，但 STEP/IGES 等导入模型应根据项目单位、源系统精度和实际模型容差确定。`OcctFreeBoundsResult.Tolerance` 会保留实际参数，便于自动审图和质量记录追溯。

## 典型工程流程

建议流程：

1. `GetTopologyCounts()` 获取结构摘要；
2. 对待检查模型或 Shell 调用一次 `AnalyzeEdgeAdjacency()`；
3. 使用 `NonManifoldEdges` 定位明显非流形问题；
4. 使用 `BoundaryCandidates` 做低成本开口筛选；
5. 在最终判断开口前调用 `AnalyzeFreeBounds()`；
6. 确需修复时再进入现有 Validation / Healing。

对于大型导入模型，如果需要多种 Edge 分类，建议保留并复用同一个 `OcctEdgeAdjacencyResult`，不要重复调用多个快捷分类方法。

## Native 实现

拓扑分析独立维护在 `OcctModelingTopologyAnalysis.h/.cpp`。

ABI 3 增量接口：

- `occt_model_shape_edge_adjacency`：批量 Edge→不同 Face 数量；
- `occt_model_shape_free_bounds`：使用 `ShapeAnalysis_FreeBounds` 提取严格 Closed/Open 自由边界。

## 验证

云端 CI 校验 Native 声明、定义、P/Invoke 与高层接口，并编译 Smoke 项目。真实 OCCT 执行由以下测试覆盖：

- `tests/OcctNet.Smoke/EdgeAdjacencySmoke.cs`；
- `tests/OcctNet.Smoke/FreeBoundsSmoke.cs`。

在安装 OCCT 7.9.0 的 Windows 环境执行：

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 根目录>"
```
