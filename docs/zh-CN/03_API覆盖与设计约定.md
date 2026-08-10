# 03 API 覆盖与设计约定

本文说明 `OcctCSharpBridge 2.6.0` 当前 `main` 的 API 范围和公共设计约定。机器可读版本、平台和 API 数量以仓库根目录 `bridge-contract.json` 为准。

## 1. 当前 API 概况

| 项目 | 数量/版本 |
|---|---:|
| Native exports | `344` |
| Managed P/Invoke declarations | `344` |
| Public .NET types | `105` |
| Viewer API | `210` |
| Modeling API | `134` |
| Native ABI | `4` |
| Bridge | `2.6.0` |
| OCCT | `7.9.0` |

Native declaration、Native definition 与 Managed P/Invoke 必须保持一一对应。

## 2. Managed API 两个主入口

### `OcctEngine`

负责 Viewer/AIS 场景：

- Viewer 初始化与 Resize/Redraw；
- Camera、Projection、Fit、Zoom、Pan、Rotation；
- Screen/World 转换与 Ray；
- Shape/Text/Dimension 等显示对象；
- 对象外观、材质、透明度、Transform；
- Object/Subshape Selection；
- Hover、Rectangle Selection、Selection Hit；
- Annotation、Vector Annotation；
- Viewport State、渲染参数与场景灯光；
- Viewer-owned Shape 的文件交换和对象互操作。

### `OcctModelingSession`

负责 Headless 几何内核场景：

- Primitive、Curve、Wire、Face、Solid；
- Transform/Location；
- Boolean、Splitter；
- Extrude、Revolve、Sweep、Loft；
- Fillet、Chamfer、Offset、Thick Solid；
- Healing、Same-Domain Unify；
- Shape/Face/Edge 几何查询；
- Topology 与邻接；
- Mass/Inertia、Distance、Projection、Ray、Classification；
- B-Spline、微分几何、Face 分析；
- Operation History；
- Shape Inspection；
- Mesh；
- STEP/IGES/BREP/STL；
- Structured Edge/Edge Intersection；
- Topology Reference。

## 3. C ABI 设计规则

Native API 使用稳定 C ABI，而不是直接 P/Invoke C++ 类。

推荐 ABI 数据类型：

- `int32`/`int64`；
- `double`；
- Plain Struct；
- UTF-8 `char*`；
- Handle/ID；
- Buffer + Capacity。

不允许把 STL 容器、OCCT `Handle(...)`、C++ 异常、模板类型直接跨 ABI 暴露。

## 4. Bulk Collection

高数量集合统一优先使用两次调用模式：

```text
call(handle, null, 0, out count, ...) -> success
allocate managed buffer
call(handle, buffer, capacity, out count, ...) -> success
```

Modeling 的 Shape、Subshape、Ancestor、Ray Hit、History、Mesh 等集合使用 Bulk-only 传输。

Viewer 同样遵守这一规则：

- `Objects` / `Shapes` 由 `occt_object_descriptors` 一次传输 `ObjectId + Kind` 快照；
- `SelectedObjects` / Subshape Hit 使用 `occt_selected_hits`；
- 已删除 `occt_object_id_at`、`occt_shape_id_at`、`occt_selected_count`、`occt_selected_at`；
- 已删除历史 `occt_shape_count` compatibility alias。

因此集合路径不再保留以下 N+1 形式：

```text
Count
At(0)
At(1)
At(2)
...
```

`tests/check-bulk-abi.ps1` 会阻止这些已退休接口重新进入 Bridge。

## 5. Owner-aware Handle

公开 Shape/Object 不是全局 ID。

Managed Handle 同时包含：

- Native Registry/Session 中的 ID；
- 创建它的 Owner 身份。

调用 API 时先验证 Owner，再进入 Native。以下写法属于错误设计：

```text
保存 long objectId
关闭 Engine A
创建 Engine B
把旧 objectId 直接交给 Engine B
```

正确做法是保留强类型 `OcctShape`、`OcctModelShape`、`IOcctObject` 等值，并遵守其 Owner 生命周期。

## 6. 错误处理

Native 层负责捕获 OCCT/C++ 异常并保存可读错误信息；Managed 层把失败转换为 `OcctException` 或更具体的参数/状态异常。

设计原则：

- 不让 C++ 异常穿过 C ABI；
- 不要求业务层解析日志判断成功失败；
- 无效参数尽可能在 Managed 层提前拒绝；
- Native 返回值语义应在同一类别 API 中保持一致；
- Error Message 用于诊断，不作为业务状态枚举。

## 7. 结构化结果优先

Bridge 不应只返回模糊的 `bool`，如果 OCCT 能提供稳定且有业务意义的结果，应形成结构化 DTO。

典型例子：

### Edge/Edge Intersection

`IntersectEdges()` 返回 `OcctEdgeIntersection` 集合，区分：

- `Point`；
- `Overlap`。

并保存两条源 Edge 的曲线参数信息，而不是只返回“是否相交”。

### Inertia

`GetLinearInertiaProperties()`、`GetSurfaceInertiaProperties()`、`GetVolumeInertiaProperties()` 返回完整惯性信息，而不是只有 Mass。

### Selection Hit

`OcctSelectionHit` 返回 Owner、SubshapeType、SubshapeIndex，而不是只暴露一个裸对象 ID。

## 8. Topology Reference

运行时 Subshape Index 只适合当前模型状态下的交互，不属于持久拓扑命名。

`CreateTopologyReference()` 创建由几何和拓扑特征组成的版本化指纹；`ResolveTopologyReference()` 根据当前 Root Shape、指纹以及可用 Operation History 解析目标。

解析结果明确区分：

- `Resolved`；
- `Ambiguous`；
- `Removed`；
- `NotFound`；
- `InvalidReference`。

应用仍然拥有自己的 Feature/Document 语义；Bridge 只提供可复用的拓扑引用基础能力。

## 9. Managed-only 工具

`OcctGeometryExtensions` 中不依赖 Native 的计算适合：

- Point/Vector 计算；
- AlmostEquals；
- AABB；
- UV Range；
- Transform/Location；
- 简单矩阵组合与逆变换。

它们是对 Bridge 值类型的轻量辅助，不重新实现 B-Rep 内核。

## 10. API 不包含的应用层能力

`main` 公共 API 不提供：

- Document；
- Command；
- Tool；
- Undo/Redo；
- Snap/Grip；
- Feature Tree；
- 项目 JSON；
- OCAF/XDE；
- 具体 BIM/设备业务规则。

这不是“缺功能”，而是可复用 Bridge 的边界。

## 11. 新 API 的基本要求

新增公开接口前应满足：

1. 有真实 OCCT 或通用 Bridge 场景；
2. Native/Managed 语义一致；
3. 参数和返回值强类型；
4. 集合优先 Bulk；
5. 明确 Owner 和生命周期；
6. 有静态契约或 Managed/Native Smoke 覆盖；
7. 不为了兼容旧实验接口增加 Alias/Wrapper；
8. 不把上层 CAD Framework 下沉到 Bridge。
