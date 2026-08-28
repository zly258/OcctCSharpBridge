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

## SDK 同步

Windows 下，`sync.ps1` 采用源码驱动的 Binary SDK 同步流程：

- Source Cache 不存在时 clone `OcctCSharpBridge`；
- 默认 fetch `origin/main`，也可用 `-BridgeBranch` 指定 `main-dev` 等分支；
- 将缓存锁定到解析出的 commit，并执行 clean detached checkout；
- 执行 `build.ps1 dist Release`，只编译 Native + Managed 并生成 Binary SDK；
- 校验 `bridge-contract.json`、`bridge-manifest.json`、Hash 和 `sourceCommit`；
- 将生成的 Binary SDK 安装到 `external/OcctCSharpBridge/win-x64`。

```powershell
.\sync.ps1
.\sync.ps1 -BridgeBranch main-dev
```

`sync.ps1` 不执行 Bridge 的 `sdk` / `all` 完整 QA Gate；Demo 自己的 build/run 继续负责 Consumer 验证。

Linux 下 `./sync.sh` 现在也支持 fresh clone 默认流程：默认跟随 `main`，在 `external/.cache` 维护干净的 Bridge Source Cache，只执行 `./build.sh dist Release`，再调用 Bridge 自己的 Portable SDK Packager 生成匹配 Runtime，完成校验后安装到 `external/OcctCSharpBridge`。同步过程不运行 Bridge tests/smoke。已有预构建制品仍可通过 `--sdk-root` 与 `--portable-root` 直接使用。

```bash
./sync.sh
./sync.sh --source main-dev
./sync.sh --force-rebuild
```
## Demo 构建

fresh clone 下，如果 Binary SDK 缓存不存在，`build.ps1` 会自动调用现有 `sync.ps1` 准备 SDK；如果 `external/OcctCSharpBridge/win-x64` 已完整存在，则不会再次同步或编译 Bridge。需要主动刷新或切换 Bridge 分支时，再显式执行 `sync.ps1`。

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

Validation 菜单只保留少量代表性 SDK 检查：Geometry Inspection 覆盖解析/自由几何读取及 Bezier/B-Spline Bulk 数据路径，Geometry Algorithms 覆盖 Extrema 与参数化 Intersection；现有 B-Spline Surface 与 Mesh Generation 继续分别验证控制网和 Mesh Buffer，不再为每个 API 增加单独菜单项。

## 综合能力案例

Samples 菜单只保留正常 CAD 操作无法直接替代的综合案例，WinForms、WPF、Avalonia 共用 `OcctDemo.Common` 中同一套实现：

1. **Section Analysis**：按平面拆分并同时显示正侧、负侧与截交结果。
2. **Drawing Projection**：生成 Front、Top、Right、Isometric 四个 HLR 工程投影。
3. **Distance & Extrema**：计算 Curve/Curve 极值并显示最近点连接线。
4. **Model Repair**：执行 FixShape，并对比修复前后的结构化检查结果。

STEP/IGES 等文件继续通过 File → Open/Import 直接打开；BRep 查询继续使用现有包围盒、几何属性、拓扑统计和形体检查；自由几何读取与 Intersection/Extrema API 覆盖继续放在精简后的 Validation 检查中，不再为相同能力增加重复 Samples。

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

`tests/check-sdk-consumer.ps1` 会静态守住 Windows clone/build/consume 契约与统一 `external/` 布局；Linux 检查继续维护自己的平台同步契约。

Demo 是 SDK 消费示例，不是第三方应用框架。第三方项目应按 Bridge 的 SDK Consumer 文档组织自己的工程、Runtime 和版本锁定策略。
