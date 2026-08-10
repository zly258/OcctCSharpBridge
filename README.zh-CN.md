# OcctCSharpBridge Demo

[English](README.md) · [Main SDK](https://github.com/zly258/OcctCSharpBridge) · [API 覆盖](docs/API_COVERAGE.zh-CN.md)

## 项目描述

`demo` 分支是 **OcctCSharpBridge 2.6.0**、Open CASCADE Technology **7.9.0** 与 .NET SDK **10.0.302** 的 Windows x64 参考应用，分别提供 WinForms、WPF 与 Avalonia 三套 CAD Demo。它用于展示如何把可复用 Bridge 接入完整桌面应用，而不是把应用层 CAD 架构塞入 `main`。

该分支包含三个桌面程序和共享 Demo 层，用于演示命令分发、历史记录、本地化、对象树、属性面板、Runtime Diagnostics、文件交换以及常用 CAD 交互。几何、拓扑、建模、选择、Viewer、Mesh、P0 惯性属性、P1 结构化求交和 P2 Topology Reference 均来自共享 Bridge。

### 界面预览

<p align="center"><img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/winform-demo-zh.png" alt="WinForms Demo" width="88%"></p>
<p align="center"><img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/wpf-demo-zh.png" alt="WPF Demo" width="88%"></p>
<p align="center"><img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/avalonia-demo-zh.png" alt="Avalonia Demo" width="88%"></p>

## 安装指南

### 环境要求

- Windows x64
- .NET SDK `10.0.302`
- Visual Studio 2022 / MSVC 工具链
- CMake `3.21+`
- OCCT `7.9.0`，VC14 x64 目录结构

默认 OCCT 路径：

```text
D:\tools\occt-vc144-64
```

其它安装位置可使用 `-OcctRoot` 或 `OCCT_ROOT` 指定。

### 构建全部 Demo

```powershell
.\build.ps1 all Release
```

单独构建某个 UI：

```powershell
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 avalonia Release
```

不需要 OCCT SDK 的静态与 Managed 验证：

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
```

使用真实 OCCT Runtime 的 Native 验证：

```powershell
.\build.ps1 smoke Release
```

## 使用示例

使用分支自带 Runner 启动已构建程序：

```powershell
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

三套 Demo 使用同一 Bridge API 和共享 CAD 行为，只保留 UI Framework 的宿主差异。可用于验证 Primitive、Boolean/Feature、对象选择、Subshape Selection、属性、标注、STEP/IGES/BREP/STL、Topology Analysis、Mesh 与 Viewport Interaction。

底层 `OcctModelingSession` 可直接使用 P0–P2：

```csharp
using OcctNet;

using var model = new OcctModelingSession();

var box = model.MakeBox(100, 80, 20);
var inertia = model.GetVolumeInertiaProperties(box);

var first = model.MakeLine(new OcctPoint3d(0, 0, 0), new OcctPoint3d(100, 0, 0));
var second = model.MakeLine(new OcctPoint3d(50, -20, 0), new OcctPoint3d(50, 20, 0));
var intersections = model.IntersectEdges(first, second);

var faces = model.GetSubshapes(box, OcctShapeType.Face);
var reference = model.CreateTopologyReference(box, faces[0]);
var resolved = model.ResolveTopologyReference(box, reference);
```

## 项目结构

```text
src/OcctNative           共享 Native OCCT Bridge
src/OcctNet              共享 .NET Bridge，demo 中不可打包
src/OcctNet.WinForms     WinForms Viewport Host
src/OcctNet.Wpf          WPF Viewport Host
src/OcctNet.Avalonia     Avalonia Windows HWND Viewport Host
src/OcctDemo.Common      三套 Demo 的共享行为
src/OcctDemo.WinForms    CAD-Winform
src/OcctDemo.Wpf         CAD-WPF
src/OcctDemo.Avalonia    CAD-Avalonia
assets/previews           demo 分支专用界面预览
tests                     Bridge 公共契约 + Demo 专用检查
```

Demo 中的 Wrapper 和应用项目全部保持 `IsPackable=false`；NuGet SDK 打包只属于 `main`。

## Native 启动排查

如果启动时出现 `DllNotFoundException` 或 Win32 126，先检查应用目录中的 Native Runtime。诊断信息会明确显示类似：

```text
OcctNative.dll [缺失]
TKernel.dll [缺失]
```

发布包还可查看 `native-dependencies.txt`。应用启动和崩溃日志位于：

```text
%LOCALAPPDATA%\OcctCSharpBridge\Logs
```

Avalonia 当前通过 Windows 子 HWND 承载 Native Viewer，因此它是 Windows x64 Host，不代表 OCCT Viewer 已具备 Linux/macOS 跨平台后端。

## 贡献指南

1. 可复用 OCCT/Native/Managed 修改与 `main` 保持一致；Demo 独有 UI 与应用行为留在 `demo`。
2. 不增加兼容别名或重复旧 API；当前库按新库维护。
3. Demo Wrapper 与 Application Project 必须保持不可打包。
4. 三套 UI 的共享行为优先放在 `OcctDemo.Common`，不要分别复制业务逻辑。
5. 提交前运行 `build.ps1 validate`、`build.ps1 managed`；具备 OCCT 时再运行 `build.ps1 smoke` 和对应 Demo Build/Run。
6. 修改界面时保留 Demo 分支专用预览图和 Native Runtime 排查说明。

## 许可证

OcctCSharpBridge 使用 [PolyForm Noncommercial License 1.0.0](LICENSE)。

Open CASCADE Technology 及其它第三方依赖遵循各自许可证。
