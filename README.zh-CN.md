# OcctCSharpBridge

[English](README.md) · [中文文档](docs/zh-CN/README.md) · [English Docs](docs/en-US/README.md) · [完整 API 参考](docs/zh-CN/api/README.md) · [Demo 分支](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge 是面向 Windows x64 的 **Open CASCADE Technology 7.9.0 → .NET 10** 桥接库，为 C# 提供强类型的 OCCT 建模、拓扑、几何分析、网格、数据交换、AIS/Viewer 交互，以及三个彼此独立的 WinForms/WPF/Avalonia 视口宿主。

`main` 只负责可复用 Bridge。Document、Feature Tree、Command/Tool、Undo/Redo、捕捉、夹点、项目持久化以及 OCAF/XDE 不进入 Bridge。

## 当前契约

| 项目 | 当前值 |
| --- | --- |
| Author | **zly258** |
| Bridge Version | **2.6.0** |
| Native ABI | **4** |
| Native exports | **347** |
| Managed P/Invoke | **347** |
| Public .NET types | **110** |
| Viewer / Modeling API | **213 / 134** |
| Open CASCADE Technology | **7.9.0** |
| .NET SDK | **10.0.302** |
| Target Framework | **`net10.0-windows`** |
| C# | **14.0** |
| Native Bridge | **C++17** |
| Avalonia | **12.1.0** |
| Platform | **Windows x64** |

`bridge-contract.json` 是版本、平台与 API 数量的机器可读事实源。

## UI 宿主架构

```text
OcctNet.WinForms ─┐
OcctNet.Wpf      ─┼─> OcctNet -> OcctNative.dll -> OCCT
OcctNet.Avalonia ─┘
```

三个 UI Adapter 都只直接依赖 `OcctNet`，互相之间不允许项目引用。`OcctNet.Wpf` 使用 WPF 原生 `HwndHost` 直接承载 OCCT 渲染 HWND，不再启用或引用 Windows Forms。

## 构建

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 test Release
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

Managed NuGet：

```powershell
.\build.ps1 pack Release
```

完整中英文 Managed + Native API Reference：

```powershell
.\build.ps1 docs Release
```

## 已验证 Binary SDK

`dist/win-x64` 是可提交的正式二进制 SDK，不是普通 build 输出：

```powershell
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

生成内容：

```text
dist/win-x64/
├─ OcctNative.dll
├─ OcctNet.dll
├─ OcctNet.WinForms.dll
├─ OcctNet.Wpf.dll
├─ OcctNet.Avalonia.dll
├─ bridge-contract.json
└─ bridge-manifest.json
```

`bridge-manifest.json` 记录 Bridge/ABI/OCCT/.NET 契约、源码 Commit 和各文件 SHA-256。OCCT `TK*.dll` 与第三方 Runtime 不进入 Binary SDK 本体。

## 一键正式发布

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

正式发布流程统一为：

```text
main 工作区必须干净
→ 自动生成完整中英文 API Reference
→ 构建 Release Native + Managed SDK
→ 生成并校验 dist/win-x64
→ commit + push main
→ 临时 detached worktree 打开 demo
→ 只同步 dist/win-x64
→ 校验 Contract / Manifest / SHA-256
→ commit + push demo
```

Managed 回归测试和 Native Smoke 保留为显式 `build.ps1 test` / `build.ps1 smoke` 目标，不再阻塞 Binary SDK 发布。

## 使用示例

```csharp
using OcctNet;

using var model = new OcctModelingSession();
var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -5), OcctVector3d.UnitZ, 8, 20);
var cut = model.Cut(plate, hole);
model.ExportStep(cut.Shape, "plate.step");
```

`OcctModelingSession` 面向 Headless 建模/拓扑；`OcctEngine` 面向 AIS/Viewer 与交互式视口。

## 项目结构

```text
bridge-contract.json            版本、平台与 API 契约
src/OcctNative                  C++17 OCCT Bridge 与稳定 C ABI
src/OcctNet                     核心 .NET Bridge
src/OcctNet.WinForms            独立 WinForms 视口宿主
src/OcctNet.Wpf                 独立 WPF HwndHost 视口宿主
src/OcctNet.Avalonia            独立 Avalonia 12.1.0 Windows HWND 视口宿主
tests                           静态契约、Managed 回归、Native Smoke
tools/OcctApiDocsGenerator      全量中英文 Managed + Native API 文档生成器
docs/zh-CN                      中文专题文档 + API Reference
docs/en-US                      英文专题文档 + API Reference
dist/win-x64                    可提交的已验证 Binary SDK
build.ps1                       validate/build/test/pack/docs/dist 统一入口
publish.ps1                     API 文档 + Binary SDK + main→demo 正式发布入口
```

## 分支边界

`main` 是唯一 Bridge 源码生产者；`demo` 是 Binary SDK 消费者，不再复制 `src/OcctNative`、`src/OcctNet*` 或 Bridge 测试。其它项目也应直接消费已验证 Binary SDK，而不是 clone 后重新编译整个 Bridge。

## Author

**zly258**  
zhangly1403@gmail.com

## 许可证

OcctCSharpBridge 使用 [PolyForm Noncommercial License 1.0.0](LICENSE)。Open CASCADE Technology 及其它第三方依赖遵循各自许可证。
