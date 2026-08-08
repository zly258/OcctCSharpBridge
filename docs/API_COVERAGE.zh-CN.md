# OCCT Bridge 2.6 接口覆盖说明

OcctCSharpBridge 2.6 面向 Windows x64 与 OCCT 7.9.0，托管层明确分为两个入口：

- `OcctEngine`：面向桌面 CAD 的交互式 AIS / Viewer / 文档对象引擎；
- `OcctModelingSession`：面向批处理、服务端和算法调用的无界面建模内核。

桥接层明确不使用 OCAF/XDE。Document、Undo/Redo、业务 Entity、Command/Tool 与 JSON 持久化属于上层应用职责。

- 原生桥接版本：`2.6.0`
- Native ABI：`3`
- OCCT：`7.9.0`
- Native exports：`345`
- Managed P/Invoke declarations：`345`
- Public .NET types：`90`
- Viewer API：`212`
- Modeling API：`133`

## 2.6 封装规则

| 范围 | 统一规则 | 示例 |
|---|---|---|
| Shape 查询 | `GetShape...` / `IsShape...` / `SetShape...` | `GetShapeBounds()` |
| Edge 查询 | `GetEdge...` / `EvaluateEdge...` | `GetEdgeCurveType()` |
| Face 查询 | `GetFace...` / `EvaluateFace...` | `GetFaceUvBounds()` |
| 索引访问 | `...At` | `GetSubshapeAt()` |
| 构造 | `Make...` | `MakePlanarFace()` |
| 算法 | 操作动词 | `Extrude()`、`OffsetWire()` |
| 网格 | Triangulation 语义 | `Triangulate()`、`GetShapeMeshData()` |
| C ABI | 唯一 `occt_...` 符号 | `occt_model_trim_edge` |

公开 Shape/Object 必须属于创建它的 Engine/Session。业务代码不能通过裸 `long` 构造 `OcctShape` / `OcctModelShape`。Native 0/1 参数在 Managed 公共 API 中使用 `bool` 或枚举表达。

## 程序集职责

| 程序集 | 职责 |
|---|---|
| `OcctNet` | 基础类型、交互式 Engine、Headless ModelingSession、运行时加载与诊断 |
| `OcctNet.WinForms` | 可复用 WinForms OCCT 视口宿主 |
| `OcctNet.Wpf` | 可复用 WPF OCCT 视口宿主 |

完整 WinForms/WPF/Avalonia CAD Demo 只位于 `demo` 分支。

## `OcctModelingSession`

### 构造与算法

支持点、直线、多段线、圆/圆弧、正多边形、椭圆、Bezier、插值 B-Spline、Wire、带孔平面 Face、Compound、Shell/Solid、Box/Cylinder/Cone/Sphere/Torus/Wedge，以及 Fuse/Cut/Common/Section/Splitter、Extrude/Revolve/Sweep/Loft、Fillet/Chamfer、Offset、Thick Solid、Healing 等能力。

### 拓扑分析

- 常用集合：`GetVertices()`、`GetEdges()`、`GetWires()`、`GetFaces()`、`GetShells()`、`GetSolids()` 等；
- 局部拓扑：`GetEdgeVertices()`、`GetWireEdges()`、`GetFaceEdges()`、`GetFaceVertices()`、`GetTopologyCounts()`；
- 单个邻接：`GetAdjacentFaces()`、`GetIncidentEdges()`、`GetIncidentFaces()`；
- `AnalyzeEdgeAdjacency()` 为整个 Root Shape 一次建立 Edge→不同 Face 的 Native 索引，返回 `OcctEdgeAdjacencyResult`；
- `GetBoundaryEdgeCandidates()`、`GetManifoldInteriorEdges()`、`GetNonManifoldEdges()` 已复用批量邻接路径；
- `AnalyzeFreeBounds()` 使用 `ShapeAnalysis_FreeBounds` 返回严格 Closed/Open 自由边界；
- `IsSameShape()` / `IsPartnerShape()` 暴露 OCCT 拓扑身份语义。

详见 [拓扑邻接与自由边界分析](TOPOLOGY_ANALYSIS.zh-CN.md)。

### 几何与微分几何

支持 Curve/Surface 类型、解析几何参数、Edge 参数范围、导数/切向/法向/曲率、Face U/V 范围、周期性、偏导、主曲率/平均曲率/高斯曲率、点投影、射线相交、Solid 点分类和 `TrimEdge()`。

`GetBSplineCurveData()` 与 `GetBSplineSurfaceData()` 提供 Degree、Pole、Weight、Knot、Multiplicity，以及 Surface 的 U/V 控制网格。Managed 集合统一使用 0 起始索引。详见 [B-Spline 曲线与曲面检查](BSPLINE_CURVES.zh-CN.md)。

### 三角网格与 Face 来源追溯

- `Triangulate()`：生成 OCCT Triangulation；
- `GetFaceMesh()`：读取单个 Face 网格；
- `GetShapeMesh()`：保持原有兼容 API，返回一个合并 `OcctMesh`；
- `GetShapeMeshData()`：返回同一个合并 Mesh，并额外保存每个源 Face 对应的连续 Node/Triangle 区间；
- `OcctShapeMeshData.GetFaceForNode()` / `GetFaceForTriangle()`：根据合并 Mesh 索引反查源 `OcctModelShape` Face；
- 来源追溯不增加新的 Native ABI，也不为每个 Triangle 额外保存一个 FaceId 数组。

这使合并 Mesh 可直接用于 Triangle 拾取、BIM/CAD 属性映射、Face 级结果着色、局部分析和选择性导出。详见 [Shape Mesh Face 来源追溯](MESH_PROVENANCE.zh-CN.md)。

### 文件交换

提供 STEP、IGES、BREP、STL 导入导出和按文件类型自动导入；STL 导出可控制离散参数。

## 纯 Managed 几何工具

`OcctGeometryExtensions` 提供点/向量计算、AABB、UV、距离结果、仿射变换，以及 `OcctModelLocation` 与 `OcctTransform3d` 双向转换，不要求加载 Native OCCT。详见 [Managed 几何与变换工具](GEOMETRY_UTILITIES.zh-CN.md)。

## Runtime 与所有权

- Engine/Session 内部使用 `SafeHandle`；
- Shape/Object 带 Owner Token，跨 Session 误用在进入 Native 前拒绝；
- `OcctRuntime.GetDiagnosticReport()` 为无副作用文本诊断；
- `OcctRuntime.GetDiagnosticInfo()` 返回结构化快照，本身不会配置或强制加载 Native；
- Native 错误统一转换为 `OcctException`。

详见 [结构化 Runtime 诊断](RUNTIME_DIAGNOSTICS.zh-CN.md)。

## 校验边界

GitHub 云端没有项目 OCCT SDK，因此 CI 重点校验：

- Native 声明、定义和 C# P/Invoke 一致，P/Invoke 均使用 Cdecl + ExactSpelling；
- API 数量来自 `bridge-contract.json`；
- Managed 项目编译并执行纯 Managed 回归测试；
- B-Spline、批量邻接、自由边界和 Mesh 来源追溯具有静态契约检查；
- Smoke 项目保持可编译；
- `main` / `demo` 共享封装逐项比较。

正式发布前仍必须在安装 OCCT 7.9.0 的 Windows 环境执行真实 Native 门禁：

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 根目录>"
```

Native Smoke 覆盖 ABI/版本、Boolean、批量邻接、严格自由边界、解析/微分几何、B-Spline、Mesh 来源追溯、OBB、Shape 身份、带孔 Face、Trim、Offset、Loft、Healing 和 BREP/STEP 往返。
