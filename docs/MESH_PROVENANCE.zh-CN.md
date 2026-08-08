# Shape Mesh Face 来源追溯

`GetShapeMesh()` 会把一个 Shape 中所有 Face 的三角网格合并为一个 `OcctMesh`。这种形式适合渲染和导出，但普通合并 Mesh 无法直接回答“某个 Node 或 Triangle 来自哪个源 Face”。

Bridge 2.6 新增 `GetShapeMeshData()`，在不破坏现有 `OcctMesh` API 的前提下保留 Face 来源关系。

## 生成带来源信息的合并 Mesh

```csharp
using var model = new OcctModelingSession();
var shape = model.MakeBox(100, 80, 60);

var data = model.GetShapeMeshData(shape);

Console.WriteLine($"Faces: {data.FaceCount}");
Console.WriteLine($"Nodes: {data.NodeCount}");
Console.WriteLine($"Triangles: {data.TriangleCount}");
```

`OcctShapeMeshData` 包含：

- `Mesh`：原有格式的合并 `OcctMesh`；
- `FaceRanges`：每个源 Face 对应一个连续区间；
- `FaceCount`、`NodeCount`、`TriangleCount`；
- `GetFaceRange(faceIndex)`；
- `TryGetFaceForNode()` / `GetFaceForNode()`；
- `TryGetFaceForTriangle()` / `GetFaceForTriangle()`。

原有代码仍可继续使用：

```csharp
OcctMesh mesh = model.GetShapeMesh(shape);
```

现在 `GetShapeMesh()` 内部直接复用 `GetShapeMeshData(...).Mesh`，因此旧接口与来源追溯接口只有一套合并实现，不会出现两套逻辑逐渐漂移的问题。

## Face Range

每个 `OcctShapeMeshFaceRange` 记录一个 Face 在合并 Mesh 中贡献的连续 Node/Triangle 区间：

```csharp
foreach (var range in data.FaceRanges)
{
    Console.WriteLine(
        $"Face {range.Face.Id}: " +
        $"nodes [{range.NodeStart}, {range.NodeEndExclusive}), " +
        $"triangles [{range.TriangleStart}, {range.TriangleEndExclusive})");
}
```

主要属性：

- `Face`；
- `NodeStart`、`NodeCount`、`NodeEndExclusive`；
- `TriangleStart`、`TriangleCount`、`TriangleEndExclusive`；
- `ContainsNode(index)`；
- `ContainsTriangle(index)`。

Range 顺序与合并 Mesh 时遍历 Face 的顺序一致。即使某个 Face 没有实际三角网格，其区间仍保持整体连续关系。

## 拾取与 BIM 属性映射

如果渲染/拾取层返回的是合并后的 Triangle 索引：

```csharp
if (data.TryGetFaceForTriangle(hitTriangleIndex, out var sourceFace))
{
    // 根据 sourceFace 继续查 CAD/BIM 属性、选择状态、分析结果等。
}
```

反查采用有序 Face Range 二分查找，而不是给每一个 Triangle 单独附加 FaceId，因此来源信息的额外内存开销与 Face 数量相关，而不是与 Triangle 数量相关。

如果业务拿到的是 Node 索引，也可以直接：

```csharp
var sourceFace = data.GetFaceForNode(nodeIndex);
```

## 索引语义

合并后的 Triangle 节点索引仍然直接指向 `data.Mesh.Nodes`，与原 `GetShapeMesh()` 完全一致，来源追溯层不会再次重新编号。

单个 Face 的局部索引转换关系为：

```text
combinedNodeIndex = localNodeIndex + range.NodeStart
combinedTriangleIndex = localTriangleIndex + range.TriangleStart
```

复制 Triangle 时，其三个 Node 索引都会增加该 Face 的 `NodeStart`。

## 所有权

`OcctShapeMeshFaceRange.Face` 是普通 `OcctModelShape`，仍归生成该 Mesh 的 `OcctModelingSession` 所有，跨 Session 使用仍按现有所有权规则拒绝。

`OcctShapeMeshData` 是一次 Managed 快照。后续重新 Triangulate 或 ClearTriangulation 不会反向修改已经生成的 Managed Node/Triangle/Range 集合。

## 性能与适用场景

该能力**不增加新的 Native ABI**。`GetShapeMeshData()` 的流程是：

1. 对 Root Shape Triangulate 一次；
2. 枚举源 Face；
3. 使用现有 Face Mesh ABI 读取每个 Face 网格；
4. 合并 Node/Triangle；
5. 每个 Face 仅记录一个紧凑 Range。

只需要渲染几何时继续使用 `GetShapeMesh()` 即可；需要以下能力时优先使用 `GetShapeMeshData()`：

- Triangle/Node 拾取后追溯 Face；
- BIM/CAD 属性映射；
- Face 级结果着色；
- 局部几何分析；
- 按 Face 选择性导出；
- 模型审查和问题定位。

## 验证

`tests/OcctNet.Smoke/ShapeMeshProvenanceSmoke.cs` 使用 6 个 Face 的 Box 校验：Range 连续性、完整 Mesh 覆盖、Node/Triangle 反查和越界行为。

云端 CI 会编译该 Smoke；真实 OCCT 执行仍需本地 Native 发布门禁：

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 根目录>"
```
