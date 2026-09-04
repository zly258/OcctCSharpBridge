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

## 共享 Bridge SDK

Demo 在 Windows 和 Linux 下都直接消费机器级 Bridge Binary SDK。

Windows 默认路径：

```text
C:\Program Files\OcctCSharpBridge\SDK\3.0\win-x64
```

Linux 默认路径：

```text
/usr/local/lib/OcctCSharpBridge/SDK/3.0/linux-x64
```

在 Bridge `main` 中安装/更新：

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

```bash
./publish.sh
```

可通过 `OCCTCSHARPBRIDGE_SDK` 覆盖 SDK Root。Demo 不再 clone Bridge，也不再在 `external/` 下维护 Binary SDK 同步副本。系统 SDK 缺失或不完整时，构建会直接给出安装路径并失败。Demo 发布阶段仍可使用 `external/OcctCSharpBridge/portable/...` 保存与系统 Binary SDK 匹配的 Portable Runtime Closure。

## Demo 构建

Windows：

```powershell
.\build.ps1 all Release

.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

Linux：

```bash
./build.sh all Release
./run.sh Release
```

## 综合能力案例

Samples 菜单只保留正常 CAD 操作无法直接替代的综合案例，WinForms、WPF、Avalonia 共用 `OcctDemo.Common` 中同一套实现：

1. **Section Analysis**：按平面拆分并同时显示正侧、负侧与截交结果。
2. **Drawing Projection**：生成 Front、Top、Right、Isometric 四个 HLR 工程投影。
3. **Distance & Extrema**：计算 Curve/Curve 极值并显示最近点连接线。
4. **Model Repair**：执行 FixShape，并对比修复前后的结构化检查结果。

STEP/IGES 等文件继续通过 File → Open/Import 直接打开；BRep 查询继续使用现有包围盒、几何属性、拓扑统计和形体检查；自由几何读取与 Intersection/Extrema 等能力通过正常命令和 Samples 展示，不再维护 Validation 测试菜单。

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

Linux 使用已安装的 Binary SDK 发布 Avalonia self-contained 应用，并合并匹配的 Bridge Portable Runtime 与 OCCT Resources：

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

Demo 是 SDK 消费示例，不是第三方应用框架。第三方项目应按 Bridge 的 SDK Consumer 文档组织自己的工程、Runtime 和版本锁定策略。
