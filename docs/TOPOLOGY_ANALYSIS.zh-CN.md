# 拓扑邻接与自由边界分析

OcctCSharpBridge 针对 CAD/BIM 模型质量检查提供两层互补能力：**低成本邻接筛选**与更严格的 **OCCT 自由边界分析**。

## 快速邻接筛选

快捷接口组合现有子拓扑/祖先拓扑能力，不增加新的 Native ABI：

```csharp
var adjacentFaces = model.GetAdjacentFaces(rootShape, edge);
var incidentEdges = model.GetIncidentEdges(rootShape, vertex);
var incidentFaces = model.GetIncidentFaces(rootShape, vertex);

var boundaryCandidates = model.GetBoundaryEdgeCandidates(rootShape);
var manifoldEdges = model.GetManifoldInteriorEdges(rootShape);
var nonManifoldEdges = model.GetNonManifoldEdges(rootShape);
```

Edge 分类依据其在指定 Root Shape 中的祖先 Face 数量：

- 1 个相邻 Face：边界候选；
- 2 个相邻 Face：普通流形内部边；
- 3 个及以上相邻 Face：非流形候选。

需要自定义数量区间时可使用 `GetEdgesByAdjacentFaceCount()`。

### 为什么叫 Candidate

“只有一个祖先 Face”是非常有用的快速筛选信号，但周期曲面 Seam Edge、导入模型的特殊拓扑等情况可能让简单邻接计数不足以作为最终几何结论。因此接口明确命名为 `GetBoundaryEdgeCandidates()`，而不是过度承诺的 `GetBoundaryEdges()`。

## 严格自由边界分析

当后续决策依赖 OCCT 自由边界算法时，使用 `AnalyzeFreeBounds()`：

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

`OcctFreeBoundsResult` 包含：

- `Tolerance`；
- `ClosedWires`；
- `OpenWires`；
- `ClosedWireCount`；
- `OpenWireCount`；
- `TotalWireCount`；
- `HasFreeBounds`；
- `HasOpenFreeBounds`。

返回的 Wire 与被分析 Shape 仍属于同一个 `OcctModelingSession`，所有权规则不变。

## Tolerance

Tolerance 与模型单位有关。默认值为 `1e-7`，适合作为精细几何分析的起点；但 STEP/IGES 等导入模型经常需要根据项目单位、源系统精度和模型实际容差来确定，不应把某个全局值机械套用到所有工程。

如果自由边界结果用于自动通过/不通过判断，应记录实际使用的 Tolerance。`OcctFreeBoundsResult.Tolerance` 会保留本次分析参数，便于审图和质量记录追溯。

## 典型工程流程

一套较稳妥的模型质量检查流程可以是：

1. 使用 `GetTopologyCounts()` 快速获取结构摘要；
2. 使用 `GetNonManifoldEdges()` 定位明显非流形问题；
3. 使用 `GetBoundaryEdgeCandidates()` 做低成本筛选；
4. 在真正判断 Shell 是否存在开口、是否需要 Sewing/Healing 前调用 `AnalyzeFreeBounds()`；
5. 确需修复时再进入现有 Validation / Healing 能力。

对于超大导入模型，当前 Managed 邻接快捷接口优先保证 API 清晰度，并不是最少 Native 调用方案。后续可增加批量邻接分析接口，在不改变高层语义的前提下优化性能。

## Native 实现

严格分析独立维护在 `OcctModelingTopologyAnalysis.h/.cpp`，底层使用 OCCT `ShapeAnalysis_FreeBounds`。

新增一个 ABI 3 增量接口：

- `occt_model_shape_free_bounds`

该接口通过 `boundaryKind` 选择返回 Closed 或 Open Wire Compound；高层 `AnalyzeFreeBounds()` 分别读取两类结果后直接返回 Wire 集合，不把临时 Compound 泄漏给业务层。

## 验证

云端 CI 校验 Native 声明、定义、P/Invoke 与高层接口一致性，并编译 Smoke 项目；真实 OCCT 执行由 `tests/OcctNet.Smoke/FreeBoundsSmoke.cs` 覆盖，需要在安装 OCCT 7.9.0 的 Windows 环境执行：

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 根目录>"
```
