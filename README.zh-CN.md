# OcctCSharpBridge

[English](README.md) · [中文文档](docs/zh-CN/README.md) · [English Docs](docs/en-US/README.md) · [API 参考](docs/zh-CN/api/README.md) · [Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo) · [跨平台 Avalonia](https://github.com/zly258/OcctCSharpBridge/tree/avalonia) · [Website](https://github.com/zly258/OcctCSharpBridge/tree/website)

OcctCSharpBridge `main` 是 **Open CASCADE Technology 7.9.0 → .NET 10 / C# 14** 的唯一正式 SDK 源，为 C# 提供强类型的 Headless 建模、拓扑与几何分析、网格、工程数据交换、AIS/Viewer 交互、一等点对象，以及彼此独立的 WinForms/WPF/Avalonia 视口宿主。

`demo` 与 `avalonia` 分支只负责 Consumer 示例和打包流程；正式的 `OcctNative`、`OcctNet` 与 UI Host 实现统一由 `main` 维护。

`main` 使用同一套 Native Core 构建 Windows/Linux，并且是各平台 Binary SDK 的唯一生产者。应用层的 Document、Feature Tree、Command/Tool、Undo/Redo、捕捉、夹点和项目持久化仍由上层 CAD/BIM 应用负责。

> STEP/XDE 边界：Bridge 会在 STEP 装配交换内部使用 XDE 保存真实产品结构、Occurrence Transform 与显示样式，但**不会把 OCAF/XDE 暴露成应用层 Document/持久化架构**。上层通过托管的 `OcctAssemblyDocument` 快照读取装配语义。

## 当前源码契约

| 项目 | 当前源码 |
| --- | --- |
| Bridge | **3.0.0-preview.1** |
| Native ABI | **5 当前 / 4 兼容** |
| Native exports / P/Invoke | **443 / 443** |
| ABI 5 / Legacy ABI 4 / Compatibility Extension | **23 / 419 / 1** |
| Public .NET types | **148** |
| Viewer / Modeling API | **292 / 151** |
| OCCT | **7.9.0** |
| .NET SDK | **10.0.302** |
| Target Framework | **`net10.0` Core/Tests/Smoke / `net10.0-windows` Desktop Adapter** |
| C# / Native | **14.0 / C++17** |
| UI Adapter | **WinForms / WPF / Avalonia** |
| 源码平台契约 | **cross-platform x64** |
| Binary SDK RID | **windows-x64 / linux-x64** |

`bridge-contract.json` 是 `main` **源码契约**的机器可读事实源。源码契约保持跨平台；每个平台打包时再将 `dist/<rid>/bridge-contract.json` 专门化为对应 RID。

### 当前 Binary SDK 发布状态

正式发布状态以仓库中实际跟踪的 `main/dist/win-x64` 与 `main/dist/linux-x64` 为准。每个平台包中的 `bridge-contract.json` 表示实际 Bridge/ABI/API 契约，`bridge-manifest.json` 记录对应源码 Commit 与文件哈希。

`publish.ps1` 与 `publish.sh` 只有在各自平台 Release 构建和校验通过后才会替换对应 Binary SDK。文档不再额外硬编码一份“当前已发布版本”，避免发布后立即产生过期信息。

## Bridge 3 Preview 重点能力

- 一等 `OcctAssemblyDocument` / `OcctAssemblyNode` STEP-XDE occurrence 模型；
- 保留稳定 XDE 节点 ID、Assembly/Instance/Part 角色、Local/Global Transform、Visibility、Surface RGBA、Curve Color 和 Subshape Style；
- 合法的多 Solid Part 仍保持为一个 Part，不再按 Solid 数量错误拆成 `Part_###`；
- 几何未改变时，名称、颜色、透明度、显隐等非几何编辑可继续通过原始 XDE 文档 round-trip；
- 一等 `OcctPoint` / `OcctPointMarker`，Native 使用真正的 `AIS_Point`；
- Shape/Mesh/Algorithm 采用拥有所有权的独立 Resource Handle，为 Headless 工作流提供明确 Native 生命周期边界；
- WPF Resize 使用“不触发 redraw 的 Native surface resize + Render 优先级合并刷新”，不再从 `WM_PAINT` 强制重绘。

## Windows Demo 预览

<p align="center">
  <img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/winform-demo-zh.png" alt="WinForms Demo 中文界面" width="49%" />
  <img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/wpf-demo-zh.png" alt="WPF Demo 中文界面" width="49%" />
</p>

完整 WinForms/WPF Demo 源码位于 [`demo`](https://github.com/zly258/OcctCSharpBridge/tree/demo) 分支；跨平台 Avalonia 独立维护在 [`avalonia`](https://github.com/zly258/OcctCSharpBridge/tree/avalonia) 分支。

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

`OcctModelingSession` 负责 Headless 建模/拓扑；`OcctEngine` 负责 AIS/Viewer 展示与交互场景。各 UI Adapter 都直接依赖 `OcctNet`，互不引用。

跨平台 Avalonia 示例与应用打包位于 `avalonia` 分支；正式 `OcctNet.Avalonia` Host 和两个 Native 平台后端仍统一由 `main` 维护。

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

Linux x64 使用同一源码树。`managed/test` 不要求本机安装 OCCT；只有 `native/smoke/avalonia-smoke/dist` 需要 OCCT Native SDK。消费者 SDK 输出到 `dist/linux-x64`：

```bash
./build.sh validate Release
./build.sh managed Release
./build.sh test Release
./build.sh all Release
./build.sh avalonia-smoke Release   # 需要 X11/XWayland DISPLAY
./build.sh dist Release
```

普通 `all` 保持 Headless，可在无图形桌面的 Linux 主机上执行；`avalonia-smoke` 是显式 Viewer 测试，因为当前 OCCT 7.9 Linux Viewer 后端使用 X11/XWayland，尚未直接支持原生 Wayland Surface。

## 正式发布 Binary SDK

Windows：

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

Linux x64：

```bash
./publish.sh
```

两个发布流程都会在更新对应 `dist/<rid>` 之前校验包级 contract、manifest、源码 Commit 与文件哈希。Consumer 分支只同步这些 SDK，不再携带第二份 Core/Native 源码。

## 使用示例

```csharp
using OcctNet;

using var model = new OcctModelingSession();
var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -5), OcctVector3d.UnitZ, 8, 20);
var cut = model.Cut(plate, hole);
model.ExportStep(cut.Shape, "plate.step");
```

当前 Bridge 3 Preview 源码中的真实 STEP 装配读取：

```csharp
using var engine = new OcctEngine();
OcctAssemblyDocument assembly = engine.ImportStepDocument("assembly.step");
foreach (OcctAssemblyNode root in assembly.Roots)
{
    // 递归遍历 root.Children。
}
```

## 分支职责

- `main`：唯一 Bridge SDK 源码、全部 UI Host Adapter、测试、文档及 `win-x64`/`linux-x64` Binary SDK 生产者。
- `demo`：Windows WinForms/WPF Demo，消费当前已发布 SDK 的本地被忽略副本 `dist/win-x64`。
- `avalonia`：消费 `main` SDK 的 Windows/Linux Avalonia 示例、打包脚本、运行脚本和预览资源。
- `website`：静态官网 / GitHub Pages 源码。

## 许可证

OcctCSharpBridge 采用 **GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0**。商业应用和闭源应用可以通过 .NET Assembly Reference、Dynamic Linking、P/Invoke 或等效 Runtime Linking 使用 Bridge，应用不会仅因为这种使用方式而被要求采用 GNU LGPL。

**OcctCSharpBridge 本身**以及对外分发的 Bridge 修改/衍生版本仍需遵守 GNU LGPL 2.1。正式条款见 [LICENSE](LICENSE)、[LICENSE_LGPL_21.txt](LICENSE_LGPL_21.txt)、[OcctCSharpBridge_LGPL_EXCEPTION.txt](OcctCSharpBridge_LGPL_EXCEPTION.txt)、[COMMERCIAL.md](COMMERCIAL.md) 与 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。

Open CASCADE Technology 及其它第三方依赖继续分别遵循其自身许可证；OCCT 仍适用其自身 GNU LGPL 2.1 + Open CASCADE Exception。
