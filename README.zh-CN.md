# OcctCSharpBridge

[English](README.md) · [文档索引](docs/00_文档索引.md) · [API 覆盖](docs/03_API覆盖与设计约定.md) · [Demo 分支](https://github.com/zly258/OcctCSharpBridge/tree/demo)

## 项目描述

OcctCSharpBridge 是面向 Windows x64 的 **Open CASCADE Technology 7.9.0 → .NET 10** 桥接库，为 C# 提供强类型的 OCCT 建模、拓扑、几何分析、网格、数据交换、AIS/Viewer 交互以及 WinForms/WPF/Avalonia 视口宿主。

`main` 只负责 OCCT Bridge，不实现应用层 CAD Framework。Document、Feature Tree、Command、Tool、Undo/Redo、捕捉、夹点、项目持久化以及 OCAF/XDE 均不进入可复用 Bridge；完整 CAD 行为放在应用层，例如 `demo` 分支。

当前基础契约：

- Bridge `2.6.0`，Native ABI `3`
- OCCT `7.9.0`
- .NET SDK `10.0.302`
- Target Framework `net10.0-windows`
- C# `14.0`
- Windows x64

Managed API 有两个主要入口：

- `OcctEngine`：AIS/Viewer、显示对象、选择、相机、外观、标注和交互式视口操作。
- `OcctModelingSession`：无窗口建模、拓扑、算法、分析、网格、History 以及 STEP/IGES/BREP/STL 数据交换。

P0–P3 已覆盖完整惯性属性、结构化 Edge/Edge 求交、版本化拓扑引用，以及高数量集合的 Bulk-only Native ABI 传输。

## 安装指南

### 1. 环境要求

需要：

- Windows x64
- .NET SDK `10.0.302`
- Visual Studio 2022 / MSVC 工具链
- CMake `3.21+`
- Open CASCADE Technology `7.9.0`，VC14 x64 目录结构

仓库默认使用以下 OCCT 路径：

```text
D:\tools\occt-vc144-64
```

其它安装位置可使用 `-OcctRoot` 参数或 `OCCT_ROOT` 环境变量指定。

### 2. 构建 Managed 包

```powershell
.\build.ps1 pack Release
```

输出目录：

```text
artifacts\packages
```

生成四个包：

```text
OcctNet
OcctNet.WinForms
OcctNet.Wpf
OcctNet.Avalonia
```

引用本地构建的 Core 包：

```powershell
dotnet add package OcctNet --version 2.6.0 --source .\artifacts\packages
```

按 UI 技术选择对应 Host，例如 WPF：

```powershell
dotnet add package OcctNet.Wpf --version 2.6.0 --source .\artifacts\packages
```

Managed 包不会捆绑 `OcctNative.dll` 或 OCCT `TK*.dll`。应用发布时需要部署与 Managed Bridge 匹配的 Native Bridge 和 OCCT Runtime。

### 3. 使用源码构建完整 Bridge

默认 OCCT 路径存在时：

```powershell
.\build.ps1 all Release
```

其它 OCCT 路径：

```powershell
.\build.ps1 all Release -OcctRoot "E:\SDK\occt-7.9.0"
```

## 使用示例

### Headless 建模

```csharp
using OcctNet;

using var model = new OcctModelingSession();

var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(
    new OcctPoint3d(50, 40, -5),
    OcctVector3d.UnitZ,
    8,
    20);

var cut = model.Cut(plate, hole);
var inertia = model.GetVolumeInertiaProperties(cut.Shape);
var inspection = model.InspectShape(cut.Shape);

model.ExportStep(cut.Shape, "plate.step");
```

### 结构化 Edge/Edge 求交

```csharp
var first = model.MakeLine(
    new OcctPoint3d(0, 0, 0),
    new OcctPoint3d(100, 0, 0));

var second = model.MakeLine(
    new OcctPoint3d(50, -20, 0),
    new OcctPoint3d(50, 20, 0));

var intersections = model.IntersectEdges(first, second);
```

返回结果区分 `Point` 与 `Overlap`，并保留两条 Edge 的原生曲线参数范围。

### 拓扑引用

```csharp
var faces = model.GetSubshapes(cut.Shape, OcctShapeType.Face);
var reference = model.CreateTopologyReference(cut.Shape, faces[0]);

var resolved = model.ResolveTopologyReference(
    cut.Shape,
    reference);
```

Topology Reference 是由几何和拓扑特征组成的版本化指纹；运行时 Subshape Index 只作为低权重提示，不作为长期拓扑身份。

## 项目结构

```text
bridge-contract.json     版本、平台与 API 契约
src/OcctNative           C++17 OCCT Bridge 与 C ABI
src/OcctNet              核心 .NET 封装
src/OcctNet.WinForms     WinForms 视口宿主
src/OcctNet.Wpf          WPF 视口宿主
src/OcctNet.Avalonia     Avalonia Windows HWND 视口宿主
tests                    契约、Managed 回归与 Native Smoke
docs                     编号化技术文档
build.ps1                本地验证、构建、打包、Smoke 统一入口
```

`OcctNet.Avalonia` 当前通过 Windows 子 HWND 承载 Native Viewer，因此它是 Windows x64 Host，不代表 OCCT Viewer 已具备 Linux/macOS 跨平台后端。

## 本地验证

不需要 OCCT SDK 的静态和 Managed 验证：

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
```

真正的 Native 验证以本机 OCCT 7.9.0 SDK 为准：

```powershell
.\build.ps1 smoke Release
```

Native 使用 `/W4 /WX` 编译，并严格按 `bridge-contract.json`、OCCT 7.9.0 Header/Library 和 Native/PInvoke 对等关系进行检查。

## 贡献指南

提交修改时保持 Bridge 简洁、无兼容冗余：

1. 一个分支只处理一个明确职责。
2. 公共 API 使用强类型和 Owner-aware Handle，不增加 Legacy Alias 或兼容包装层。
3. 集合数据优先使用 Bulk C ABI，不使用重复的 `Count + At` 跨 ABI 调用。
4. OCCT 能力封装在 Bridge 内；Document、Command、Tool 等应用行为留在应用层。
5. 不向可复用 Bridge 引入 OCAF/XDE。
6. 提交前执行 `build.ps1 validate`、`build.ps1 managed`；本机具备 OCCT 时再执行 `build.ps1 smoke`。
7. `main` 中可复用源码变更需要同步到 `demo` 分支。

## 许可证

OcctCSharpBridge 使用 [PolyForm Noncommercial License 1.0.0](LICENSE)。

Open CASCADE Technology 及其它第三方依赖遵循各自许可证。
