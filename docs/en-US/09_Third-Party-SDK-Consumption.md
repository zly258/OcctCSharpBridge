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

## 2. Preferred Windows acquisition order

```text
Windows Release asset produced from formal main
        ↓ if unavailable
an explicitly approved main source commit
        ↓
build.ps1 dist Release
        ↓
Windows Portable SDK packager
```

### 2.1 Formal Release asset available

Prefer:

```text
OcctCSharpBridge-<version>-win-x64-portable.zip
```

A third-party consumer does not rerun Bridge ManagedTests, the consumer matrix, or viewport/window smoke for every SDK refresh. Full Bridge QA belongs to the release-production stage.

### 2.2 No formal Release asset

For a controlled build from an approved formal `main` commit:

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

`dist` is the Consumer Artifact fast path: it builds Native + Managed and writes the contract/manifest/hashes without rerunning the complete regression/smoke gate.

Record the exact `sourceCommit`. If the commit has not been confirmed through the Bridge release QA process, the output is a local consumer build rather than an official Release artifact.

## 3. Binary versus Portable SDK

### Binary SDK

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

Use it for:

- compile-time `<Reference>` values;
- CI contract/manifest/hash validation;
- Demo/internal consumer synchronization;
- builds from a known Bridge source revision.

The root `OcctNative.dll` by itself is **not a complete deployment runtime closure**.

### Portable SDK

Formal Windows deployment uses:

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

Recommended model:

> Binary SDK for compilation and identity validation; Portable SDK for Windows deployment.

Both must come from one Bridge build/source commit.

## 4. Consumer repository layout

Do not scatter SDK files through business source folders. Prefer an explicit SDK root:

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

An enterprise artifact cache is also appropriate; the SDK does not need to be committed into the business Git repository.

A shared MSBuild property can define the SDK root:

```xml
<Project>
  <PropertyGroup>
    <OcctBridgeSdkRoot>$(MSBuildThisFileDirectory)external/OcctCSharpBridge/win-x64</OcctBridgeSdkRoot>
  </PropertyGroup>
</Project>
```

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
./build.sh validate Release
./build.sh all Release
./build.sh avalonia-smoke Release   # requires DISPLAY
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
