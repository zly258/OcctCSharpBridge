# OcctCSharpBridge

[English](README.md) · [中文文档](docs/zh-CN/README.md) · [English Docs](docs/en-US/README.md) · [API 参考](docs/zh-CN/api/README.md) · [Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo) · [Website](https://github.com/zly258/OcctCSharpBridge/tree/website)

OcctCSharpBridge 是面向 Windows x64 的 **Open CASCADE Technology 7.9.0 → .NET 10 / C# 14** 桥接库，为 C# 提供强类型的 Headless 建模、拓扑与几何分析、网格、工程数据交换、AIS/Viewer 交互、一等点对象，以及彼此独立的 WinForms/WPF/Avalonia 视口宿主。

`main` 是唯一 Bridge 源码与 Binary SDK 生产分支。应用层的 Document、Feature Tree、Command/Tool、Undo/Redo、捕捉、夹点和项目持久化仍由上层 CAD/BIM 应用负责。

> STEP/XDE 边界：Bridge 会在 STEP 装配交换内部使用 XDE 保存真实产品结构、Occurrence Transform 与显示样式，但**不会把 OCAF/XDE 暴露成应用层 Document/持久化架构**。上层通过托管的 `OcctAssemblyDocument` 快照读取装配语义。

## 当前源码契约

| 项目 | 当前源码 |
| --- | --- |
| Bridge | **2.7.0** |
| Native ABI | **4** |
| Native exports / P/Invoke | **349 / 349** |
| Public .NET types | **117** |
| Viewer / Modeling API | **215 / 134** |
| OCCT | **7.9.0** |
| .NET SDK | **10.0.302** |
| Target Framework | **`net10.0-windows`** |
| C# / Native | **14.0 / C++17** |
| Avalonia | **12.1.0** |
| Platform | **Windows x64** |

`bridge-contract.json` 是**源码契约**的机器可读事实源。

### 当前已发布 Binary SDK

正式发布状态以仓库中实际跟踪的 `main/dist/win-x64` 为准。请读取 [`dist/win-x64/bridge-contract.json`](dist/win-x64/bridge-contract.json) 获取已发布 Bridge/ABI/API 契约，读取 [`dist/win-x64/bridge-manifest.json`](dist/win-x64/bridge-manifest.json) 获取对应源码 Commit 与文件哈希。

`publish.ps1` 只有在 Windows/MSVC + OCCT 7.9 的 Release 构建和校验成功后才会替换这些文件。文档不再额外硬编码一份“当前已发布版本”，避免每次发布后立即产生过期信息。

## 2.7 源码重点能力

- 一等 `OcctAssemblyDocument` / `OcctAssemblyNode` STEP-XDE occurrence 模型；
- 保留稳定 XDE 节点 ID、Assembly/Instance/Part 角色、Local/Global Transform、Visibility、Surface RGBA、Curve Color 和 Subshape Style；
- 合法的多 Solid Part 仍保持为一个 Part，不再按 Solid 数量错误拆成 `Part_###`；
- 几何未改变时，名称、颜色、透明度、显隐等非几何编辑可继续通过原始 XDE 文档 round-trip；
- 一等 `OcctPoint` / `OcctPointMarker`，Native 使用真正的 `AIS_Point`；
- WPF Resize 使用“不触发 redraw 的 Native surface resize + Render 优先级合并刷新”，不再从 `WM_PAINT` 强制重绘。

## 架构

```text
你的 CAD / BIM 应用
  Document · Feature Tree · Command/Tool · Undo/Redo · JSON
                 │
                 ▼
OcctNet.WinForms ─┐
OcctNet.Wpf      ─┼─> OcctNet -> stable C ABI -> OcctNative -> OCCT 7.9.0
OcctNet.Avalonia ─┘
```

`OcctModelingSession` 负责 Headless 建模/拓扑；`OcctEngine` 负责 AIS/Viewer 展示与交互场景。三个 UI Adapter 都直接依赖 `OcctNet`，互不引用。

## 构建

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 test Release
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 docs Release
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

## 正式发布 Binary SDK

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

Binary SDK 发布方向保持单向：

```text
main 必须干净且基于最新 origin/main
→ 生成中英文 API Reference
→ 构建并验证 Release Binary SDK
→ commit/push main 的 dist/win-x64
→ demo 用户在 demo 分支本地运行 sync.ps1
```

`demo/dist` 已加入 `.gitignore`，Demo 是 SDK 消费者，不再作为第二份 Binary SDK 仓库。

## 使用示例

```csharp
using OcctNet;

using var model = new OcctModelingSession();
var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -5), OcctVector3d.UnitZ, 8, 20);
var cut = model.Cut(plate, hole);
model.ExportStep(cut.Shape, "plate.step");
```

2.7 源码中的真实 STEP 装配读取：

```csharp
using var engine = new OcctEngine();
OcctAssemblyDocument assembly = engine.ImportStepDocument("assembly.step");
foreach (OcctAssemblyNode root in assembly.Roots)
{
    // 递归遍历 root.Children。
}
```

## 分支职责

- `main`：Bridge 源码、测试、文档和正式 Binary SDK 生产者。
- `demo`：WinForms/WPF/Avalonia 应用，消费当前已发布 SDK 的本地被忽略副本 `dist/win-x64`。
- `website`：静态官网 / GitHub Pages 源码。

## 许可证

OcctCSharpBridge 采用 **GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0**。商业应用和闭源应用可以通过 .NET Assembly Reference、Dynamic Linking、P/Invoke 或等效 Runtime Linking 使用 Bridge，应用不会仅因为这种使用方式而被要求采用 GNU LGPL。

**OcctCSharpBridge 本身**以及对外分发的 Bridge 修改/衍生版本仍需遵守 GNU LGPL 2.1。正式条款见 [LICENSE](LICENSE)、[LICENSE_LGPL_21.txt](LICENSE_LGPL_21.txt)、[OcctCSharpBridge_LGPL_EXCEPTION.txt](OcctCSharpBridge_LGPL_EXCEPTION.txt)、[COMMERCIAL.md](COMMERCIAL.md) 与 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。

Open CASCADE Technology 及其它第三方依赖继续分别遵循其自身许可证；OCCT 仍适用其自身 GNU LGPL 2.1 + Open CASCADE Exception。
