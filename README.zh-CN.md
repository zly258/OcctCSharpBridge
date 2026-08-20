# OcctCSharpBridge Demo 开发分支

[English](README.md) · [Bridge Development SDK](https://github.com/zly258/OcctCSharpBridge/tree/main) · [Bridge 第三方 SDK 接入说明](https://github.com/zly258/OcctCSharpBridge/blob/main/docs/zh-CN/09_%E7%AC%AC%E4%B8%89%E6%96%B9%E9%A1%B9%E7%9B%AE%E6%B6%88%E8%B4%B9SDK.md)

`demo` 是 Bridge Binary/Portable SDK 的开发 Consumer，默认跟随 `main`。它不维护 `OcctNative` / `OcctNet*` 实现源码，也不直接调用 `occt_*` Native ABI。

```text
OcctDemo.Common
├─ OcctDemo.WinForms  → Windows x64
├─ OcctDemo.Wpf       → Windows x64
└─ OcctDemo.Avalonia  → Windows x64 / Linux x64
```

Demo 自身使用 .NET 10，用来覆盖 Bridge 支持的最新 Consumer Runtime；Bridge Managed Binary SDK 仍以 .NET 8 为最低 TFM，并面向 .NET 8/9/10 Consumer。

## SDK 同步：现在是 Consumer 快路径

`dist/` 是本地可删除缓存并由 Git 忽略。同步得到两组来自**同一个 Bridge sourceCommit** 的产物：

```text
dist/win-x64/                   # 最小 Binary SDK，供编译引用
dist/portable/win-x64/          # Portable Runtime，供发布

# Linux
dist/linux-x64/
dist/portable/linux-x64/
```

### 缓存命中

如果本地 Binary SDK 的 `manifest.sourceCommit` 与远端目标分支一致，并且 Binary/Portable Manifest 的所有 Hash 均通过，`sync` 直接返回：

```text
0 次 Bridge 编译
0 次 Bridge 测试
0 个窗口 Smoke
```

### 缓存失效

以前 Windows `sync.ps1` 会执行 Bridge `sdk` 完整 Gate，Linux `sync.sh` 会执行 `all -> dist`。这会重复触发 ManagedTests、Core Smoke，以及 Windows 的 WinForms/WPF/Avalonia 窗口 Smoke。

现在统一改为：

```text
Bridge build dist Release
        ↓
Native + Managed + Binary SDK
        ↓
Bridge Portable Packager
        ↓
Contract / sourceCommit / SHA-256 校验
        ↓
Demo 本地 dist Cache
```

同步阶段明确**不运行**：

- Bridge Consumer Matrix；
- Bridge ManagedTests；
- Bridge Core Native Smoke；
- WinForms/WPF/Avalonia Viewport Smoke；
- Linux Avalonia 图形 Smoke。

完整 QA 属于 Bridge `main/main` 自己的 `sdk` / `publish` 流程，不属于 Consumer 刷新 SDK 的职责。

Windows：

```powershell
.\sync.ps1
```

强制重新生成 Consumer SDK：

```powershell
.\sync.ps1 -ForceRebuild
```

显式来源：

```powershell
.\sync.ps1 -SourceBranch main -ForceRebuild
```

Linux：

```bash
./sync.sh
./sync.sh --force-rebuild
```

## 已有 Bridge 制品时不要重新编译

如果 Bridge 已经发布/生成匹配的 Binary SDK 与 Portable SDK，Demo 可以直接校验并复制，Bridge 编译次数为 0。

Windows：

```powershell
.\sync.ps1 `
  -SdkRoot <binary-sdk> `
  -PortableRoot <portable-sdk>
```

Linux：

```bash
./sync.sh \
  --sdk-root <binary-sdk> \
  --portable-root <portable-sdk>
```

两者必须属于同一个 Bridge Build；`sourceCommit`、Bridge Version 和 Package Hash 必须完全匹配。

## Demo 构建

Windows：

```powershell
.\build.ps1 validate Release
.\build.ps1 all Release

.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

Linux：

```bash
./build.sh validate Release
./build.sh all Release
./run.sh Release
```

这里验证的是 **Demo Consumer 自己**，不会重新执行 Bridge 的完整 QA Gate。

## Windows 发布

默认统一发布：

```powershell
.\publish.ps1 all Release -Zip
```

默认布局使用**一份共享私有 .NET 10 Desktop Runtime**，避免 WinForms/WPF/Avalonia 各带一套完整 Runtime：

```text
artifacts/publish/CAD-Demo-win-x64/
├─ apps/
│  ├─ winform/
│  │  └─ CAD-Winform.exe
│  ├─ wpf/
│  │  └─ CAD-WPF.exe
│  └─ avalonia/
│     └─ CAD-Avalonia.exe
├─ dotnet/                      # 三个 App 共用一套私有 .NET 10 Desktop Runtime
├─ runtime/                     # OcctNative + OCCT/第三方 Runtime，仅一份
├─ occt/resources/
├─ bridge-contract.json
├─ bridge-manifest.json
├─ bridge-portable-manifest.json
├─ run-winform.cmd
├─ run-wpf.cmd
├─ run-avalonia.cmd
└─ package-manifest.json
```

默认 `all` 不要求目标机预装 .NET 10，因为包内带一份共享私有 Runtime。

如果显式使用：

```powershell
.\publish.ps1 all Release -SelfContained -Zip
```

则保留三个应用各自的 Self-contained Runtime Closure，体积会明显增大。

显式：

```powershell
.\publish.ps1 all Release -FrameworkDependent -Zip
```

则不带私有 .NET Runtime，目标机需要安装匹配 Runtime。

发布阶段会移除应用目录中可能由最小 Binary SDK 带出的 flat `OcctNative.dll`，最终统一使用 `runtime/OcctNative.dll` 及其完整 OCCT Closure。

## Linux 发布

```bash
./publish.sh Release
```

Linux 当前发布 Avalonia self-contained 应用，并合并匹配的 Bridge Portable Runtime 与 OCCT Resources：

```text
CAD-Avalonia-linux-x64/
├─ CAD-Avalonia
├─ Managed/.NET publish files
├─ runtime/
│  ├─ libOcctNative.so
│  ├─ libTKernel.so*
│  └─ libTK*.so* / packaged dependencies
├─ occt/resources/
├─ bridge-portable-manifest.json
├─ package-manifest.json
└─ run.sh
```

Linux Portable/AppImage 的发行版兼容性取决于 Native 构建的 glibc/libstdc++ ABI 基线。较新的 Linux 上编译出的 OCCT 即使被装入 Portable 包，也可能无法在更旧的 Kylin/Debian/Ubuntu 上运行；这不是 Demo sync 能通过复制更多文件解决的问题。

## Consumer 边界

Demo 只允许通过 Managed SDK 使用 Bridge：

- 不跟踪 `src/OcctNative` 或 `src/OcctNet*`；
- 不声明 `LibraryImport/DllImport("OcctNative")`；
- 不使用 pre-ABI5 Handle/Metadata；
- 不重新实现 Bridge Runtime Closure 收集器；
- 不把 SDK 同步变成第二套 Bridge Release Gate。

`tests/check-sdk-consumer.ps1` / `.sh` 会静态守住这些边界，并检查同步脚本只能使用 `dist` Consumer 快路径。

Demo 是 SDK 消费示例，不是第三方应用框架。第三方项目应按 Bridge 的 SDK Consumer 文档组织自己的工程、Runtime 和版本锁定策略。
