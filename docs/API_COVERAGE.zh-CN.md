# OCCT Bridge 2.6 接口覆盖说明

OcctCSharpBridge 2.6 面向 Windows x64 与 Open CASCADE Technology 7.9.0。可复用 SDK 保持两个核心 Managed façade，并提供三个可选视口宿主：

- `OcctEngine`：交互式 AIS / Viewer / Object 操作；
- `OcctModelingSession`：无界面几何、拓扑、算法、网格、分析与文件交换；
- `OcctNet.WinForms`、`OcctNet.Wpf`、`OcctNet.Avalonia`：只负责可复用视口宿主。

Bridge 明确不使用 OCAF/XDE。Document、Feature/Entity、Command、Tool、Undo/Redo、Snap/Grip 和项目持久化属于 `demo` 或其它 CAD 应用层。详见 [架构边界：Bridge 与 CAD 应用层](ARCHITECTURE_BOUNDARIES.zh-CN.md)。

- 原生桥接版本：`2.6.0`
- Native ABI：`3`
- OCCT：`7.9.0`
- Native exports：`351`
- Managed P/Invoke declarations：`351`
- Public .NET types：`99`
- Compatibility .NET types：`1`
- Viewer API：`214`
- Modeling API：`137`

`Public .NET types` 表示 Bridge 2.6 的主 owner-aware 公共接口；`Compatibility .NET types` 当前只有 Bridge 2.5 的 `OcctObject` 兼容句柄。2.x 期间保留兼容入口，但不继续扩展新的 legacy API。

## API 规则

| 范围 | 统一规则 | 示例 |
|---|---|---|
| Shape 查询 | `GetShape...` / `IsShape...` / `SetShape...` | `GetShapeBounds()` |
| Edge 查询 | `GetEdge...` / `EvaluateEdge...` | `GetEdgeCurveType()` |
| Face 查询 | `GetFace...` / `EvaluateFace...` | `GetFaceUvBounds()` |
| 批量分析 | `Analyze...` | `AnalyzeFaces()`、`AnalyzeEdgeAdjacency()` |
| 结构化检查 | `Inspect...` | `InspectShape()` |
| 索引访问 | `...At` | `GetSubshapeAt()` |
| 构造 | `Make...` | `MakePlanarFace()` |
| 算法 | 操作动词 | `Extrude()`、`OffsetWire()` |
| 网格 | Triangulation 语义 | `Triangulate()`、`GetShapeMeshData()` |
| C ABI | 唯一 `occt_...` 符号 | `occt_model_shape_face_analysis` |

公开 Shape/Object 必须属于创建它的 Engine/Session。主 API 不允许业务代码通过裸 `long` 构造 `OcctShape` / `OcctModelShape`。Native 0/1 参数在 Managed 公共 API 中使用 `bool` 或枚举。

## 程序集职责

| 程序集 | 职责 |
|---|---|
| `OcctNet` | 基础类型、`OcctEngine`、Headless `OcctModelingSession`、Runtime/诊断以及不依赖 UI 框架的视口交互判定 |
| `OcctNet.WinForms` | 可复用 WinForms HWND 视口宿主 |
| `OcctNet.Wpf` | 通过 `WindowsFormsHost` 提供可复用 WPF 视口宿主 |
| `OcctNet.Avalonia` | 通过 `NativeControlHost` + Windows 子 HWND 提供可复用 Avalonia 视口宿主 |

`OcctNet.Avalonia` 当前仍是 **Windows-only** Host，不代表 Linux/macOS Viewer 已得到支持。`OcctNet` 核心本身不引用 WinForms、WPF 或 Avalonia。

完整 WinForms/WPF/Avalonia CAD 应用与 `CadCommon` 仍只位于 `demo`。

## `OcctEngine`

`OcctEngine` 管理 AIS 对象和 Viewer 上下文，覆盖：

- 相机、视图、投影、Screen/World 转换；
- 已注册对象生命周期、显示、外观、材质、深度/显示状态与变换；
- 对象/Subshape 选择、框选、Hover 与结构化 Selection Hit；
- Viewer 场景中的几何/特征创建与标注；
- Viewer 管理 Shape 的 STEP/IGES/BREP/STL 交换。

### 结构化 Selection

- `GetSelectedHits()` 返回 `OcctSelectionHit`；
- `TryGetDetectedHit()` 返回当前 Detected/Hover 注册实体；
- `OcctSelectionHit` 暴露 `Owner`、`SubshapeType` 与运行时 `SubshapeIndex`；
- Selected Hit 使用两次调用的批量 ABI，避免 N+1 P/Invoke；
- Runtime Subshape Index 使用与 `GetSubshapeAt()` 一致的 `TopExp_Explorer` 顺序，但**不是 Persistent Naming**。

详见 [Viewer 结构化选择命中](SELECTION_HITS.zh-CN.md)。

## `OcctModelingSession`

### 构造与算法

构造能力覆盖 Vertex、Line、Polyline、Circle/Arc、Polygon、Ellipse、Bezier、插值 B-Spline、Rectangle/Planar Face、Wire、Compound、Sewing Shell/Solid、Box/Cylinder/Cone/Sphere/Torus/Wedge，以及带孔平面 Face。

算法覆盖 Fuse/Cut/Common/Section/Splitter、Extrude/Revolve/Sweep/Loft、Fillet/Chamfer、3D Offset、平面 Wire Offset、Thick Solid、Same-Domain Unify、Healing 与 Operation History。

