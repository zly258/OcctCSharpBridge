# 第三方项目消费 SDK

本文面向**不参与 OcctCSharpBridge 源码开发**的第三方 CAD/BIM/工程应用，说明如何可靠地引用、验证、部署和升级正式 `main` 生成的 SDK。

核心原则：

> 第三方项目消费的是一个经过版本、平台、ABI、Source Commit 和 Hash 约束的 SDK 制品，而不是从 Bridge 仓库复制若干 DLL/SO 后自行拼装。

## 1. 应该拿哪一种 SDK

OcctCSharpBridge 有两类相关产物。

### 1.1 最小 Binary SDK

用于：

- 编译期 `<Reference>`；
- CI 中做 Contract / Manifest / Hash 校验；
- Demo 或内部 Consumer 快速同步；
- 从明确 Bridge Source Revision 构建应用。

Windows x64：

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

Linux x64：

```text
libOcctNative.so
OcctNet.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

最小 Binary SDK **不是完整运行时包**。它没有 OCCT Native Dependency Closure 和完整 OCCT Resources。

### 1.2 Portable SDK

用于最终应用部署和对外分发。它在 Managed DLL 之外包含：

```text
runtime/
  OcctNative / libOcctNative
  OCCT Native Libraries
  打包器选中的第三方 Native Dependencies
occt/resources/
package-manifest.json
License / Notice
```

第三方正式应用应优先获取从正式 `main` 通过 `publish.ps1` / `publish.sh` 生成的 Portable SDK 或对应 Release Asset。

**推荐模型：Binary SDK 负责编译；Portable SDK 负责部署。** 两者必须来自同一个 Bridge Build / Source Commit。

## 2. 当前兼容契约

第三方项目接入前至少确认：

| 项目 | 当前要求 |
| --- | --- |
| Bridge | `3.0.0-preview.1` |
| Native ABI | 仅 ABI 5 |
| OCCT | 7.9.0 |
| 架构 | x64 |
| Core/Avalonia Binary TFM | `net8.0` |
| WinForms/WPF Binary TFM | `net8.0-windows` |
| 支持 Consumer | .NET 8 / 9 / 10 |
| Windows UI | WinForms / WPF / Avalonia |
| Linux UI | Avalonia |

第三方项目自身不需要使用与 Bridge 源码相同的 TFM。比如 Bridge Managed DLL 为 `net8.0`，第三方应用可以是 `net10.0`；Windows Desktop 可以是 `net8.0-windows`、`net9.0-windows` 或 `net10.0-windows`。

构建 Bridge 源码所需的稳定版 .NET 10 SDK 与第三方应用的运行时基线是两个概念。第三方只消费正式二进制时，不需要为了 Bridge 安装精确的 `10.0.303` SDK。

## 3. 推荐第三方仓库结构

不建议把 Bridge 文件散落在业务项目目录中。建议明确一个 External/SDK Root：

```text
MyCadApp/
  src/
    MyCadApp/
      MyCadApp.csproj
  external/
    OcctCSharpBridge/
      win-x64/
        OcctNet.dll
        OcctNet.Wpf.dll
        ...
        bridge-contract.json
        bridge-manifest.json
      linux-x64/
        OcctNet.dll
        OcctNet.Avalonia.dll
        ...
  build/
  artifacts/
```

也可以把 SDK 放在企业制品缓存目录，而不是提交到业务 Git 仓库。关键是让 SDK Root 可配置，并由 Manifest 决定其身份。

推荐在 `Directory.Build.props` 或项目文件中统一定义：

```xml
<Project>
  <PropertyGroup>
    <OcctBridgeRid Condition="$([MSBuild]::IsOSPlatform('Windows'))">win-x64</OcctBridgeRid>
    <OcctBridgeRid Condition="$([MSBuild]::IsOSPlatform('Linux'))">linux-x64</OcctBridgeRid>
    <OcctBridgeSdkRoot>$(MSBuildThisFileDirectory)external/OcctCSharpBridge/$(OcctBridgeRid)</OcctBridgeSdkRoot>
  </PropertyGroup>
</Project>
```

如果 SDK 由 CI 下载，也可以从命令行覆盖：

```bash
dotnet build -p:OcctBridgeSdkRoot=/opt/company-sdk/OcctCSharpBridge/linux-x64
```

## 4. Headless/Core 项目

只做几何、建模、拓扑、网格或数据交换，不嵌入 Bridge UI Host 时，只引用 `OcctNet.dll`：

```xml
<ItemGroup>
  <Reference Include="OcctNet">
    <HintPath>$(OcctBridgeSdkRoot)/OcctNet.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>
```

最小代码：

```csharp
using OcctNet;

OcctRuntime.Configure();

