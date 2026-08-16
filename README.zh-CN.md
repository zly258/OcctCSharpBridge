# OcctCSharpBridge

[English](README.md) · [中文文档](docs/zh-CN/README.md) · [English Docs](docs/en-US/README.md) · [构建/测试说明](docs/zh-CN/08_构建测试与发布.md) · [统一 Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge 是可复用的 **Open CASCADE Technology 7.9.0 → .NET 10 / C# 14** Bridge。`main` 统一维护正式 Native Core、Managed API、WinForms/WPF/Avalonia Adapter、测试、文档和各平台 Binary SDK 生产流程。

Bridge 3 **仅支持 ABI 5**。ABI 4 导出、兼容 Shim、旧 Handle、兼容性测试、旧 Consumer 契约和旧 Binary SDK 都不属于当前源码树。

> STEP/XDE 边界：Bridge 可以在 STEP 装配交换内部使用 XDE 保存产品结构、Occurrence Transform 与显示元数据，但不会把 OCAF/XDE 暴露成上层应用的 Document 或持久化架构。

## 当前源码契约

| 项目 | 当前源码 |
| --- | --- |
| Bridge | **3.0.0-preview.1** |
| Native ABI | **仅 ABI 5** |
| API Policy | **abi5-only** |
| OCCT | **7.9.0** |
| .NET SDK | **精确 10.0.303** |
| Target Framework | **`net10.0` Core/Avalonia · `net10.0-windows` WinForms/WPF** |
| C# / Native | **14.0 / C++17** |
| UI Adapter | **WinForms / WPF / Avalonia** |
| 源码平台 | **Windows x64 / Linux x64** |

`bridge-contract.json` 是机器可读的唯一契约事实源。Native Declaration、Definition 与 Managed `LibraryImport` 的 API Surface 由 `tests/check-api-surface.ps1` 直接从当前源码校验；README 和 docs 不维护容易失真的硬编码接口数量或生成式 API Reference。

## 架构

```text
你的 CAD / BIM 应用
  Document · Feature Tree · Command/Tool · Undo/Redo · JSON
                 │
                 ▼
OcctNet.WinForms ─┐
OcctNet.Wpf      ─┼─> OcctNet -> ABI5 C API -> OcctNative -> OCCT 7.9.0
OcctNet.Avalonia ─┘
```

`OcctModelingSession` 负责 Headless 建模/拓扑资源；`OcctEngine` 负责 AIS/Viewer 展示与交互场景。各 UI Adapter 直接依赖 `OcctNet`，互不引用。

Document、Feature Tree、Command/Tool、Undo/Redo、捕捉、夹点和项目持久化仍属于上层应用职责。

## 构建与校验

Windows 完整验证：

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

Windows Binary SDK：

```powershell
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux x64：

```bash
./build.sh validate Release
./build.sh managed Release
./build.sh test Release
./build.sh all Release
./build.sh avalonia-smoke Release
./build.sh dist Release
```

完整 Target、静态 Contract Checks、Managed Tests、Native Smoke、SDK 10.0.303 排障和发布说明见 [构建、测试与发布](docs/zh-CN/08_构建测试与发布.md)。

## Binary SDK 策略

`dist/win-x64` 与 `dist/linux-x64` 是生成的 Release 构建产物，不是源码仓库内容。源码分支**不提交 Binary SDK 文件**。每个包通过 `bridge-manifest.json` 记录 Source Commit 与 SHA-256，并在本地消费或外部分发前完成校验。

统一 `demo` 分支按 `sourceCommit` 与 Manifest Hash 消费这些 SDK。正式二进制可通过受审查的 GitHub Release Asset 或其它受控制品渠道发布，不需要 GitHub Actions 流水线。

## 使用示例

```csharp
using OcctNet;

using var model = new OcctModelingSession();
var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -5), OcctVector3d.UnitZ, 8, 20);
var cut = model.Cut(plate, hole);
model.ExportStep(cut.Shape, "plate.step");
```

## 分支职责

- `main`：唯一正式 Bridge SDK 源码与 Binary SDK 生产分支。
- `main-dev`：Bridge SDK 开发与校验，通过后 PR 到 `main`。
- `demo` / `demo-dev`：唯一 Binary SDK Consumer；Windows x64 提供 WinForms、WPF、Avalonia，Linux x64 仅提供 Avalonia。
- `website`：双语项目官网。
- `backup/*`：历史备份分支，保持不变。

独立 `avalonia` / `avalonia-dev` 分支的有效内容全部并入统一 Demo 后废弃。

## 许可证

OcctCSharpBridge 使用 **GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0**。正式条款见 [LICENSE](LICENSE)、[LICENSE_LGPL_21.txt](LICENSE_LGPL_21.txt)、[OcctCSharpBridge_LGPL_EXCEPTION.txt](OcctCSharpBridge_LGPL_EXCEPTION.txt)、[COMMERCIAL.md](COMMERCIAL.md) 与 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。