射线命中结果以及 Generated/Modified 拓扑历史均通过批量复制 Native ABI 获取；旧的索引式 `...At` 导出继续保留用于 ABI 兼容，但托管集合接口不再逐项调用。

### 拓扑与 Shape 查询

- Shape 类型、方向、闭合、有效性、检查报告、Hash、Tolerance；
- AABB 与 OBB；
- 线/面/体质量属性、惯性张量/主惯性属性与 Shape Distance；
- Location 读写；
- 通用 Subshape 遍历和常用集合；
- Edge/Face/Wire 的祖先、邻接；
- 批量 `AnalyzeEdgeAdjacency()` 与严格 `AnalyzeFreeBounds()`；
- `IsSameShape()` / `IsPartnerShape()` OCCT 拓扑身份语义。

详见 [拓扑邻接与自由边界分析](TOPOLOGY_ANALYSIS.zh-CN.md)。

### 几何与微分几何

- Vertex 与 Edge 求值；
- Curve/Surface 类型与解析几何参数；
- Edge 参数范围、导数、切向/法向、曲率与曲率中心；
- Face UV 范围、周期性、偏导、法向、主曲率/平均曲率/高斯曲率；
- 点投影到 Edge/Face、Ray Intersection、Solid 点分类和精确 `TrimEdge()`；
- B-Spline Curve/Surface 的 Degree、Pole、Weight、Knot、Multiplicity 与控制网格。

详见 [B-Spline 曲线与曲面检查](BSPLINE_CURVES.zh-CN.md)。

### 批量检查

`AnalyzeFaces()` 在 Native 中一次遍历 Face，返回 SurfaceType、Orientation、Area、Tolerance、UV、AABB 和拓扑统计。`InspectShape()` 组合有效性、闭合、容差、Check Report、Bounds、拓扑数量、Edge 邻接、Face 分析、可选 Free Bounds 与可选 Mesh Statistics，只提供客观数据，不硬编码应用层“通过/不通过”规则。

详见 [批量 Face 分析与 Shape 检查](SHAPE_INSPECTION.zh-CN.md)。

### 三角网格与来源追溯

`Triangulate()`、`GetFaceMesh()`、`GetShapeMesh()`、`GetShapeMeshData()`、`ClearTriangulation()` 覆盖网格能力。`OcctShapeMeshData` 保存每个源 Face 的 Node/Triangle 连续区间，因此可以把合并 Mesh 索引映射回 CAD 拓扑，无需为每个 Triangle 单独保存 FaceId。

详见 [Shape Mesh Face 来源追溯](MESH_PROVENANCE.zh-CN.md)。

### 文件交换

直接提供 STEP、IGES、BREP、STL 导入导出和通用文件导入；STL 导出支持显式离散参数。

## Native 内部组织

内部源码组织不属于 ABI：

- Session/Registry、Shape Queries、Topology、Geometry Queries、Viewer Interop 独立分责；
- Geometry 构造拆分为 Curves、Planar、Primitives、Assembly、Transform；
- Boolean、Feature、Healing、Operation History、Projection/Ray/Classification、Mesh、Exchange 已独立分责；
- 广义 `OcctModelingInternal.hxx` 已退出，模块只包含最窄内部 Header 与自己直接使用的 OCCT Header。

这些整理保持 ABI 3 已有签名不变，同时以加法方式扩展到 354 个 C Export。

## UI Host 交互边界

WinForms 与 Avalonia 只共享与 UI 框架无关的判定逻辑：Hover/WorldPoint 节流、框选阈值与方向、拖拽终点恢复、默认缩放倍率。窗口创建、DPI、Mouse Capture、WPF Hosting、Win32 子类化继续由各 Host 独立处理，不建立脆弱的“万能 UI 基类”。

## Runtime 与所有权

- `OcctEngine` / `OcctModelingSession` 内部使用 `SafeHandle`；
- Managed Object/Shape 带 Owner Token，跨 Engine/Session 使用在进入 Native 前拒绝；
- `OcctRuntime.GetDiagnosticInfo()` / `GetDiagnosticReport()` 为无副作用诊断入口；
- Native 失败统一转换为带 Operation/NativeMessage 的 `OcctException`。

详见 [结构化 Runtime 诊断](RUNTIME_DIAGNOSTICS.zh-CN.md)。

## 校验边界

GitHub 云端没有本项目真实 OCCT SDK，因此不会声称执行了 Native 几何。CI 负责：

- Native 声明、定义、P/Invoke 名称一一对应，并校验 Cdecl + ExactSpelling；
- API 数量来自 `bridge-contract.json`，主公共类型与兼容公共类型分开统计；
- 对 Core、WinForms、WPF、Avalonia 做公共 Managed API 签名快照校验；
- Managed 构建和不加载 OCCT 的回归测试；
- UI Host、Selection、Topology、Runtime、Package、源码组织与分支边界契约；
- Smoke 项目源码编译；
- `main` / `demo` 可复用源码直接同步比较。

正式发布前仍必须在安装 OCCT 7.9.0 的 Windows 机器执行真实 Native 门禁：

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 根目录>"
```

真正的 C++ 编译/链接、DLL 加载和几何/拓扑运行结果以本地 Native Smoke 为准。