using var model = new OcctModelingSession();
var box = model.MakeBox(100, 80, 10);
model.ExportStep(box, "box.step");
```

## 5. Avalonia 项目

Avalonia 需要 Core + Avalonia Adapter：

```xml
<ItemGroup>
  <Reference Include="OcctNet">
    <HintPath>$(OcctBridgeSdkRoot)/OcctNet.dll</HintPath>
    <Private>true</Private>
  </Reference>
  <Reference Include="OcctNet.Avalonia">
    <HintPath>$(OcctBridgeSdkRoot)/OcctNet.Avalonia.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>
```

Avalonia Framework 包版本由第三方应用自己管理。不要因为使用 `OcctNet.Avalonia` 就把整个 Demo 项目复制进业务仓库。

Linux 只提供 Core + Avalonia Managed Surface；WinForms/WPF 不属于 Linux 契约。

## 6. WinForms 项目

Windows WinForms：

```xml
<ItemGroup>
  <Reference Include="OcctNet">
    <HintPath>$(OcctBridgeSdkRoot)/OcctNet.dll</HintPath>
    <Private>true</Private>
  </Reference>
  <Reference Include="OcctNet.WinForms">
    <HintPath>$(OcctBridgeSdkRoot)/OcctNet.WinForms.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>
```

Target Framework 应使用受支持的 Windows Desktop TFM，例如：

```xml
<TargetFramework>net10.0-windows</TargetFramework>
<UseWindowsForms>true</UseWindowsForms>
```

## 7. WPF 项目

Windows WPF：

```xml
<ItemGroup>
  <Reference Include="OcctNet">
    <HintPath>$(OcctBridgeSdkRoot)/OcctNet.dll</HintPath>
    <Private>true</Private>
  </Reference>
  <Reference Include="OcctNet.Wpf">
    <HintPath>$(OcctBridgeSdkRoot)/OcctNet.Wpf.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>
```

例如：

```xml
<TargetFramework>net10.0-windows</TargetFramework>
<UseWPF>true</UseWPF>
```

不要为了方便同时引用 WinForms、WPF、Avalonia 三个 Adapter。一个应用只引用自己使用的 UI Surface，可以减少依赖和发布歧义。

## 8. 编译引用与最终部署必须分开理解

MSBuild `<Reference Private="true">` 会把 Managed DLL 复制到输出目录，也可能把最小 SDK 中的 Native Bridge 带到普通构建输出。但**最终发布包不能只依赖最小 Binary SDK 的 flat Native 文件**。

正式部署应基于 Portable SDK：

```text
MyCadApp/
  MyCadApp.exe                   # 或 Linux apphost
  MyCadApp.dll
  OcctNet.dll
  OcctNet.Wpf.dll                # 示例
  runtime/
    OcctNative.dll               # Windows
    TKernel.dll
    TK*.dll
    ...
  occt/
    resources/
      ...
```

Linux：

```text
MyCadApp/
  MyCadApp
  MyCadApp.dll
  OcctNet.dll
  OcctNet.Avalonia.dll
  runtime/
    libOcctNative.so
    libTKernel.so*
    libTK*.so*
    ...
  occt/resources/
```

如果普通 `dotnet publish` 把最小 SDK 中的 flat `OcctNative.dll` / `libOcctNative.so` 放到了应用根目录，而正式部署又合并了 Portable `runtime/`，建议从最终发布包删除根目录的 flat Native Bridge，避免优先加载一个没有相邻 OCCT Closure 的 Native 文件。

## 9. Runtime 初始化

推荐在应用启动的最早阶段、创建任何 Bridge Native-backed 对象之前调用：

```csharp
OcctRuntime.Configure();
```

包括：

- 第一个 `OcctModelingSession`；
- 第一个 `OcctEngine`；
- 第一个 WinForms/WPF/Avalonia Viewport Host。

默认 Portable 布局下不需要目标机配置开发机的 `OCCT_ROOT` / `CASROOT`。

如果企业应用有自己的安装目录，可以显式指定 OCCT Root 和 Native Bridge Directory，但这些目录仍必须属于同一套 SDK Payload。

## 10. 第三方应用自己的 .NET 发布方式

Portable SDK 不捆绑第三方应用的 .NET Runtime。应用可以自行选择：

### Framework-dependent

```bash
dotnet publish -c Release --self-contained false
```

目标机需要匹配的 .NET Runtime。

### Self-contained

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

或：

```bash
dotnet publish -c Release -r linux-x64 --self-contained true
```

然后再把匹配平台的 Bridge Portable SDK Runtime/Resources 合并到应用发布目录。

Bridge Portable SDK 与 .NET Self-contained 是两个正交问题：前者解决 OCCT/Native Closure，后者解决 .NET Runtime。

## 11. CI 接受 SDK 前必须校验什么

第三方 CI 至少应检查：

1. `bridge-contract.json` 存在；
2. `bridge-manifest.json` 存在；
3. `platform` 与目标 RID 一致；
4. ABI `current = 5` 且 `minimumSupported = 5`；
5. Bridge Version 符合项目锁定策略；
6. `sourceCommit` 非空并符合允许的正式版本；
7. Manifest 中所有 SHA-256 与本地文件一致；
8. Portable SDK 的 `bridgeSourceCommit` / `bridgeVersion` 与 Binary SDK 完全一致；
9. Portable Package Manifest 中全部文件 Hash 正确。

不要只判断文件名和 DLL Version，也不要用“目录最近修改时间”判断 SDK 是否最新。

## 12. 推荐的版本锁定策略

企业项目可以在自己的构建配置中锁定：

```text
BridgeVersion = 3.0.0-preview.1
SourceCommit   = <reviewed main commit>
NativeAbi      = 5
Platform       = win-x64 / linux-x64
```

升级时执行：

```text
下载新 Binary SDK + Portable SDK
        ↓
