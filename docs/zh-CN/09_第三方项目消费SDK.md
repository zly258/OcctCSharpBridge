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

## 2. Windows 系统级 SDK

Windows 只维护一套机器级 Binary SDK：

```text
C:\Program Files\OcctCSharpBridge\SDK\3.0\win-x64
```

开发机或构建机可用 `OCCTCSHARPBRIDGE_SDK` 覆盖。Consumer 不再克隆 Bridge、不运行 sync 脚本，也不在业务仓库保存第二套 Binary SDK。

```xml
<PropertyGroup>
  <OcctBridgeSdkRoot Condition="'$(OCCTCSHARPBRIDGE_SDK)' != ''">$(OCCTCSHARPBRIDGE_SDK)</OcctBridgeSdkRoot>
  <OcctBridgeSdkRoot Condition="'$(OcctBridgeSdkRoot)' == ''">$(ProgramFiles)\OcctCSharpBridge\SDK\3.0\win-x64</OcctBridgeSdkRoot>
</PropertyGroup>
```

SDK 不存在时应直接构建失败，不要静默回退到仓库缓存。

Bridge 维护者通过 `.\publish.ps1` 更新系统 SDK。兼容的 3.0.x 更新保持 `SDK\3.0\win-x64` 路径稳定，精确 Patch Version 与 Source Commit 由 Contract/Manifest 记录。

## 3. SDK 身份

安装目录中的 Managed DLL、`OcctNative.dll`、Contract、Manifest 必须视为一个原子整体。`Private=true` 复制到应用输出目录属于正常构建输出，不代表维护了另一套源 SDK。

## 4. Native Viewport 生命周期

WPF/Avalonia 正常把 Viewport 加入 Visual Tree。依赖首帧已经实际呈现的操作等待 `HostState == Ready`。

Bridge 只会在获得有效布局尺寸并至少完成一次 `ResizeSurface + Redraw` 后进入 `Ready`。不要使用鼠标移动、固定延时、重复 `FitAll` 或额外启动 `Redraw` 规避首帧问题。

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
