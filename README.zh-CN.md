# OcctCSharpBridge

[English](README.md) · [中文文档](docs/zh-CN/README.md) · [English Docs](docs/en-US/README.md) · [第三方 SDK 接入](docs/zh-CN/09_第三方项目消费SDK.md) · [Third-party SDK Guide](docs/en-US/09_Third-Party-SDK-Consumption.md) · [统一 Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge 是面向 Windows x64 / Linux x64 的可复用 **Open CASCADE Technology 7.9.0 → .NET 8-10 / C# 14** Bridge。`main` 统一维护正式 Native Core、Managed API、WinForms/WPF/Avalonia Adapter、测试、文档和各平台 SDK 生产流程。

Bridge 3 **仅支持 ABI 5**。ABI 4 导出、兼容 Shim、旧 Handle、旧兼容测试和旧 Binary SDK Payload 均不属于当前源码树。

> STEP/XDE 边界：Bridge 可以在 STEP 装配交换内部使用 XDE 保存产品结构、Occurrence Transform 与显示元数据，但不会把 OCAF/XDE 暴露成上层应用的 Document 或持久化架构。

## 当前契约

| 项目 | 契约 |
| --- | --- |
| Bridge | **3.0.0-preview.1** |
| Native ABI | **仅 ABI 5** |
| API Policy | **abi5-only** |
| OCCT | **7.9.0** |
| 构建 SDK | **稳定版 .NET 10 SDK，基线 `10.0.100`，`latestFeature` 滚动** |
| Binary SDK TFM | **`net8.0` Core/Avalonia · `net8.0-windows` WinForms/WPF** |
| Consumer 支持 | **.NET 8 / .NET 9 / .NET 10** |
| C# / Native | **14.0 / C++17** |
| UI Adapter | **WinForms / WPF / Avalonia** |
| 平台 | **Windows x64 / Linux x64** |

`bridge-contract.json` 是机器可读的契约事实源。Native Declaration/Definition 与 Managed `LibraryImport` 直接由源码检查，文档不维护容易漂移的硬编码 API 数量。

仓库明确区分**构建工具链**与**Consumer 运行时基线**：源码使用稳定版 .NET 10 SDK 编译 C# 14，而发布的 Managed Binary SDK 以 .NET 8 为最低 TFM，因此同一套 DLL 可供 .NET 8、.NET 9、.NET 10 应用引用。

## 架构

```text
你的 CAD / BIM 应用
  Document · Feature Tree · Command · Undo/Redo · Persistence
                 │
                 ▼
OcctNet.WinForms ─┐
OcctNet.Wpf      ─┼─> OcctNet -> ABI5 C API -> OcctNative -> OCCT 7.9.0
OcctNet.Avalonia ─┘
```

`OcctModelingSession` 负责 Headless 建模/拓扑资源；`OcctEngine` 负责 AIS/Viewer 展示与交互场景。Document、Feature Tree、命令体系、Undo/Redo、捕捉、夹点和项目持久化属于上层应用职责。

## SDK 生产分为两级

### Consumer 快速产物

当 Demo、集成测试或受控第三方项目只需要从一个明确源码提交生成最新 SDK 时，使用 `dist`：

```powershell
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

```bash
./build.sh dist Release
```

`dist` 会执行生成可追溯 Binary SDK 所需的源码/契约检查，然后构建 Native + Managed 并写入 `dist/<rid>`。它**不会运行** Consumer Matrix、ManagedTests、Core Native Smoke，也不会启动 WinForms/WPF/Avalonia 窗口 Smoke。

### Bridge 完整验证与正式发布

验证或发布 Bridge 本身时才运行完整 Gate：

```powershell
.\build.ps1 sdk Release -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

```bash
./build.sh all Release
./publish.sh
```

Windows `sdk` / publish 继续保留 .NET 8/9/10 Consumer 编译矩阵、ManagedTests、Core Native Smoke，以及 WinForms/WPF/Avalonia Viewport Smoke。Linux 正式发布保留 Headless 完整验证；需要图形显示的 Avalonia Smoke 仍作为显式 DISPLAY 测试。

Consumer 每次刷新 SDK 时不应重复执行 Bridge 完整 QA Gate。Demo 因此在缓存失效时只调用 `dist` 快路径，再依据 `sourceCommit` 和 SHA-256 接受产物。

## Binary SDK 与 Portable SDK

最小 Binary SDK 面向编译引用和受控自动化：

```text
Windows: dist/win-x64/
  OcctNative.dll
  OcctNet.dll
  OcctNet.WinForms.dll
  OcctNet.Wpf.dll
  OcctNet.Avalonia.dll
  bridge-contract.json
  bridge-manifest.json

Linux: dist/linux-x64/
  libOcctNative.so
  OcctNet.dll
  OcctNet.Avalonia.dll
  bridge-contract.json
  bridge-manifest.json
```

`dist/` 完全属于生成目录并由 Git 忽略。

应用部署和对外分发应使用 `publish.ps1` / `publish.sh` 生成的 **Portable SDK**，或使用从同一 `main` Source Commit 生成并经过审查的制品。Portable SDK 在 Managed DLL 之外增加 `runtime/`、`occt/resources/`、License/Notice 与递归 Package Manifest；它不负责捆绑第三方应用自身的 .NET Runtime。

第三方项目接入请直接阅读：[第三方项目消费 SDK](docs/zh-CN/09_第三方项目消费SDK.md)。

## 最小使用示例

```csharp
using OcctNet;

OcctRuntime.Configure();

using var model = new OcctModelingSession();
var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -5), OcctVector3d.UnitZ, 8, 20);
var cut = model.Cut(plate, hole);
model.ExportStep(cut.Shape, "plate.step");
```

采用 Portable SDK 布局部署时，应在第一次创建 `OcctEngine` 或 `OcctModelingSession` 前调用 `OcctRuntime.Configure()`。

## 分支职责

- `main`：正式 Bridge 源码与 Release SDK 生产分支；
- `main-dev`：Bridge 开发与候选验证；
- `demo`：正式 Binary/Portable SDK Consumer；
- `demo-dev`：开发 Consumer，通常跟随 `main-dev`；
- `website`：双语项目官网。

Binary SDK 与 Portable SDK 都属于生成制品，不提交到源码分支。第三方正式交付应消费经过审查的 `main` 产物，而不是 `main-dev` 开发包。
