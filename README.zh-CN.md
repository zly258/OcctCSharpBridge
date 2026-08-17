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

`dist/` 只作为本地构建状态存在并被 Git 忽略。Windows/Linux 两套同步脚本都会校验 contract schema 3、manifest schema 2、ABI5-only、稳定版 .NET 10 SDK 基线/`latestFeature` 策略、C# 14 和 SDK 文件哈希；当 `manifest.sourceCommit` 与目标 SDK Source Commit 一致时直接复用，不重复构建。

正式 `demo` 在 Windows 上消费 `main`：

```powershell
.\sync.ps1
.\build.ps1 all Release
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

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
