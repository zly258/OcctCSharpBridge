# Third-party SDK Consumption

This guide is for external CAD/BIM/engineering applications that **consume OcctCSharpBridge without participating in Bridge source development**. It defines how to reference, validate, deploy, and upgrade an SDK produced from formal `main`.

Core rule:

> A third-party project consumes a versioned, platform-specific, ABI/source-identified SDK artifact. It should not assemble a runtime by copying arbitrary Bridge/OCCT binaries from different builds.

## 1. Which artifact should an application use?

OcctCSharpBridge exposes two related artifact types.

### 1.1 Minimal Binary SDK

Use it for:

- compile-time `<Reference>` inputs;
- CI contract/manifest/hash validation;
- Demo or controlled internal consumer synchronization;
- application builds pinned to a known Bridge source revision.

Windows x64:

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

Linux x64:

```text
libOcctNative.so
OcctNet.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

The minimal Binary SDK is **not a complete runtime package**. It does not contain the OCCT native dependency closure or the complete OCCT resource tree.

### 1.2 Portable SDK

Use it for final application deployment and redistribution. In addition to the managed assemblies it contains:

```text
runtime/
  OcctNative / libOcctNative
  OCCT native libraries
  packaged third-party native dependencies
occt/resources/
package-manifest.json
licenses/notices
```

A formal third-party application should prefer a Portable SDK produced from formal `main` by `publish.ps1` / `publish.sh`, or the corresponding reviewed release asset.

**Recommended model: use the Binary SDK for compilation and the Portable SDK for deployment.** Both artifacts must originate from the same Bridge build/source commit.

## 2. Current compatibility contract

Before integration, verify at least:

| Item | Current contract |
| --- | --- |
| Bridge | `3.0.0-preview.1` |
| Native ABI | ABI 5 only |
| OCCT | 7.9.0 |
| Architecture | x64 |
| Core/Avalonia Binary TFM | `net8.0` |
| WinForms/WPF Binary TFM | `net8.0-windows` |
| Supported consumers | .NET 8 / 9 / 10 |
| Windows UI | WinForms / WPF / Avalonia |
| Linux UI | Avalonia |

A consuming project does not need to target the same TFM as the Bridge binary. For example, a `net10.0` application can reference the `net8.0` Bridge assemblies. Windows desktop consumers may use supported `net8.0-windows`, `net9.0-windows`, or `net10.0-windows` targets.

The stable .NET 10 SDK used to compile Bridge source is not the third-party application's runtime requirement. Binary consumers do not need an exact `10.0.303` SDK merely because Bridge source uses C# 14.

## 3. Recommended third-party repository layout

Do not scatter Bridge files across business projects. Define one external SDK root:

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

The SDK can also live in a company artifact/cache directory instead of business Git. The important point is to make the SDK root configurable and identify it from its manifest rather than from a folder name.

A shared `Directory.Build.props` can define the root:

```xml
<Project>
  <PropertyGroup>
    <OcctBridgeRid Condition="$([MSBuild]::IsOSPlatform('Windows'))">win-x64</OcctBridgeRid>
    <OcctBridgeRid Condition="$([MSBuild]::IsOSPlatform('Linux'))">linux-x64</OcctBridgeRid>
    <OcctBridgeSdkRoot>$(MSBuildThisFileDirectory)external/OcctCSharpBridge/$(OcctBridgeRid)</OcctBridgeSdkRoot>
  </PropertyGroup>
</Project>
```

CI can override it:

```bash
dotnet build -p:OcctBridgeSdkRoot=/opt/company-sdk/OcctCSharpBridge/linux-x64
```

## 4. Headless/Core application

A project that uses geometry/modeling/topology/mesh/exchange without a Bridge UI host references only `OcctNet.dll`:

```xml
<ItemGroup>
  <Reference Include="OcctNet">
    <HintPath>$(OcctBridgeSdkRoot)/OcctNet.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>
```

Minimal code:

```csharp
using OcctNet;

OcctRuntime.Configure();

using var model = new OcctModelingSession();
var box = model.MakeBox(100, 80, 10);
model.ExportStep(box, "box.step");
```

## 5. Avalonia application

Avalonia requires Core plus the Avalonia adapter:

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

The consuming application owns its Avalonia framework package versions. Do not vendor the Demo project merely to use `OcctNet.Avalonia`.

Linux exposes Core + Avalonia managed surfaces only; WinForms/WPF are outside the Linux contract.

## 6. WinForms application

Windows WinForms:

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

Example framework settings:

```xml
<TargetFramework>net10.0-windows</TargetFramework>
<UseWindowsForms>true</UseWindowsForms>
```

## 7. WPF application

Windows WPF:

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

Example:

```xml
<TargetFramework>net10.0-windows</TargetFramework>
<UseWPF>true</UseWPF>
```

Do not reference WinForms, WPF and Avalonia adapters together merely for convenience. Reference only the UI surface the application uses.

## 8. Compile-time references and deployment are different concerns

MSBuild `<Reference Private="true">` copies managed assemblies to output and may also leave the flat native Bridge from a minimal SDK in ordinary build output. The **final deployed application must not rely on only the flat native file from the minimal SDK**.

Formal deployment should merge the Portable SDK runtime layout:

```text
MyCadApp/
  MyCadApp.exe                   # or Linux apphost
  MyCadApp.dll
  OcctNet.dll
  OcctNet.Wpf.dll                # example
  runtime/
    OcctNative.dll               # Windows
    TKernel.dll
    TK*.dll
    ...
  occt/
    resources/
      ...
