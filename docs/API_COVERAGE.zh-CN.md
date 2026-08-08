# OCCT Bridge 2.6 接口覆盖说明

OcctCSharpBridge 2.6 面向 Windows x64 与 OCCT 7.9.0，托管层明确分为两个入口：

- `OcctEngine`：面向桌面 CAD 的交互式 AIS / Viewer / 文档对象引擎。
- `OcctModelingSession`：面向批处理、服务端和算法调用的无界面建模内核。

桥接层明确不使用 OCAF/XDE。文档机制、撤销重做、业务实体和 JSON 持久化由上层应用负责。

- 原生桥接版本：`2.6.0`
- Native ABI：`3`
- OCCT：`7.9.0`
- Native exports: `336`
- Managed P/Invoke declarations: `336`
- Public .NET types: `82`
- Viewer API：`212`
- Modeling API：`124`

## 2.6 命名与封装规则

2.6 不再为了兼容保留同一能力的多套名称。

| 范围 | 统一规则 | 示例 |
|---|---|---|
| Shape 查询 | `GetShape...` / `IsShape...` / `SetShape...` | `GetShapeBounds()` |
| Edge 查询 | `GetEdge...` / `EvaluateEdge...` | `GetEdgeCurveType()` |
| Face 查询 | `GetFace...` / `EvaluateFace...` | `GetFaceUvBounds()` |
| 索引访问 | 使用 `...At` | `GetSubshapeAt()` |
| 构造 | `Make...` | `MakePlanarFace()` |
| 算法 | 直接使用操作动词 | `Extrude()`、`OffsetWire()` |
| 网格 | 使用 Triangulation 语义 | `Triangulate()`、`GetShapeMesh()` |
| C ABI | 保持唯一 `occt_...` 符号 | `occt_model_trim_edge` |

公开对象必须属于其创建的 `OcctEngine` 或 `OcctModelingSession`。不再允许业务代码通过一个裸 `long` 构造 `OcctShape` / `OcctModelShape`；持久化 ID 应通过 `GetShape()`、`TryGetShape()`、`GetObject()` 重新解析并绑定所有权。

C ABI 中的 0/1 标志不再直接暴露为公开 `int`。托管层使用 `bool` 和枚举，内部 Native DTO 负责 P/Invoke 布局转换。

## 程序集职责

| 程序集 | 职责 |
|---|---|
| `OcctNet` | 基础类型、交互式 Engine、Headless ModelingSession、运行时加载 |
| `OcctNet.WinForms` | 可复用 WinForms OCCT 视口宿主 |
| `OcctNet.Wpf` | 可复用 WPF OCCT 视口宿主 |

完整 WinForms / WPF / Avalonia CAD Demo 只位于 `demo` 分支，不进入 `main`。

## 交互式 `OcctEngine`

`OcctEngine` 管理 AIS 对象和 Viewer 上下文，适用于“创建后立即显示、选择、修改外观和交互”的桌面 CAD 文档场景。

覆盖相机与视图、屏幕/世界坐标转换、选择、对象生命周期、外观与材质、局部变换、交互式基本体/特征、文字和尺寸标注，以及 STEP/IGES/BREP/STL 交换。光照只保留强类型 `OcctSceneLightingSettings`，旧的简化光照接口和重复 C ABI 别名已在 ABI 3 删除。

## 无界面 `OcctModelingSession`

### 几何与拓扑构造

- 点、直线、多段线、圆、圆弧、正多边形、椭圆、Bezier、插值 B-Spline。
- 矩形 Wire 和矩形平面 Face。
- Wire、Compound、缝合 Shell、Shell 转 Solid。
- Box、Cylinder、Cone、Sphere、Torus、Wedge。
- `MakePlanarFace(outerWire, innerWires)` 可直接生成带孔平面，避免为板件孔洞额外做 Boolean Cut。

### Shape 查询与拓扑

