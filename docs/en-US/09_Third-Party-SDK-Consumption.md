# Third-party SDK Consumption

This guide is for CAD/BIM/engineering applications that **consume OcctCSharpBridge without developing the Bridge itself**. It describes how to reference, validate, deploy, and upgrade the 3.x Stable SDK.

Core principle:

> A consumer accepts a versioned, platform-specific SDK artifact constrained by ABI, source commit, and hashes. It does not assemble a runtime by copying arbitrary DLLs from a Bridge repository or development machine.

## 1. 3.0 support model

| Item | 3.0 Stable |
| --- | --- |
| Bridge | `3.0.0` |
| Native ABI | ABI 5 only |
| OCCT | 7.9.0 exact |
| Core/Avalonia Binary TFM | `net8.0` |
| WinForms/WPF Binary TFM | `net8.0-windows` |
| Consumers | .NET 8 / 9 / 10 |
| Official prebuilt SDK | **Windows x64** |
| Linux | source build support, Avalonia |

Windows is the official prebuilt distribution platform. Linux does not receive an official 3.x Binary/Portable Release asset; Linux consumers build in an environment compatible with the target distribution.

## 2. Installed Windows SDK

Use one machine-wide Binary SDK:

```text
C:\Program Files\OcctCSharpBridge\SDK\3.0\win-x64
```

A machine may override this with `OCCTCSHARPBRIDGE_SDK`. Consumers do not clone Bridge, run sync scripts, or keep a second Binary SDK under the application repository.

```xml
<PropertyGroup>
  <OcctBridgeSdkRoot Condition="'$(OCCTCSHARPBRIDGE_SDK)' != ''">$(OCCTCSHARPBRIDGE_SDK)</OcctBridgeSdkRoot>
  <OcctBridgeSdkRoot Condition="'$(OcctBridgeSdkRoot)' == ''">$(ProgramFiles)\OcctCSharpBridge\SDK\3.0\win-x64</OcctBridgeSdkRoot>
</PropertyGroup>
```

If the SDK is missing, fail the build instead of silently falling back to a repository cache.

Bridge maintainers update the installed SDK with `.\publish.ps1`. Compatible 3.0.x updates keep the stable `SDK\3.0\win-x64` path; the exact patch version and source commit remain in the contract and manifest.

## 3. SDK identity

The installed managed assemblies, `OcctNative.dll`, contract, and manifest are one atomic payload. `Private=true` may copy assemblies into application output, which is normal build output rather than another source SDK.

## 4. Native viewport lifecycle

WPF and Avalonia applications create the viewport normally in the visual tree. When an operation depends on a presented first native frame, wait for `HostState == Ready`.

The Bridge reaches `Ready` only after a usable arranged size and at least one `ResizeSurface + Redraw`. Do not add mouse-motion, fixed-delay, duplicate-`FitAll`, or startup redraw workarounds.

## 5. Core reference

```xml
<ItemGroup>
  <Reference Include="OcctNet">
    <HintPath>$(OcctBridgeSdkRoot)\OcctNet.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>
```

Consumer projects may target `net8.0`, `net9.0`, or `net10.0`. One net8-based Bridge managed assembly set is used; there is not a different Binary SDK for each runtime major.

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

Windows Desktop consumers may also use `net9.0-windows` or `net10.0-windows`.

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

Windows Avalonia:

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

A Linux Avalonia application must not consume the Windows SDK. Build the Linux Binary SDK from Bridge source in the Linux target environment and apply the same source-identity rules to `OcctNet.dll` / `OcctNet.Avalonia.dll`.

## 9. Runtime initialization

With the Portable SDK layout, configure the runtime before creating the first native-backed object:

```csharp
using OcctNet;

OcctRuntime.Configure();

using var model = new OcctModelingSession();
var box = model.MakeBox(100, 80, 20);
```

This must happen before the first `OcctEngine`, `OcctModelingSession`, or other operation that causes `OcctNative` to load.

Recommended application layout:

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

Do not leave a stale root-level `OcctNative.dll` that competes with `runtime/OcctNative.dll`.

## 10. Validate an SDK before accepting it

At minimum inspect:

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

A Windows 3.0 Stable SDK should satisfy:

```text
Bridge version: compatible 3.0.x line
ABI:            5 / 5
Platform:       win-x64
OCCT:           7.9.0
Configuration:  Release
```

Do not edit manifests to bypass managed/native version mismatches.

## 11. Upgrade as one payload

Replace together:

```text
OcctNet*.dll
runtime/
occt/resources/
bridge-contract.json
bridge-manifest.json
package-manifest.json
```

Do not combine:

```text
old OcctNet.dll + new OcctNative.dll
new managed assemblies + old runtime/
runtime/ and resources/ from different source commits
```

Record the accepted Bridge version and source commit in the business application's build log or artifact metadata.

## 12. Threading and lifetime

Consumers must follow [Stable Support and Compatibility](10_Stable-Support-and-Compatibility.md):

- one `OcctEngine` / `OcctModelingSession` is not a concurrent thread-safe object by default;
- UI host lifecycle operations follow the UI thread;
- Engine/Session IDs are not mixed across owners;
- `IDisposable` owned resources are released according to their contract;
- stale native handles are not retained across host recreation.

## 13. Units and exchange

Ordinary modeling APIs do not silently switch project units. The application Document/Project layer owns a consistent unit and coordinate policy.

STEP/IGES unit behavior also involves file metadata and the OCCT translator. Engineering applications with strict unit requirements should validate import/export boundaries explicitly.

## 14. Linux consumers

The 3.x Linux position is:

```text
source builds
Core runs
Avalonia runs
no official prebuilt Binary/Portable Release asset
```

Typical validation:

```bash
./build.sh build Release
```

Build Linux binaries for the intended distribution, OCCT 7.9.0 installation, and C/C++ runtime baseline.

## 15. Troubleshooting

Start with:

```csharp
Console.WriteLine(OcctRuntime.GetDiagnosticReport());
```

Then verify:

1. x64 process;
2. `runtime/OcctNative.dll` exists;
3. OCCT/VC native closure is complete;
4. managed/native versions and ABI match;
5. `occt/resources` is complete;
6. no stale second SDK is being loaded;
7. `sourceCommit` and hashes match.

Bridge maintainer release validation is described in [Build, Test and Publish](08_Build-Test-and-Publish.md).
