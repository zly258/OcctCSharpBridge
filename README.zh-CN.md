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
.\build.ps1 managed Release
.\build.ps1 test Release
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux：

```bash
./build.sh managed Release
./build.sh test Release
./build.sh all Release
```

日常验证保持简单：

- 编译负责发现项目/API 结构问题；
- ManagedTests 验证托管行为和必要 ABI Layout；
- 一个 Modeling Smoke 验证 Native Bridge + OCCT 主链；
- 一个 Avalonia Viewer Smoke 验证跨平台 Viewer 生命周期；
- WinForms/WPF 只做真实项目编译，不再维护重复 GUI Smoke。

## 分发

快速 Binary SDK：

```powershell
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

正式 Windows Portable SDK：

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

发布只执行 Release 构建/测试/Smoke、打包，以及一次移除开发机 OCCT 路径后的 .NET 10 隔离包运行检查。

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

## Demo

[demo](https://github.com/zly258/OcctCSharpBridge/tree/demo) 分支提供 WinForms、WPF、Avalonia 参考宿主。

生成的 `dist/`、`artifacts/`、Portable SDK 和发布压缩包不提交到源码分支。