- Shape 类型、方向、闭合性、有效性、最大容差、检查报告和哈希。
- AABB 和 `GetShapeOrientedBounds()` 定向包围盒 OBB。
- 线/面/体质量属性和 Shape 距离。
- Location 读取与写入。
- 通用子拓扑、外环、内环、祖先拓扑查询。
- 常用集合快捷接口：`GetVertices()`、`GetEdges()`、`GetWires()`、`GetFaces()`、`GetShells()`、`GetSolids()`、`GetCompSolids()`、`GetCompounds()`。
- 局部拓扑快捷接口：`GetEdgeVertices()`、`GetWireEdges()`、`GetFaceEdges()`、`GetFaceVertices()` 与 `GetTopologyCounts()`。
- `IsSameShape()` / `IsPartnerShape()` 直接暴露 OCCT 的拓扑身份语义。

### 几何与微分几何

- 顶点坐标、Edge 端点、归一化 Edge 求值。
- Curve / Surface 类型与解析几何参数。
- 原始曲线参数范围、导数、切向、法向、曲率和曲率中心。
- Face U/V 范围、周期性、偏导、法向、主曲率/平均曲率/高斯曲率。
- 点投影到 Edge/Face、射线相交、Solid 点分类。
- `TrimEdge()` 使用 OCCT 原始参数直接裁剪曲线 Edge。

### 建模算法

- Fuse、Cut、Common、Section、Splitter。
- Extrude、Revolve、Sweep、Loft。
- Edge Fillet / Chamfer。
- `OffsetShape()`：Shape/三维偏移。
- `OffsetWire()`：平面 Wire 偏移，支持 Arc / Tangent / Intersection 连接策略。
- Thick Solid、Same Domain 合并、Shape Healing 和算法历史追踪。

### 三角网格

`Triangulate()` 使用强类型 `OcctModelMeshParameters` 生成 OCCT Triangulation；`GetFaceMesh()` 返回单个 Face 网格；`GetShapeMesh()` 将所有 Face 网格合并为一个 `OcctMesh` 并修正三角形节点索引；`ClearTriangulation()` 清理缓存网格。

### 文件交换

提供 STEP、IGES、BREP、STL 导入导出和按文件类型自动导入；STL 导出可显式控制线性/角度离散精度。

## 纯 Managed 几何工具

`OcctGeometryExtensions` 围绕现有 Bridge 值类型增加不依赖 Native OCCT 的轻量级计算：

- 点插值以及点/向量带容差比较；
- 向量夹角、投影和正交分量；
- AABB 有效性、包含、相交、扩展、合并、体积和对角线长度；
- UV 参数范围有效性、中心和包含判断；
- 距离结果的分离向量、中点和容差判断；
- 仿射点/向量变换、矩阵组合、求逆、平移、旋转和均匀缩放；
- `OcctModelLocation` 与 `OcctTransform3d` 双向转换。

角度统一使用弧度。矩阵采用行优先仿射矩阵和列向量语义，`left.Multiply(right)` 表示先执行 `right`。详细示例见 [Managed 几何与变换工具](GEOMETRY_UTILITIES.zh-CN.md)。

## 生命周期与运行时

- `OcctEngine` 和 `OcctModelingSession` 内部使用 `SafeHandle`。
- 所有 Shape/Object 带内部 Owner Token。
- 跨 Engine / 跨 Session 误用在进入 Native 前直接拒绝。
- `OcctRuntime` 优先解析应用目录中的 Native 文件，并提供 `GetDiagnosticReport()` 排查 Win32 126 等部署问题。
- Native 错误统一转换为带 Operation 和 NativeMessage 的 `OcctException`。

## 校验边界

GitHub 云端没有 OCCT SDK，因此 CI 不伪装执行 Native 建模，而是校验：

- Native 声明与 C# P/Invoke 符号完全对应。
- 所有 P/Invoke 均为 Cdecl + ExactSpelling。
- API 数量严格读取 `bridge-contract.json`。
- 所有 Managed 项目编译，并执行纯 Managed 回归测试。
- Managed 几何与变换工具无需 OCCT Runtime 即可执行回归测试。
- `main` / `demo` 的共享封装内容逐项比较。

正式发布前必须在安装 OCCT 7.9.0 的 Windows 环境执行：

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 根目录>"
```

Native Smoke 覆盖 ABI/版本加载、Boolean、拓扑、解析/微分几何、OBB、Shape 身份、带孔 Face、Edge Trim、Wire Offset、整 Shape 网格、Loft、Healing 以及 BREP/STEP 往返。
