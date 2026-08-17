# OcctCSharpBridge Demo

[English](README.md) · [Main SDK](https://github.com/zly258/OcctCSharpBridge/tree/main)

`demo` 是唯一的 Binary SDK Consumer 分支，`main` 是唯一的 Bridge SDK 源。

```text
OcctDemo.Common
├─ OcctDemo.WinForms  → Windows x64
├─ OcctDemo.Wpf       → Windows x64
└─ OcctDemo.Avalonia  → Windows x64 / Linux x64
```

| 平台 | WinForms | WPF | Avalonia |
|---|---:|---:|---:|
| Windows x64 | 是 | 是 | 是 |
| Linux x64 | 否 | 否 | 是 |

Demo 严格作为 Bridge 3 / ABI5 Consumer：不跟踪 `OcctNative`、`OcctNet*` 实现源码，也不直接调用 `occt_*` Native ABI。

Demo 本身继续以 **.NET 10** 为目标，用于验证最新支持的 Consumer Runtime。构建工具链以稳定版 .NET 10 SDK `10.0.100` 为基线，并使用 `latestFeature` 滚动，因此后续稳定版 10.0.x SDK 可以直接使用。Bridge Binary SDK 当前以 .NET 8 为最低运行时基线，但 Demo 是否兼容由 Contract 中 `supportedConsumerFrameworks` / `supportedDesktopConsumerFrameworks` 决定，不再假设 Bridge 最低 TFM 必须是某一个固定值。

Demo 自身运行目录也与 Bridge 最低 TFM 解耦：`run.ps1` 直接从各 Demo `.csproj` 读取 `TargetFramework`。因此当前 WPF/WinForms 正确运行于 `net10.0-windows` 输出目录，即使消费的 Bridge DLL 目标仍是 `net8.0-windows`。

## 当前 Viewport 契约

三个 UI Host 现在统一消费同一套 Bridge Viewport 模型，不再各自维护框架特有的生命周期逻辑：

- `OcctViewportInteractionFeatures` 控制 Hover、点选/框选、旋转、平移和缩放；
- `PreviewPointerInput / PointerInput`、`PreviewKeyInput / KeyInput` 提供平台无关输入；
- `HostState`、`EngineGeneration`、`EngineRecreated`、`EngineDisposing`、`Faulted` 定义 Native Host 生命周期；
- `InitialOptions`、`RenderReady`、`FirstFrameRendered` 定义首帧配置和真正可显示状态；
- `NativeHandleChanged` 只用于高级宿主集成/诊断中的 HWND/XID 变化通知；
- `HoverHitChanged` 直接报告 Owner/Subshape 身份变化，应用无需反复调用 `DetectAt`；
- 批量场景/视图配置统一使用已有的 `BeginDisplayBatch()`；
- Samples 菜单增加 transient **Viewer 投影测试**，实际调用 `ProjectPointToEdge`、`ProjectPointToFace` 并验证参数回代。

共享快捷键映射直接消费 `OcctKeyInputEventArgs`，因此 Viewport 获得焦点后，Ctrl+Z/Y/N/O/S、Delete、F、0/1/2/3、Escape 不再依赖 WinForms/WPF/Avalonia 各自的 Key 枚举。窗口级快捷键只保留为焦点不在 Viewport 时的 fallback。

## Binary SDK 流程

`dist/` 只作为本地构建状态存在并被 Git 忽略。Windows 同步脚本使用仓库内固定的 `.cache/main-sdk-source/` 作为可复用源码克隆：首次同步只克隆一次，后续仅 fetch/checkout 到目标 `main` 或 `main-dev` commit，并复用被忽略的构建缓存，不再在仓库旁边创建 `.OcctCSharpBridge-main-sdk-<guid>` 临时 worktree。`.cache/` 整体由 Git 忽略。

Windows 从源码同步时，如果所选 Bridge Source Revision 已提供 **`build.ps1 sdk Release`**，就直接执行该完整 Release Gate；对于尚未包含 `sdk` Target 的旧 Source Revision，`sync.ps1` 会执行等价的 **`all Release` → `dist Release`** 完整验证序列。因此迁移期间默认 `SourceBranch=main` 仍然可用，但不会退回成“只编译就打包”的未验证流程。新的 `sdk` Gate 会编译 .NET 8/9/10 Consumer Matrix、运行 ManagedTests、Core Native Smoke 与三套 Windows Viewport Host Smoke，全部通过后才用已经验证过的 Bridge 输出生成 `dist/win-x64`。

Windows Binary SDK Payload 是严格、扁平的 7 个文件；Demo 只接受以下内容，`-SdkRoot` 同样不允许额外文件或目录：

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

这样可以防止旧 DLL、未 Hash 文件混入一个表面上 Manifest 合法的 SDK。复制前会校验 contract schema 3、manifest schema 2、ABI5-only、Consumer TFM 支持列表、C# 14、`sourceCommit` 和全部 SHA-256；当本地 `manifest.sourceCommit` 与目标 SDK Source Commit 一致时直接复用，不重复构建。

正式 `demo` 在 Windows 上消费 `main`：

```powershell
.\sync.ps1
.\build.ps1 all Release
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

`publish.ps1 all` 默认输出一个**framework-dependent** 统一目录：

```text
artifacts/publish/CAD-Demo-win-x64/
├─ CAD-Winform.exe
├─ CAD-WPF.exe
├─ CAD-Avalonia.exe
├─ run-winform.cmd
├─ run-wpf.cmd
├─ run-avalonia.cmd
├─ Bridge / OCCT / 应用公共依赖（单份）
├─ occt/resources/...
└─ package-manifest.json
```

统一包**不携带 .NET Runtime**，目标机器需要安装 **.NET 10 Desktop Runtime x64**。三个应用共享一份应用公共依赖、Bridge DLL、OCCT DLL 和 OCCT 资源，不再生成三个包含大量重复框架/Bridge/OCCT DLL 的目录。

发布前脚本只运行一次 Demo Build Gate：统一包执行 `all`，单目标包只执行对应目标；各 staging publish 不再额外重复调用 `build.ps1`。生成的 `package-manifest.json` 会记录 Bridge Source Commit、发布模式/所需 Runtime，以及包内每个文件的 SHA-256 和大小。

只发布单个前端时仍生成独立包。单目标默认 self-contained，也可以显式改为 framework-dependent：

```powershell
.\publish.ps1 wpf Release -SelfContained -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 avalonia Release -FrameworkDependent -OcctRoot "D:\tools\occt-vc144-64"
```

统一 `all` 不允许 self-contained，因为 WinForms/WPF/Avalonia 的 Windows Desktop Runtime Closure 中存在同名但内容不同的 Framework DLL；包装脚本会在真正 publish 前直接拒绝该组合。

开发阶段验证 `demo-dev` 对尚未合并的 `main-dev` SDK 时，必须显式指定来源：

```powershell
.\sync.ps1 -SourceBranch main-dev -ForceRebuild
.\build.ps1 validate Release
.\build.ps1 all Release
```

不要修改 `sync.ps1` 的默认 `SourceBranch=main`；正式 `demo` 必须始终消费正式 `main`。

Linux：

```bash
./sync.sh
./build.sh all Release
./run.sh Release
./publish.sh Release
```

Linux 只构建 `OcctDemo.Common` 和 `OcctDemo.Avalonia`，绝不构建 WinForms/WPF。当前 Avalonia Viewer 交互运行需要 X11/XWayland。

平台细节见 [LINUX.md](LINUX.md) 和 [docs/platform-matrix.md](docs/platform-matrix.md)。

## Demo 预览

- WinForms / Windows：`assets/previews/winform-demo-zh.png`
- WPF / Windows：`assets/previews/wpf-demo-zh.png`
- Avalonia / Windows：`assets/previews/avalonia-win-demo-zh.png`
- Avalonia / Linux：`assets/previews/avalonia-linux-demo-zh.png`

## 分支职责

- `main` / `main-dev`：Bridge SDK 源码与开发。
- `demo` / `demo-dev`：统一的 Windows/Linux Demo Consumer。
- `website`：双语项目官网。
- 历史备份分支如存在，不参与日常开发并保持不变。

当前不存在独立 Avalonia 源码分支。Avalonia 已属于 `main` 和统一 `demo`。

许可证为 GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0。
