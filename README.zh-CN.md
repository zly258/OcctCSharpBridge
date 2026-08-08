# OcctCSharpBridge

[English](README.md) · [桌面 Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge 是面向 Windows x64 的 **Open CASCADE Technology 7.9.0 → .NET 8** 桥接项目。`main` 分支只保留可复用的 C++ Bridge、严格 C ABI、类型安全 C# 封装、WinForms/WPF 可复用视口宿主、接口契约测试、Native Smoke 项目以及 Managed SDK 打包定义；完整 CAD 应用位于 `demo` 分支。

**Bridge 2.6.0 / ABI 3** 是一次破坏性收口版本：删除兼容别名和公开裸 ID 构造方式，公开配置不再暴露 0/1 Native 标志位，统一接口命名，并补充 OBB、拓扑身份判断、带孔平面、精确 Edge 裁剪、平面 Wire Offset 和整 Shape Mesh 等 Headless 能力。

桥接层明确不使用 OCAF/XDE。Document、Entity、Command、Undo/Redo、JSON 持久化、Tool、捕捉等应用职责由上层实现。

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
docs                    API 覆盖、快速开始、打包与 Runtime 说明
build.ps1               校验、构建、打包、Smoke 统一入口
```

托管层明确保留两个职责不同的入口：

- `OcctEngine`：交互式 CAD/AIS/Viewer 会话，负责显示对象、选择、外观、相机、交互、标注以及直接进入当前 CAD 文档的交互式几何。
- `OcctModelingSession`：无界面建模内核，负责批处理、服务端和算法场景中的几何、拓扑、建模算法、网格、分析、修复、历史和工程文件交换。

两者可能拥有等价的 OCCT 能力，因为对象模型和使用场景不同；但 **同一个 façade 内不再保留旧名+新名两套接口**。

交互对象统一使用 `IOcctObject`，提供 `Id`、`Kind`、`IsValid`；具体实例仅为 `OcctShape`、`OcctText`、`OcctDimension`。不再存在通用 `OcctObject` 包装，也没有公开裸 ID 构造器。加上 Headless 类型，目前可复用 SDK 共 **81 个公开 .NET 类型**。

## 统一命名规则

- Shape 查询：`GetShape...`、`IsShape...`、`SetShape...`
- Edge 查询：`GetEdge...`、`EvaluateEdge...`、`TrimEdge()`
- Face 查询：`GetFace...`、`EvaluateFace()`
- 索引访问：`...At`，如 `GetSubshapeAt()`
- 构造：`Make...`
- 算法：`Extrude()`、`OffsetShape()`、`OffsetWire()` 等操作动词
- 网格：`Triangulate()`、`ClearTriangulation()`、`GetFaceMesh()`、`GetShapeMesh()`

所有 Shape/Object 都绑定其所属 Engine/Session。持久化 ID 必须通过 `GetShape()`、`TryGetShape()`、`GetObject()`、`TryGetObject()` 解析，不能再用一个 `long` 伪造托管对象。

## Headless 建模示例

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
var mesh = model.GetShapeMesh(cut.Shape);
model.ExportStep(cut.Shape, "plate.step");
```

带孔平面和 Wire Offset 可直接构造：

```csharp
var outer = model.MakeRectangleWire(100, 80);
var inner = model.MakeRectangleWire(20, 20, new OcctPoint3d(40, 30, 0));
var face = model.MakePlanarFace(outer, new[] { inner });
var offset = model.OffsetWire(outer, 5.0, joinType: OcctJoinType.Arc);
```

完整能力分类见 [中文 API 覆盖说明](docs/API_COVERAGE.zh-CN.md)，快速接入流程见 [快速开始](docs/GETTING_STARTED.zh-CN.md)。

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
| `smoke` | 构建并真实执行 OCCT Native 建模 | 是 |
| `all` | 构建 Native + 可复用 Managed Host | 是 |

没有 OCCT SDK 时，提交前执行：

```powershell
.\build.ps1 ci Release
```

仅在 `main` 分支生成三套 Managed SDK 包：

```powershell
.\build.ps1 pack Release
```

输出位于 `artifacts/packages`。版本统一来自 `bridge-contract.json`，包内包含 XML IntelliSense 文档和符号包，并自动检查不得包含 `OcctNative.dll`、OCCT `TK*.dll` 或 `runtimes/` Native Payload。**NuGet 仅属于 main SDK 分支**；`demo` 分支明确设置为不可打包，只负责完整桌面应用和 Native 发布。详见 [打包与运行时部署](docs/PACKAGING.zh-CN.md)。

正式发布前必须执行：

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

GitHub 托管环境无法配置本项目真实 OCCT SDK，所以仓库不再保留长期 skipped 的 Native 云端 workflow。Native 真实执行明确作为本地发布门禁，云端负责完整 Managed/静态契约以及 main 分支 Managed 包校验。

## 运行时部署

`OcctNet.dll`、对应 UI Host、`OcctNative.dll`、OCCT DLL 和第三方 DLL 必须来自**同一 Bridge 构建**，不要混用不同 ABI 文件。

`OcctRuntime.GetDiagnosticReport()` 会输出 Native 候选路径、OCCT 路径和资源环境变量，用于排查 Win32 126 等部署问题。

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

Demo 发布脚本会复制应用本地 Native 依赖，并在生成最终包前执行 Native LoadLibrary 探针。Demo 中共享项目明确 `IsPackable=false`，不会承担 NuGet SDK 发布职责。

## 其它项目引用

开发阶段可以直接 ProjectReference：

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
  <!-- 可选 -->
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.WinForms\OcctNet.WinForms.csproj" />
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.Wpf\OcctNet.Wpf.csproj" />
</ItemGroup>
```

如需本地 NuGet 源，在 `main` 执行 `build.ps1 pack`，再把 `artifacts/packages` 添加为 NuGet Source。业务应用仍需自行部署匹配版本的 Native Bridge/OCCT Runtime。

## 2.6 契约

权威值来自 `bridge-contract.json`：

- Bridge：`2.6.0`
- Native ABI：`3`
- OCCT：严格 `7.9.0`
- Target：`.NET 8` / Windows x64
- Native exports：`336`
- Managed P/Invoke：`336`
- Public .NET types：`81`
- Viewer API：`212`
- Modeling API：`124`

`build.ps1 validate` 会在 API 数量、Native/PInvoke 映射、命名和职责边界、版本、SDK/包策略或文档漂移时直接失败。

## 常见问题

**提示 `OCCT_ROOT is not configured`**  
设置 `$env:OCCT_ROOT` 或通过 `-OcctRoot` 指定。

**找不到 `TKernel.lib` / `TKernel.dll`**  
检查 `win64\vc14\lib`、`win64\vc14\bin` 和 OCCT 7.9.0 版本。

**Managed 编译通过，但 Native 加载失败**  
Managed 编译和 NuGet 包都不会自动部署 OCCT。请使用 Demo Publish，或将匹配的 Native/OCCT/第三方依赖闭包放在 EXE 目录。

**需要完整可运行 CAD 示例**  
切换 `demo` 分支，不要把应用层 Document/Tool 逻辑塞进 `main`。

## License

项目采用 [PolyForm Noncommercial License 1.0.0](LICENSE)。Open CASCADE Technology 及第三方组件遵循各自许可证。

## 联系方式

Liaoyuan Zhang · [zhangly1403@gmail.com](mailto:zhangly1403@gmail.com)