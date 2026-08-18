# OcctCSharpBridge Demo 开发分支

[English](README.md) · [Development SDK](https://github.com/zly258/OcctCSharpBridge/tree/main-dev)

`demo-dev` 是开发阶段的 Demo Consumer 分支，默认消费 `main-dev`。正式 `demo` 仍然消费正式 `main`。

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

Demo 本身以 **.NET 10** 为目标，用来覆盖 Bridge 支持的最新 Consumer Runtime。构建工具链使用稳定版 .NET 10 SDK，基线为 `10.0.100`，`rollForward=latestFeature`。Bridge Binary SDK 以 .NET 8 为最低基线，实际兼容范围由 Contract 中的 `supportedConsumerFrameworks` / `supportedDesktopConsumerFrameworks` 决定。

## 当前 Viewport 契约

三个 UI Host 统一消费同一套 Bridge Viewport 模型：

- `OcctViewportInteractionFeatures` 控制 Hover、点选/框选、旋转、平移和缩放；
- `PreviewPointerInput / PointerInput`、`PreviewKeyInput / KeyInput` 提供平台无关输入；
- `HostState`、`EngineGeneration`、`EngineRecreated`、`EngineDisposing`、`Faulted` 定义 Native Host 生命周期；
- `InitialOptions`、`RenderReady`、`FirstFrameRendered` 定义首帧就绪状态；
- `NativeHandleChanged` 用于高级宿主集成与诊断；
- `HoverHitChanged` 直接报告 Owner/Subshape 身份变化；
- 批量场景/视图更新统一使用 `BeginDisplayBatch()`；
- Samples 菜单包含使用 `ProjectPointToEdge` / `ProjectPointToFace` 的 Viewer 投影测试。

## SDK 同步模型

`dist/` 是本地生成状态，全部由 Git 忽略。`demo-dev` 现在会从**同一个 `main-dev` sourceCommit** 同步两类 Bridge 产物：

```text
dist/win-x64/                  # 严格最小 Binary SDK，用于编译引用
└─ ABI5 7 文件 Payload

dist/portable/win-x64/         # 已验证 Portable Runtime，仅用于发布
├─ runtime/                     # OcctNative + OCCT/第三方/VC Runtime Closure
├─ occt/resources/              # OCCT 资源
├─ package-manifest.json
└─ Bridge License/Notice/元数据
```

Linux 对应：

```text
dist/linux-x64/
dist/portable/linux-x64/
```

最小 Windows Binary SDK 契约保持不变，仍严格只允许以下 7 个文件：

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

这样 Demo 编译引用和 Consumer 校验不会因为 Portable Runtime 中的大量 OCCT DLL 而被污染。

Windows `sync.ps1` 继续复用 `.cache/main-sdk-source/` Bridge 源码克隆。当前 `main-dev` 会先执行完整 `build.ps1 sdk Release` Gate，再直接调用 Bridge 自己的 `tools/package-portable-sdk.ps1`，由同一个已验证 Binary SDK 生成 Portable Runtime。同步完成后再次校验 Binary SDK 与 Portable SDK 的 `sourceCommit`、Bridge 版本以及 Manifest SHA-256。

Linux `sync.sh` 使用同样的思路：Binary SDK 构建完成后调用 Bridge 的 `tools/package-portable-sdk.sh`。因此 `ldd`、OCCT/TBB 等依赖筛选、`$ORIGIN` RPATH 修正、OCCT Resource 收集全部由 Bridge 负责，Demo 不再维护第二套逻辑。

开发分支默认来源已经是 `main-dev`：

```powershell
.\sync.ps1 -ForceRebuild
```

也可以显式指定：

```powershell
.\sync.ps1 -SourceBranch main-dev -ForceRebuild
```

如果使用外部预构建产物，必须同时提供完全匹配的 Binary SDK 与 Portable SDK：

```powershell
.\sync.ps1 -SdkRoot <binary-sdk> -PortableRoot <portable-sdk>
```

Linux：

```bash
./sync.sh --force-rebuild
./sync.sh --sdk-root <binary-sdk> --portable-root <portable-sdk>
```

## 构建与开发运行

Windows：

```powershell
.\build.ps1 all Release
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

`run.ps1` 是开发运行入口，仍可以依赖本机 OCCT 安装。Portable OCCT Runtime 只属于正式发布阶段。

Linux：

```bash
./build.sh all Release
./run.sh Release
```

Linux 只构建 `OcctDemo.Common` 与 `OcctDemo.Avalonia`；交互 Viewer 需要 X11/XWayland。

## 发布逻辑

`demo-dev` 发布脚本已经不再自行收集 OCCT 依赖。

旧流程：

```text
Demo publish
→ 自己 dumpbin / ldd
→ 自己分析 OCCT/TBB 等依赖
→ 自己复制 OCCT resources
```

当前流程：

```text
main-dev Bridge sync
→ 已验证最小 Binary SDK
→ Bridge 自己生成 Portable SDK
→ Demo .NET publish
→ 复用完全匹配的 Portable Runtime / resources
→ Demo 最终包
```

`publish.ps1` / `publish.sh` 会在打包前确认 Portable SDK 的 `bridgeSourceCommit`、`bridgeVersion` 与当前同步的最小 Binary SDK 完全一致，并再次校验 Portable Manifest 中的所有文件哈希。

### Windows 统一包

```powershell
.\publish.ps1 all Release -Zip
```

输出结构：

```text
artifacts/publish/CAD-Demo-win-x64/
├─ CAD-Winform.exe
├─ CAD-WPF.exe
├─ CAD-Avalonia.exe
├─ OcctNet*.dll
├─ runtime/
│  ├─ OcctNative.dll
│  ├─ TKernel.dll
│  ├─ TK*.dll
│  └─ 必需第三方 / VC Runtime DLL
├─ occt/resources/...
├─ bridge-contract.json
├─ bridge-manifest.json
├─ bridge-portable-manifest.json
├─ run-winform.cmd
├─ run-wpf.cmd
├─ run-avalonia.cmd
└─ package-manifest.json
```

Demo Project Publish 过程中如果在根目录复制了最小 `OcctNative.dll`，发布脚本会主动删除它，避免 Runtime Resolver 优先加载一个没有相邻依赖 Closure 的 Native Bridge。最终只使用 `runtime/OcctNative.dll`。

`run-*.cmd` 会设置：

```text
OCCT_BRIDGE_NATIVE_DIR=<app>/runtime
OCCT_ROOT=<app>/occt
CASROOT=<app>/occt
PATH=<app>/runtime;...
CSF_* resource variables
```

统一 `all` 仍为 framework-dependent，因为 WinForms/WPF/Avalonia 的 Windows Desktop Self-contained Closure 不适合合并为一套共享 Framework DLL。目标机器需要安装 **.NET 10 Desktop Runtime x64**。

单目标默认 self-contained：

```powershell
.\publish.ps1 wpf Release -SelfContained -Zip
.\publish.ps1 avalonia Release -FrameworkDependent -Zip
```

现在 Demo 发布命令**不再需要也不接受 `-OcctRoot`**；OCCT Runtime 已在 `sync` 阶段由 Bridge Portable Packager 生成并验证。

### Linux 发布

```bash
./publish.sh Release
```

Linux 发布会删除根目录可能存在的 `libOcctNative.so`，直接复用 `dist/portable/linux-x64/runtime` 和匹配的 OCCT Resources；Bridge Portable Runtime 中的共享库已经由 Bridge Packager 写入 `$ORIGIN` RPATH。最终 Demo 还会生成自己的 `package-manifest.json`。

## 分支职责

- `main-dev`：Bridge 开发源码和 Portable Runtime 来源；
- `demo-dev`：开发 Demo Consumer，默认消费 `main-dev`；
- `main`：正式 Bridge SDK；
- `demo`：正式 Demo Consumer，当前仍应保持默认消费 `main`，待开发方案验证后再单独升级；
- `website`：双语官网。

当前不存在独立 Avalonia 源码分支，Avalonia 属于统一 Bridge/Demo 分支体系。

许可证为 GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0；具体见仓库 License 文件。
