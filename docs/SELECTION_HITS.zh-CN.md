# Viewer 结构化选择命中

`OcctEngine` 现在可以直接读取当前 AIS Selection / Detection 的结构化身份，上层不需要再自行反查裸 Object ID，也不需要直接处理 OCCT Owner。

```csharp
var selected = engine.GetSelectedHits();
if (engine.TryGetDetectedHit(out var hover))
{
    Console.WriteLine($"{hover.Owner.Id}: {hover.SubshapeType} #{hover.SubshapeIndex}");
}
```

## 数据契约

`OcctSelectionHit` 只暴露当前实现中**真实可取得、语义明确**的数据：

- `Owner`：被选择/检测敏感实体所属的已注册 `IOcctObject`；
- `SubshapeType`：`Vertex`、`Edge`、`Wire`、`Face`、`Shell`、`Solid` 或 `Shape`；
- `SubshapeIndex`：当前运行时拓扑索引；整对象选择为 `-1`；
- `IsSubshape`：`SubshapeIndex >= 0` 的快捷判断。

接口刻意不保留没有真实数据来源的 `Point` 占位字段。只有在 Native Viewer 路径能够稳定、明确地提供命中点时，才应单独增加该能力。

## Selected Hit 批量读取

`GetSelectedHits()` 使用统一的“两次调用批量 ABI”模式：

```text
occt_selected_hits(handle, null, 0, &count)
→ 按实际数量分配 Managed Buffer
→ occt_selected_hits(handle, buffer, capacity, &count)
```

因此不会采用 `count + hit_at(index)` 的 N+1 跨 P/Invoke 模式。无论选中多少对象，Selected Hit 读取都保持两次 Native Crossing。

`TryGetDetectedHit()` 使用单次 `occt_detected_hit()`，并遵循 Bridge 统一成功/错误契约。当前没有检测到已注册对象时返回 `false`，不使用特殊负数错误码污染高层 API。

## Subshape 身份

对于 BRep 子拓扑选择，`SubshapeIndex` 与下面接口使用相同的 `TopExp_Explorer` 顺序：

```csharp
engine.GetSubshapeAt(ownerShape, hit.SubshapeType, hit.SubshapeIndex)
```

因此当前交互中可以直接服务于：

- Edge 圆角/倒角选择；
- Shell 删除 Face；
- 测量；
- 属性查看；
- 上层 Feature Command；
- 基于当前 Face/Edge 的业务交互。

整对象选择统一为：

```text
SubshapeType  = Shape
SubshapeIndex = -1
```

## 持久化边界

该 Index 只是**运行时交互标识**，不是 Persistent Naming。应用不能只把 `SubshapeIndex` 作为长期唯一拓扑引用写入文档。

参数化 CAD 应用应先把 Selection Hit 转为自己拥有的稳定引用，例如：

```text
SelectionHit
→ 当前运行时 Subshape
→ Feature / Operation History 语义引用
→ 几何 + 邻接签名兜底
```

Persistent Naming、Document Entity、Undo/Redo、Command 状态和 Feature 语义仍属于应用层，不重新塞回 Bridge。

## Native 代码组织

结构化 Selection State 与框选 Overlay 明确分开：

```text
OcctSelectionOverlay.h     2D 框选 Overlay
OcctSelectionState.h       Selected/Detected 结构化身份 ABI
OcctSelectionState.cpp     Selection State 实现
```

这样 Selection 身份能力不会继续和 UI Overlay 渲染职责混在一起。
