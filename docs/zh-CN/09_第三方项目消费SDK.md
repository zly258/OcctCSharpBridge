# 第三方项目消费 SDK

本文面向**不参与 OcctCSharpBridge 源码开发**的第三方 CAD/BIM/工程应用，说明如何引用、验证、部署和升级 3.x Stable SDK。

核心原则：

> 第三方项目消费的是一个由版本、平台、ABI、Source Commit 和 Hash 约束的 SDK 制品，而不是从 Bridge 仓库或开发机随意复制若干 DLL。

## 1. 当前支持模型

| 项目 | 3.0 Stable |
| --- | --- |
| Bridge | `3.0.0` |
| Native ABI | ABI 5 only |
| OCCT | 7.9.0 exact |
| Core/Avalonia Binary TFM | `net8.0` |
| WinForms/WPF Binary TFM | `net8.0-windows` |
| Consumer | .NET 8 / 9 / 10 |
| 官方预编译 SDK | **Windows x64** |
| Linux | 源码构建支持，Avalonia |

Windows 是官方预编译分发平台。Linux 不提供官方 3.x Binary/Portable Release Asset，使用者应在目标发行版兼容环境中自行构建。

## 2. Windows 第三方项目优先获取方式

优先级：

```text
正式 main 对应的 Windows Release Asset
        ↓ 没有
指定正式 main Source Commit
        ↓
build.ps1 dist Release
        ↓
Windows Portable SDK Packager
```

### 2.1 有正式 Release Asset

优先使用：

```text
OcctCSharpBridge-<version>-win-x64-portable.zip
```

第三方直接消费已构建的 Bridge SDK，不在 Consumer 仓库维护第二套 QA 流程。

### 2.2 没有 Release Asset

如果必须从正式 `main` 的一个明确提交生成：

```powershell
git switch main
git reset --hard <approved-main-commit>

.\build.ps1 dist Release `
  -OcctRoot "D:\tools\occt-vc144-64"

.\tools\package-portable-sdk.ps1 `
  -SdkRoot .\dist\win-x64 `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -OutputDirectory .\artifacts\consumer-sdk `
  -Zip
```

`dist` 构建 Native + Managed，并写入 Consumer 所需的 Contract/Manifest/Hash。

这种本地生成物应记录精确 `sourceCommit`。如果无法确认该 Source Commit 是否已经经过 Bridge Release QA，它只能视为本地 Consumer Build，而不是官方 Release Asset。

## 3. Binary SDK 与 Portable SDK

### Binary SDK

Windows：

```text
dist/win-x64/
  OcctNative.dll
  OcctNet.dll
  OcctNet.WinForms.dll
  OcctNet.Wpf.dll
  OcctNet.Avalonia.dll
  bridge-contract.json
  bridge-manifest.json
```

它主要用于：

- 编译期 `<Reference>`；
- CI Contract/Manifest/Hash 校验；
- Demo/内部 Consumer 同步；
- 从明确 Source Commit 构建。

根目录 `OcctNative.dll` **不代表完整部署 Runtime Closure**。

### Portable SDK

正式 Windows 部署应使用：

```text
OcctCSharpBridge-<version>-win-x64-portable/
  OcctNet.dll
  OcctNet.WinForms.dll
  OcctNet.Wpf.dll
  OcctNet.Avalonia.dll
  bridge-contract.json
  bridge-manifest.json
  package-manifest.json
  runtime/
    OcctNative.dll
    OCCT runtime closure
    required redistributable native dependencies
  occt/
    resources/
  LICENSE / NOTICE ...
```

推荐模型：

> Binary SDK 负责“编译和身份校验”；Portable SDK 负责“Windows 部署”。

二者必须来自同一个 Bridge Build / Source Commit。

## 4. 第三方仓库结构

不建议把 SDK 文件散落在业务源码目录。推荐：

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
  artifacts/
  build/
```

企业项目也可以把 SDK 放到受控制品缓存，不必提交到业务 Git 仓库。

统一定义 SDK Root：

```xml
<Project>
  <PropertyGroup>
    <OcctBridgeSdkRoot>$(MSBuildThisFileDirectory)external/OcctCSharpBridge/win-x64</OcctBridgeSdkRoot>
  </PropertyGroup>
</Project>
```

## 5. Core 引用

普通 Headless/建模项目：

```xml
<ItemGroup>
  <Reference Include="OcctNet">
    <HintPath>$(OcctBridgeSdkRoot)\OcctNet.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>
```

项目可以使用：

```xml
<TargetFramework>net8.0</TargetFramework>
```

也可以是：

```text
net9.0
net10.0
```

Bridge Binary DLL 不需要针对三个 Runtime 各复制一套。

## 6. WPF

```xml
<PropertyGroup>
  <TargetFramework>net8.0-windows</TargetFramework>
  <UseWPF>true</UseWPF>
  <PlatformTarget>x64</PlatformTarget>
</PropertyGroup>

