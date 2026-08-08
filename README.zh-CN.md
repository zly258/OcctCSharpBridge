# OcctCSharpBridge

[English](README.md) · [文档索引](docs/INDEX.zh-CN.md) · [桌面 Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge 是面向 Windows x64 的 **Open CASCADE Technology 7.9.0 → .NET 8** 桥接项目。`main` 分支只保留可复用 C++ Bridge、严格 C ABI、类型安全 C# 封装、WinForms/WPF 可复用视口宿主、契约测试、Native Smoke 场景和 Managed SDK 打包；完整 CAD 应用位于 `demo` 分支。

**Bridge 2.6.0 / ABI 3** 已完成接口收口与能力扩展：删除兼容别名和公开裸 ID 构造方式，统一命名/所有权，并补充批量拓扑邻接、严格自由边界、B-Spline Curve/Surface 数据读取、Mesh Face 来源追溯、结构化 Runtime 诊断、OBB、精确 Trim、Wire Offset、带孔 Face、整 Shape Triangulation、Healing 与工程文件交换等能力。

桥接层明确不使用 OCAF/XDE。Document、Entity、Command、Tool、Undo/Redo、JSON 持久化、捕捉等应用职责由上层实现。

## 环境要求

- Windows x64
- Visual Studio 2022 / MSVC v143 兼容工具链
- .NET SDK **8.0.423**（`global.json` 固定）
- C# 12.0
- CMake 3.21+
- Open CASCADE Technology **7.9.0**，VC14 x64 目录结构
- PowerShell 5.1+ 或 PowerShell 7+

典型 OCCT 目录：

```text
D:\tools\occt-vc144-64\
├─ inc\
├─ win64\vc14\bin\
├─ win64\vc14\lib\
└─ 3rdparty-vc14-64\
```

## 仓库结构

```text
bridge-contract.json    Bridge/ABI/OCCT/.NET/API 唯一契约来源
global.json             固定 .NET SDK
Directory.Build.props   C# 编译策略
src/OcctNative          C++17 Bridge 与 C ABI
src/OcctNet             C# 核心封装
src/OcctNet.WinForms    可复用 WinForms 视口宿主
src/OcctNet.Wpf         可复用 WPF 视口宿主
tests                   契约测试、Managed Test、Native Smoke
docs                    结构化 API/集成/Runtime 文档
build.ps1               校验、构建、打包、Smoke 统一入口
```

托管层明确保留两个职责不同的入口：

- `OcctEngine`：交互式 CAD/AIS/Viewer 会话，负责显示对象、选择、外观、相机、交互和标注；
- `OcctModelingSession`：无界面建模内核，负责批处理、服务端、几何、拓扑、算法、网格、分析、修复、历史和工程文件交换。

同一个 façade 内不再保留旧名+新名两套接口。当前可复用 SDK 共 **90 个公开 .NET 类型**。

## 统一 API 规则

- Shape：`GetShape...`、`IsShape...`、`SetShape...`
- Edge：`GetEdge...`、`EvaluateEdge...`、`TrimEdge()`
- Face：`GetFace...`、`EvaluateFace...`
- 索引访问：`...At`
- 构造：`Make...`
- 算法：`Extrude()`、`OffsetShape()`、`OffsetWire()` 等操作动词
- Mesh：`Triangulate()`、`GetFaceMesh()`、`GetShapeMesh()`、`GetShapeMeshData()`
- 分析：`AnalyzeEdgeAdjacency()`、`AnalyzeFreeBounds()`

所有 Shape/Object 都绑定所属 Engine/Session。持久化 ID 必须通过 `GetShape()`、`TryGetShape()`、`GetObject()`、`TryGetObject()` 解析，不能用裸 `long` 伪造托管对象。

## Headless 示例

```csharp
using var model = new OcctModelingSession();

var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(
    new OcctPoint3d(50, 40, -5),
    OcctVector3d.UnitZ,
    8,
    20);

var cut = model.Cut(plate, hole);
var bounds = model.GetShapeOrientedBounds(cut.Shape, optimal: true);
var topology = model.AnalyzeEdgeAdjacency(cut.Shape);
var meshData = model.GetShapeMeshData(cut.Shape);
model.ExportStep(cut.Shape, "plate.step");
```

### 批量拓扑与严格自由边界

```csharp
var adjacency = model.AnalyzeEdgeAdjacency(cut.Shape);
var boundaryCandidates = adjacency.BoundaryCandidates;
var nonManifold = adjacency.NonManifoldEdges;

var freeBounds = model.AnalyzeFreeBounds(cut.Shape, tolerance: 1e-6);
```

`AnalyzeEdgeAdjacency()` 对整个 Root Shape 一次构建 Edge→不同 Face 的 Native 索引，适合大型 STEP/BIM 模型；真正判断 Shell 是否存在开口时再使用 `AnalyzeFreeBounds()`。

### Mesh Face 来源追溯

```csharp
var meshData = model.GetShapeMeshData(cut.Shape);
if (meshData.TryGetFaceForTriangle(hitTriangleIndex, out var sourceFace))
{
    // 根据源 Face 查询 CAD/BIM 属性、做选择、分析或局部导出。
}
```

`GetShapeMesh()` 继续保持兼容；`GetShapeMeshData()` 额外记录每个源 Face 对应的连续 Node/Triangle 区间，不增加 Native ABI，也不为每个 Triangle 单独保存 FaceId。

### B-Spline 曲线与曲面

```csharp
var curveData = model.GetBSplineCurveData(edge);
var surfaceData = model.GetBSplineSurfaceData(face);
```

可读取 Degree、Pole、Weight、Knot、Multiplicity，以及 Surface U/V 控制网格。

完整分类见 [文档索引](docs/INDEX.zh-CN.md)。

## 构建与校验

需要 Native 能力时设置本机 OCCT：

```powershell
git clone https://github.com/zly258/OcctCSharpBridge.git
cd OcctCSharpBridge
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"
```

统一命令：

```powershell
.\build.ps1 <target> <configuration> [-OcctRoot <path>]
```

| Target | 作用 | 需要 OCCT SDK |
|---|---|---|
| `validate` | API/版本/目录/PInvoke/UI Host/SDK 包契约检查 | 否 |
| `managed` | 构建可复用 Managed 封装与 Host | 否 |
| `pack` | 构建并校验本地 Managed NuGet 与符号包 | 否 |
| `ci` | 契约检查 + Managed 构建/Test + Smoke 编译 + 包校验 | 否 |
| `native` | CMake/MSVC 构建 `OcctNative.dll` | 是 |
| `smoke` | 构建并真实执行 OCCT Native 场景 | 是 |
| `all` | 构建 Native + 可复用 Managed Host | 是 |

没有 OCCT SDK 时，提交前执行：

```powershell
.\build.ps1 ci Release
```

仅在 `main` 分支生成三套 Managed SDK 包：

```powershell
.\build.ps1 pack Release
```

输出位于 `artifacts/packages`。版本来自 `bridge-contract.json`，包内包含 XML IntelliSense 文档和符号包，并自动检查不得包含 `OcctNative.dll`、OCCT `TK*.dll` 或 `runtimes/` Native Payload。**NuGet 只属于 main SDK 分支**；`demo` 分支共享项目保持不可打包，只负责完整桌面应用和 Native 发布。

正式发布前执行真实 Native 门禁：

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

GitHub 托管环境没有本项目真实 OCCT SDK，因此云端负责静态契约、Managed 构建/Test、Smoke 源码编译和 NuGet 包校验；真实 OCCT 几何/拓扑执行仍由本地 Native Smoke 保证。

## Runtime 部署与诊断

`OcctNet.dll`、对应 UI Host、`OcctNative.dll`、OCCT DLL 和第三方 DLL 必须来自**同一 Bridge 构建**。

`OcctRuntime.GetDiagnosticReport()` 是无副作用文本诊断；`OcctRuntime.GetDiagnosticInfo()` 返回 app-local/环境配置/实际 Loaded 的 `OcctNative.dll`、`TKernel.dll` 路径和存在状态、进程架构等结构化信息，调用本身不会配置或强制加载 Runtime。详见 [结构化 Runtime 诊断](docs/RUNTIME_DIAGNOSTICS.zh-CN.md)。

## 桌面 Demo

`main` 不放完整 CAD 应用。WinForms / WPF / Avalonia Demo 位于 `demo`：

```powershell
git switch demo
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"
.\build.ps1 all Release
.\run.ps1 winform
.\run.ps1 wpf
.\run.ps1 avalonia
```

Demo 发布脚本负责应用本地 Native 依赖和 Native Load 探针；Demo 中共享项目明确 `IsPackable=false`。

## 其它项目引用

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
  <!-- 可选 -->
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.WinForms\OcctNet.WinForms.csproj" />
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.Wpf\OcctNet.Wpf.csproj" />
</ItemGroup>
```

如需本地 NuGet 源，在 `main` 执行 `build.ps1 pack`，再将 `artifacts/packages` 添加为 NuGet Source。业务应用仍需部署匹配版本的 Native Bridge/OCCT Runtime。

## 2.6 契约

权威值来自 `bridge-contract.json`：

- Bridge：`2.6.0`
- Native ABI：`3`
- OCCT：严格 `7.9.0`
- Target：`.NET 8` / Windows x64
- Native exports：`345`
- Managed P/Invoke：`345`
- Public .NET types：`90`
- Viewer API：`212`
- Modeling API：`133`

`build.ps1 validate` 会在 API 数量、Native/PInvoke 映射、命名/职责边界、版本、SDK/包策略或文档漂移时直接失败。

## 常见问题

**提示 `OCCT_ROOT is not configured`**  
设置 `$env:OCCT_ROOT` 或通过 `-OcctRoot` 指定。

**找不到 `TKernel.lib` / `TKernel.dll`**  
检查 `win64\vc14\lib`、`win64\vc14\bin` 和 OCCT 7.9.0。

**Managed 编译通过，但 Native 加载失败**  
Managed 编译和 NuGet 包不会自动部署 OCCT。使用 Demo Publish 或将匹配的 Native/OCCT/第三方依赖放在 EXE 目录，并查看 `OcctRuntime.GetDiagnosticInfo()`。

**需要完整可运行 CAD 示例**  
切换 `demo` 分支，不要把应用层 Document/Tool 逻辑塞进 `main`。

## License

项目采用 [PolyForm Noncommercial License 1.0.0](LICENSE)。Open CASCADE Technology 及第三方组件遵循各自许可证。

## 联系方式

Liaoyuan Zhang · [zhangly1403@gmail.com](mailto:zhangly1403@gmail.com)
