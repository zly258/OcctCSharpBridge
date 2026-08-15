# OcctCSharpBridge

[English](README.md) · [中文文档](docs/zh-CN/README.md) · [English Docs](docs/en-US/README.md) · [构建/测试说明](docs/zh-CN/08_构建测试与发布.md) · [Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo) · [跨平台 Avalonia](https://github.com/zly258/OcctCSharpBridge/tree/avalonia)

OcctCSharpBridge 是可复用的 **Open CASCADE Technology 7.9.0 → .NET 10 / C# 14** Bridge。`main` 统一维护正式 Native Core、Managed API、WinForms/WPF/Avalonia 视口宿主、测试、文档和各平台 Binary SDK 生产流程。

Bridge 3 **仅支持 ABI 5**。ABI 4 导出、兼容 Shim、旧 Handle、兼容性测试、旧 Consumer 契约和旧 Binary SDK 都不属于当前源码树。

> STEP/XDE 边界：Bridge 可以在 STEP 装配交换内部使用 XDE 保存产品结构、Occurrence Transform 与显示元数据，但不会把 OCAF/XDE 暴露成上层应用的 Document 或持久化架构。

## 当前源码契约

| 项目 | 当前源码 |
| --- | --- |
| Bridge | **3.0.0-preview.1** |
| Native ABI | **仅 ABI 5** |
| API Policy | **abi5-only** |
| OCCT | **7.9.0** |
| .NET SDK | **10.0.302** |
| Target Framework | **`net10.0` Core / `net10.0-windows` Desktop Adapter** |
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

Windows 推荐完整验证：

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

其它常用 target：

```powershell
.\build.ps1 validate Release
.\build.ps1 native Release
.\build.ps1 managed Release
.\build.ps1 test Release
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 clean
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

完整 target、6 个静态 Contract Checks、Managed Tests、Native Smoke、SDK 10.0.302 排障和发布说明见 [构建、测试与发布](docs/zh-CN/08_构建测试与发布.md)。

ABI5 契约检查会保证 Native 声明、实现和 Managed `LibraryImport` 一致，拒绝 pre-ABI5 Handle 与兼容遗留，并在仓库存在平台 Binary SDK 时检查其契约是否仍为 ABI5-only。

## Binary SDK

开发分支不保留 ABI4 Binary SDK。`build.ps1 dist` / `build.sh dist` 只从当前 ABI5 源码生成平台包。正式发布前必须验证 Package Contract、Manifest、Source Commit 与文件哈希，验证通过后才允许在 `main` 跟踪对应的 `dist/<rid>`。

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

- `main`：唯一正式 Bridge SDK 源码和 Binary SDK 生产分支。
- `main-dev`：ABI5 SDK 开发与校验，通过后 PR 到 `main`。
- `demo` / `demo-dev`：WinForms/WPF Consumer，不保存第二份 Core/Native 源码。
- `avalonia` / `avalonia-dev`：Windows/Linux Avalonia Consumer 与打包流程。

## 许可证

OcctCSharpBridge 使用 **GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0**。正式条款见 [LICENSE](LICENSE)、[LICENSE_LGPL_21.txt](LICENSE_LGPL_21.txt)、[OcctCSharpBridge_LGPL_EXCEPTION.txt](OcctCSharpBridge_LGPL_EXCEPTION.txt)、[COMMERCIAL.md](COMMERCIAL.md) 与 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。
