# OCCT Bridge API 覆盖说明

OcctCSharpBridge `2.6.0` 是面向 Windows x64、Open CASCADE Technology `7.9.0` 与 .NET 10 的桥接库。可复用接口明确分为 `OcctEngine` 与 `OcctModelingSession`：前者负责 AIS/Viewer 交互，后者负责 Headless 建模。

OCAF/XDE，以及应用层 Document、Command、Tool、Undo/Redo、Snap/Grip、Feature Tree 和项目持久化均不属于 `main` Bridge。

- Native bridge version：`2.6.0`
- Native ABI：`3`
- OCCT：`7.9.0`
- Native exports：`348`
- Managed P/Invoke declarations：`348`
- Public .NET types：`105`
- Viewer API：`214`
- Modeling API：`134`

## Managed 程序集

| 程序集 | 职责 |
|---|---|
| `OcctNet` | 基础类型、Runtime Loading、`OcctEngine`、`OcctModelingSession` 与无 UI 框架依赖的交互策略 |
| `OcctNet.WinForms` | WinForms HWND 视口宿主 |
| `OcctNet.Wpf` | 内部复用 WinForms Host 的 WPF 视口宿主 |
| `OcctNet.Avalonia` | 基于 Windows 子 HWND 的 Avalonia `NativeControlHost` |

Avalonia Host 当前仍是 Windows-only，不表示已经具备 Linux/macOS OCCT Viewer Backend。

## 交互式 Viewer API

`OcctEngine` 覆盖 View、Camera、Projection、Zoom/Pan/Rotation、Screen/World 转换、对象生命周期、显示与外观、Transform、Object/Subshape Selection、Rectangle Selection、Hover、结构化 Selection Hit、Annotation 与 Viewer-owned Shape 文件交换。

`GetSelectedHits()` 与 `TryGetDetectedHit()` 返回 Owner-aware `OcctSelectionHit`。Selection Hit 包含所属注册对象、拓扑类型和运行时 Subshape Index。运行时 Index 不属于 Persistent Naming。

## Headless Modeling API

### 几何与拓扑

`OcctModelingSession` 覆盖 Primitive/Curve 构造、Wire/Face/Solid 组装、Transform、Shape Query、解析/微分几何、B-Spline 检查、Topology 遍历、Adjacency、Free Bounds、Face Analysis、Shape Inspection 和 OBB。

高数量集合统一使用两次调用的 Bulk C ABI：第一次以 Null Buffer 查询数量，第二次一次复制完整结果。该方式用于 Session Shape 枚举、Subshape、Inner Wire、Ancestor、Ray Hit、Operation History 与 Face Mesh 数组。

### 建模算法

支持 Boolean Fuse/Cut/Common/Section、Splitter、Extrude、Revolve、Sweep、Loft、Fillet、Chamfer、Offset、Thick Solid、Same-Domain Unify 与 Healing。OCCT 提供 History 时，算法结果保留 Operation History。

### P0 — 惯性属性

提供 `GetLinearInertiaProperties()`、`GetSurfaceInertiaProperties()` 与 `GetVolumeInertiaProperties()`。

`OcctInertiaProperties` 包含 Mass、Center of Mass、完整 Inertia Tensor、Principal Moments、Principal Axes、Radius of Gyration 与对称性标记。

### P1 — 结构化 Edge/Edge 求交

`IntersectEdges()` 返回 `IReadOnlyList<OcctEdgeIntersection>`，而不是简单 Boolean 命中。每个结果分为 `Point` 或 `Overlap`，并保留 Start/End Point 以及两条源 Edge 的原生曲线参数范围。结果通过 `occt_model_edge_intersections_copy` 批量传输。

### P2 — Topology Reference

`CreateTopologyReference()` 为 Root Shape 内的 Vertex、Edge 或 Face 创建版本化几何/拓扑指纹。指纹包含 Topology Type、Curve/Surface Type、Measure、Center、Bounds、Tolerance、Orientation、Vertex/Edge/Face Adjacency Count 与 Runtime Index Hint。

`ResolveTopologyReference()` 返回 `Resolved`、`Ambiguous`、`Removed`、`NotFound` 或 `InvalidReference`。Runtime Index 只作为低权重 Hint；解析以完整 Fingerprint 为主，也可以结合 OCCT Operation History。

### P3 — Native/Managed ABI 清理

Modeling 集合 ABI 已统一为 Bulk-only。旧的 Shape、Topology、Ray Hit、History、Mesh `Count + At` 集合接口已从新库中移除，Managed Collection 不再逐项跨 Native Boundary 调用。

Native 按职责拆分为 Session/Registry、Shape/Geometry Query、Topology、Boolean/Feature/Healing/History、Projection/Ray/Classification、Mesh、Exchange、Inertia、Structured Intersection 与 Topology Reference。已废弃的广义 `OcctModelingInternal.hxx` 不允许重新引入。

## 数据交换与 Mesh

支持 STEP、IGES、BREP、STL 导入导出以及通用文件导入。Triangulation 支持显式网格参数，并通过 Source Face Range 保留合并 Mesh 的 Face Provenance。

## Runtime 与所有权

`OcctEngine` 和 `OcctModelingSession` 使用 Owner-aware Handle。跨 Engine/Session 的 Handle 会在进入 Native 前被拒绝。Runtime Diagnostics 用于检查 Bridge/OCCT 加载状态，不依赖应用层 CAD Document。

## 本地验证

静态与 Managed 验证：

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
```

使用真实 OCCT SDK 的 Native 编译、链接、加载和几何验证：

```powershell
.\build.ps1 smoke Release
```

本地检查覆盖 Native Declaration/Definition/PInvoke 一致性、API 数量、Bulk ABI、源码职责、UI Host 边界、包内容以及 No-OCAF/XDE 边界。