校验 Contract / Manifest / Hash
        ↓
替换全部 OcctNet*.dll
        ↓
替换 runtime/
        ↓
替换 occt/
        ↓
重新编译应用
        ↓
应用级回归测试
```

禁止：

```text
只换 OcctNet.dll
保留旧 OcctNative
保留旧 runtime/TK*.dll
把 win-x64 与 linux-x64 文件混放
从两个 sourceCommit 拼一个包
```

## 13. 从 Bridge 源码生成 SDK 的场景

普通第三方团队优先使用正式 Release Asset。如果企业内部确实需要从已审核的 Bridge Source Commit 构建，可使用快速 Consumer Artifact Path。

Windows：

```powershell
git switch main
git reset --hard <approved-main-commit>
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux：

```bash
git switch main
git reset --hard <approved-main-commit>
./build.sh dist Release
```

`dist` 只生成 Consumer Binary SDK，不代表完整 Bridge Release Gate 已通过。正式对外制品仍应来自 Bridge `publish` 流程或企业自己的等价受控 Gate。

如果已经拥有预先验证的 SDK，第三方项目本身没有理由重新编译 Bridge。

## 14. Linux 兼容性要求

Linux 的 Portable Runtime 会携带 OCCT 等非系统 Native Libraries，但不会把 glibc/libstdc++ ABI 问题自动封装掉。

如果 `ldd` 报：

```text
GLIBC_2.xx not found
GLIBCXX_3.4.xx not found
CXXABI_1.x.x not found
```

说明 Native Bridge/OCCT 的构建基线比目标 Linux 新。正确解决办法是用更老的目标 ABI 基线重新编译 OCCT 和 `libOcctNative.so`，而不是简单复制另一个 `libc.so.6`。

AppImage 只能改善单文件分发体验，不能把一个已经要求新 glibc 的 ELF 自动变成兼容旧 glibc 的 ELF。

企业若要声明“支持某组 Debian/Ubuntu/Kylin 版本”，应明确最老构建基线，并对该矩阵做真实运行验证。

## 15. 常见错误

### `DllNotFoundException` / `Unable to load OcctNative`

检查：

- `OcctRuntime.Configure()` 是否足够早；
- `runtime/` 是否存在；
- Native Closure 是否完整；
- x64 是否一致；
- Windows `PATH` / Loader 依赖是否可解析；
- Linux `ldd runtime/libOcctNative.so` 是否出现 `not found` 或 ABI Version Error。

### 能加载 Native，但 STEP/IGES/Shader 相关功能失败

检查 `occt/resources/` 是否完整，以及相应 `CSF_*` 是否被 Runtime 配置。

### 编译报找不到 `OcctNet.*`

检查 `OcctBridgeSdkRoot` 和 `<HintPath>`，不要把 Portable `runtime/` 当作 Managed Reference Root。

### ABI/版本不匹配

不要覆盖异常或修改 Manifest。重新部署来自同一 Build 的 Managed + Native + Runtime + Resources。

## 16. License 与第三方 Notice

如果第三方应用重新分发 Portable SDK 中的 Bridge/OCCT/第三方 Native 文件，应同时保留对应的 License 和 Third-party Notice 文件，并按照项目 License/Exception 及第三方组件许可证履行分发义务。

正式条款以仓库中的 `LICENSE`、LGPL 文本、OcctCSharpBridge Exception、`COMMERCIAL.md`、`THIRD_PARTY_NOTICES.md` 以及具体第三方组件许可证为准。

## 17. Demo 的定位

`demo` / `demo-dev` 是官方参考 Consumer，可以用于查看：

- Binary SDK 的 `<Reference HintPath>` 方式；
- WinForms/WPF/Avalonia Host 的实际使用；
- `OcctRuntime` 运行时配置；
- Portable Runtime 合并；
- Manifest/Source Commit/Hash 校验。

但第三方项目不应复制 Demo 的业务 UI、菜单、命令系统来替代自己的应用架构。Demo 是消费示例，不是应用框架。
