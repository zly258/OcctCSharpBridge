# OcctCSharpBridge

[English](README.md) · [中文文档](docs/zh-CN/README.md) · [English Docs](docs/en-US/README.md) · [第三方 SDK 接入](docs/zh-CN/09_第三方项目消费SDK.md) · [统一 Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge 3.0 是面向 CAD/BIM/工程应用的 **Open CASCADE Technology 7.9.0 → .NET** Bridge。

## 当前基线

| 项目 | 值 |
| --- | --- |
| Bridge | 3.0.0 |
| Native ABI | 仅 ABI 5 |
| OCCT | 7.9.0 |
| 构建 SDK | 稳定版 .NET 10 |
| Managed Target | .NET 8 / 9 / 10 |
| Windows UI | WinForms / WPF / Avalonia |
| Linux UI | Avalonia 源码构建 |
| 官方预编译 | Windows x64 |

`bridge-contract.json` 只保存机器可读的构建/分发事实，不再维护另一套冻结 API 清单。

## 构建

Windows：

```powershell
.\build.ps1 build Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux：

```bash
./build.sh build Release
```

## 发布与安装

仅编译：

```powershell
.\build.ps1 build Release -OcctRoot "D:\tools\occt-vc144-64"
```

安装/更新共享 Windows x64 SDK：

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

默认 SDK Root：`C:\Program Files\OcctCSharpBridge\SDK\3.0\win-x64`。可通过 `OCCTCSHARPBRIDGE_SDK` 或 `-InstallRoot` 覆盖。Windows Consumer 直接引用系统安装 SDK，不再需要仓库同步副本。

WPF/Avalonia 的 `HostState == Ready` 现在表示 Native Viewport 已获得有效尺寸并完成至少一次 `ResizeSurface + Redraw`。应用层不应再用鼠标移动或 Dispatcher 固定延时补首帧。

## 最小使用

```csharp
using OcctNet;

OcctRuntime.Configure();

using var model = new OcctModelingSession();
var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -5), OcctVector3d.UnitZ, 8, 20);
var cut = model.Cut(plate, hole);
model.ExportStep(cut.Shape, "plate.step");
```

Bridge 保持低层几何/建模/Viewer 封装。Document、Feature Tree、Command、Undo/Redo、捕捉、夹点、Catalog/业务语义和项目持久化属于上层应用。

## 使用 OcctCSharpBridge 的项目

### ModelScript

[ModelScript](https://github.com/zly258/ModelScript) 是基于 OcctCSharpBridge 与 OCCT 构建的 Windows x64 参数化 CAD 编辑器，支持类型化参数与表达式、JSON 模型文档、参数化命令与变换、交互式三维预览以及 STEP/IGES 导出。

- **WPF：**正式实现位于 [main](https://github.com/zly258/ModelScript/tree/main)，WPF 持续开发位于 [dev](https://github.com/zly258/ModelScript/tree/dev)。
- **Avalonia：**独立实现位于 [avalonia](https://github.com/zly258/ModelScript/tree/avalonia) 分支。
- 两套前端都消费 OcctCSharpBridge Binary SDK，不复制 Bridge 源码。

| WPF | Avalonia |
| --- | --- |
| [![ModelScript WPF 预览](https://raw.githubusercontent.com/zly258/ModelScript/dev/docs/screenshots/wpf-zh-CN.png)](https://github.com/zly258/ModelScript/tree/dev) | [![ModelScript Avalonia 预览](https://raw.githubusercontent.com/zly258/ModelScript/avalonia/docs/screenshots/avalonia-zh-CN.png)](https://github.com/zly258/ModelScript/tree/avalonia) |

## Demo

[demo](https://github.com/zly258/OcctCSharpBridge/tree/demo) 分支提供 WinForms、WPF、Avalonia 参考宿主。

生成的 `dist/`、`artifacts/`、Portable SDK 和发布压缩包不提交到源码分支。