<ItemGroup>
  <Reference Include="OcctNet">
    <HintPath>$(OcctBridgeSdkRoot)\OcctNet.dll</HintPath>
    <Private>true</Private>
  </Reference>
  <Reference Include="OcctNet.Wpf">
    <HintPath>$(OcctBridgeSdkRoot)\OcctNet.Wpf.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>
```

Windows Desktop Consumer 也可以使用 `net9.0-windows` / `net10.0-windows`。

## 7. WinForms

```xml
<PropertyGroup>
  <TargetFramework>net8.0-windows</TargetFramework>
  <UseWindowsForms>true</UseWindowsForms>
  <PlatformTarget>x64</PlatformTarget>
</PropertyGroup>

<ItemGroup>
  <Reference Include="OcctNet">
    <HintPath>$(OcctBridgeSdkRoot)\OcctNet.dll</HintPath>
    <Private>true</Private>
  </Reference>
  <Reference Include="OcctNet.WinForms">
    <HintPath>$(OcctBridgeSdkRoot)\OcctNet.WinForms.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>
```

## 8. Avalonia

Windows Avalonia：

```xml
<ItemGroup>
  <Reference Include="OcctNet">
    <HintPath>$(OcctBridgeSdkRoot)\OcctNet.dll</HintPath>
    <Private>true</Private>
  </Reference>
  <Reference Include="OcctNet.Avalonia">
    <HintPath>$(OcctBridgeSdkRoot)\OcctNet.Avalonia.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>
```

Linux Avalonia 不应直接拿 Windows SDK。应在 Linux 目标环境中从 Bridge 源码生成对应的 Linux Binary SDK，然后按相同 Source Identity 原则引用 `OcctNet.dll` / `OcctNet.Avalonia.dll`。

## 9. Runtime 初始化

Portable SDK 布局下，在第一次创建 Native-backed 对象之前：

```csharp
using OcctNet;

OcctRuntime.Configure();

using var model = new OcctModelingSession();
var box = model.MakeBox(100, 80, 20);
```

必须早于第一个：

```text
OcctEngine
OcctModelingSession
任何会触发 OcctNative 加载的调用
```

推荐应用目录：

```text
MyCadApp/
  MyCadApp.exe / .dll
  OcctNet*.dll
  runtime/
  occt/resources/
  bridge-contract.json
  bridge-manifest.json
  package-manifest.json
```

不要把另一个旧 `OcctNative.dll` 留在应用根目录与 `runtime/OcctNative.dll` 竞争加载。

## 10. 接受 SDK 前的校验

至少检查：

```text
bridgeVersion
nativeAbi.current
nativeAbi.minimumSupported
occtVersion
platform
sourceCommit
configuration
files[].sha256
```

Windows 3.0 Stable 应满足：

```text
Bridge version: 3.0.x compatible line
ABI:            5 / 5
Platform:       win-x64
OCCT:           7.9.0
Configuration:  Release
```

不要通过手工修改 Manifest 绕过 Managed/Native 版本不匹配。

## 11. 升级策略

升级 SDK 时整体替换：

```text
OcctNet*.dll
runtime/
occt/resources/
bridge-contract.json
bridge-manifest.json
package-manifest.json
```

禁止：

```text
旧 OcctNet.dll + 新 OcctNative.dll
新 Managed DLL + 旧 runtime/
不同 sourceCommit 的 runtime/ + resources/
```

建议业务项目把接受的 Bridge Version + Source Commit 记录在自己的构建日志或制品元数据中。

## 12. Threading / Lifetime

第三方必须遵守 [Stable 支持与兼容策略](10_稳定版支持与兼容策略.md)：

- 同一个 `OcctEngine` / `OcctModelingSession` 默认不是并发线程安全对象；
- UI Host 生命周期操作遵守 UI Thread；
- Engine/Session ID 不跨 Owner 混用；
- `IDisposable` Owned Resource 按契约释放；
- Host 重建后不要继续缓存旧 Native Handle。

## 13. 单位与数据交换

普通 Modeling API 不自动切换项目单位。第三方应用应在 Document/Project 层统一单位和坐标策略。

STEP/IGES 单位行为涉及文件元数据与 OCCT Translator；对工程项目有严格单位要求时，应在导入/导出边界做业务校核。

## 14. Linux 使用者

Linux 3.x 的定位是：

```text
源码可以构建
Core 可以运行
Avalonia 可以运行
不发布官方预编译 Binary/Portable Asset
```

典型验证：

```bash
./build.sh build Release
```

Linux 二进制应针对自己的目标发行版、OCCT 7.9.0 和 C/C++ Runtime 基线自行构建。

## 15. 排障

出现 Native Load 问题时首先输出：

```csharp
Console.WriteLine(OcctRuntime.GetDiagnosticReport());
```

然后检查：

1. x64 进程；
2. `runtime/OcctNative.dll` 是否存在；
3. OCCT/VC Native Closure 是否完整；
4. Managed/Native Version 与 ABI 是否一致；
5. `occt/resources` 是否完整；
6. 是否混入另一份旧 SDK；
7. `sourceCommit` 与 Hash 是否一致。

Bridge 维护者的正式 Windows 发布验证见 [构建、测试与发布](08_构建测试与发布.md)。