```

Linux:

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

If ordinary `dotnet publish` places a flat `OcctNative.dll` / `libOcctNative.so` from the minimal SDK at the application root and the final package also merges Portable `runtime/`, remove the root-level flat native Bridge from the final package. This avoids selecting a native file that does not have the adjacent OCCT closure expected by the deployment layout.

## 9. Runtime initialization

Call this very early in application startup, before any native-backed Bridge object is created:

```csharp
OcctRuntime.Configure();
```

This includes the first:

- `OcctModelingSession`;
- `OcctEngine`;
- WinForms/WPF/Avalonia viewport host.

The default Portable layout does not require the target machine to reproduce the developer machine's `OCCT_ROOT` / `CASROOT`.

An enterprise application may explicitly configure another install layout, but the native directory and OCCT root must still belong to one coherent SDK payload.

## 10. The application's own .NET deployment

The Portable SDK does not bundle the consuming application's .NET runtime. The application chooses independently.

Framework-dependent:

```bash
dotnet publish -c Release --self-contained false
```

Self-contained Windows:

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

Self-contained Linux:

```bash
dotnet publish -c Release -r linux-x64 --self-contained true
```

Then merge the matching Bridge Portable SDK runtime/resources into the application package.

Bridge Portable deployment and .NET self-contained deployment solve different problems: the first provides the Bridge/OCCT native closure; the second provides the .NET runtime.

## 11. CI checks before accepting an SDK

A third-party CI should verify at least:

1. `bridge-contract.json` exists;
2. `bridge-manifest.json` exists;
3. `platform` matches the target RID;
4. ABI `current = 5` and `minimumSupported = 5`;
5. Bridge version satisfies the application's pinned policy;
6. `sourceCommit` is non-empty and is an approved formal source revision;
7. every SHA-256 in the Binary SDK manifest matches the local file;
8. Portable `bridgeSourceCommit` / `bridgeVersion` exactly match the Binary SDK;
9. every Portable package-manifest hash matches.

Do not identify an SDK by file names, directory timestamps, or an arbitrary DLL version alone.

## 12. Recommended version-pinning policy

An enterprise build can pin:

```text
BridgeVersion = 3.0.0-preview.1
SourceCommit   = <reviewed main commit>
NativeAbi      = 5
Platform       = win-x64 / linux-x64
```

Upgrade as one transaction:

```text
download new Binary + Portable SDK
        ↓
validate contract / manifest / hashes
        ↓
replace all OcctNet*.dll
        ↓
replace runtime/
        ↓
replace occt/
        ↓
rebuild the application
        ↓
run application regression tests
```

Never:

```text
replace only OcctNet.dll
keep an old OcctNative
keep old runtime/TK*.dll
mix win-x64 and linux-x64 payloads
assemble one package from two source commits
```

## 13. Building an SDK from Bridge source

Most external teams should consume formal release artifacts. If an enterprise build must generate an SDK from an approved Bridge source revision, use the consumer artifact fast path.

Windows:

```powershell
git switch main
git reset --hard <approved-main-commit>
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux:

```bash
git switch main
git reset --hard <approved-main-commit>
./build.sh dist Release
```

`dist` generates the consumer Binary SDK; it does not certify that the complete Bridge release gate has passed. Formal external artifacts should still come from Bridge `publish` or an equivalent controlled enterprise gate.

If a validated SDK already exists, the consuming project has no reason to compile Bridge again.

## 14. Linux compatibility requirements

Linux Portable Runtime carries OCCT and selected non-system native libraries, but it does not make glibc/libstdc++ ABI requirements disappear.

Errors such as:

```text
GLIBC_2.xx not found
GLIBCXX_3.4.xx not found
CXXABI_1.x.x not found
```

mean the native Bridge/OCCT build baseline is newer than the target system. The correct solution is to rebuild OCCT and `libOcctNative.so` against an older supported ABI baseline, not to copy an arbitrary `libc.so.6` into the application package.

AppImage improves distribution convenience but does not automatically transform an ELF already linked against a newer glibc into an old-glibc-compatible binary.

If a product claims support for a Debian/Ubuntu/Kylin matrix, define the oldest native build baseline and run real launch/render/modeling tests on that support matrix.

## 15. Common failures

### `DllNotFoundException` / unable to load OcctNative

Check:

- `OcctRuntime.Configure()` runs early enough;
- `runtime/` exists;
- the native closure is complete;
- process and package are x64;
- Windows loader dependencies are available;
- Linux `ldd runtime/libOcctNative.so` has no `not found` or ABI-version errors.

### Native loads, but STEP/IGES/shader operations fail

Check `occt/resources/` and the effective `CSF_*` resource configuration.

### Compile-time `OcctNet.*` reference failure

Check `OcctBridgeSdkRoot` and `<HintPath>`. Do not use Portable `runtime/` as the managed reference root.

### ABI/version mismatch

Do not suppress the error or edit a manifest. Redeploy Managed + Native + runtime + resources from one build.

## 16. License and third-party notices

If an application redistributes Bridge/OCCT/third-party native files from the Portable SDK, preserve the applicable license and third-party notice material and comply with the project license/exception and the licenses of packaged third-party components.

The authoritative terms are the repository `LICENSE`, LGPL text, OcctCSharpBridge exception, `COMMERCIAL.md`, `THIRD_PARTY_NOTICES.md`, and the relevant third-party licenses.

## 17. Role of the Demo

`demo` / `demo-dev` are reference consumers useful for studying:

- Binary SDK `<Reference HintPath>` integration;
- WinForms/WPF/Avalonia hosts;
- `OcctRuntime` configuration;
- Portable Runtime merging;
- contract/sourceCommit/hash validation.

A third-party project should not copy the Demo's menu/command/business UI as its application architecture. The Demo is an SDK consumer example, not an application framework.
