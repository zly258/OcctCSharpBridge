# OcctCSharpBridge

[简体中文](README.zh-CN.md) · [Desktop demos](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge is a Windows x64 bridge from **Open CASCADE Technology 7.9.0** to **.NET 8**. The `main` branch is intentionally kept reusable: it contains the native C++ bridge, stable C ABI, type-safe managed wrapper, optional viewport host libraries, API contract checks, and modeling smoke scenarios. Complete CAD applications are maintained on the `demo` branch.

The bridge intentionally excludes OCAF/XDE as an application document layer. Documents, entities, command history, undo/redo, JSON persistence, tools, snapping, and other application concepts belong to the consuming program.

## Requirements

- Windows x64
- Visual Studio 2022 / MSVC v143-compatible toolchain
- .NET SDK **8.0.423**, pinned by `global.json`
- C# 12.0
- CMake 3.21 or newer
- Open CASCADE Technology **7.9.0**, VC14 x64 layout
- PowerShell 5.1+ or PowerShell 7+

A typical OCCT directory used by the scripts looks like:

```text
D:\tools\occt-vc144-64\
├─ inc\
├─ win64\vc14\bin\
├─ win64\vc14\lib\
└─ 3rdparty-vc14-64\
```

## Clone first

Start from a normal Git clone. The default branch is `main`.

```powershell
git clone https://github.com/zly258/OcctCSharpBridge.git
cd OcctCSharpBridge
git switch main
```

Configure OCCT once for the current PowerShell session:

```powershell
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"
```

You can also omit the environment variable and pass `-OcctRoot` explicitly to every command that needs the native SDK.

## Repository structure

```text
bridge-contract.json    Authoritative Bridge/ABI/OCCT/.NET/API metadata
global.json             Pinned .NET SDK used locally and in CI
Directory.Build.props   Shared C# compiler policy
src/OcctNative          C++17 native bridge and stable C ABI
src/OcctNet             UI-independent, type-safe .NET wrapper
src/OcctNet.WinForms    Optional WinForms HWND viewport host
src/OcctNet.Wpf         Optional WPF viewport host
tests                   API contract checks and native modeling smoke scenarios
docs                    English and Chinese API inventories
build.ps1               Validation/build/CI/smoke entry point
```

`bridge-contract.json` is the metadata source of truth. Version/API checks, branch synchronization, build output paths, and the website contract validate against this file rather than maintaining independent expected values.

The managed API is organized around two native session types:

- `OcctEngine`: interactive Viewer/AIS session, camera, projection, display attributes, object identity, selection, transforms, annotations, screen/world conversion and input-oriented operations.
- `OcctModelingSession`: headless geometry, topology, construction, algorithms, mesh, analysis, healing, history and engineering file exchange.

`OcctViewportControl` is provided by `OcctNet.WinForms`; `OcctWpfViewport` is provided by `OcctNet.Wpf`. They are optional host libraries and are not application frameworks.

## `build.ps1` usage

General syntax:

```powershell
.\build.ps1 <target> <configuration> [-OcctRoot <path>]
```

Supported targets on `main`:

| Target | What it does | OCCT SDK required |
| --- | --- | --- |
| `validate` | Runs API/version/source-organization/PInvoke/UI-host contract checks | No |
| `managed` | Builds `OcctNet`, WinForms host and WPF host | No |
| `ci` | Runs the same contract checks as `validate`, then builds all reusable managed projects and compiles the Smoke project | No |
| `native` | Configures and builds `OcctNative.dll` with CMake/MSVC | Yes |
| `smoke` | Builds native + managed components and runs real native modeling smoke scenarios | Yes |
| `all` | Builds native bridge and all reusable managed host projects | Yes |

Configurations: `Debug`, `Release`, `RelWithDebInfo`.

### Validate API contracts

Use this after changing public APIs, native declarations, P/Invoke signatures or project organization:

```powershell
.\build.ps1 validate Release
```

### Run the same managed build as GitHub Actions

```powershell
.\build.ps1 ci Release
```

This is the preferred pre-push check when the OCCT SDK is not available. GitHub Actions calls this same target instead of duplicating individual `dotnet build` commands.

### Build only the managed wrapper

```powershell
.\build.ps1 managed Release
```

### Build only the native bridge

```powershell
.\build.ps1 native Release -OcctRoot "D:\tools\occt-vc144-64"
```

### Build the complete reusable bridge

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

When `OCCT_ROOT` is already set:

```powershell
.\build.ps1 all Release
```

### Run native modeling smoke scenarios

This is the strongest local check on `main` because it loads the native bridge and performs real OCCT modeling operations:

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

## GitHub Actions and native smoke testing

The normal `API Surface` workflow runs `build.ps1 ci Release` on a GitHub-hosted Windows runner and therefore does not require an OCCT SDK.

A separate `Native Smoke` workflow is available for a real OCCT runtime test. The repository intentionally does not vendor the OCCT SDK, so this job remains skipped until the repository is explicitly configured with:

- Repository variable `OCCT_NATIVE_CI_ENABLED=true`
- Repository secret `OCCT_SDK_URL`: URL to a ZIP archive containing the expected `inc` and `win64\vc14` OCCT 7.9.0 layout
- Optional repository secret `OCCT_SDK_SHA256`: SHA-256 of that ZIP archive

When enabled, the workflow downloads and validates the SDK archive, builds `OcctNative.dll`, loads the native bridge, and executes the real modeling Smoke scenarios through `build.ps1 smoke Release`.

## Run desktop applications

`main` deliberately does not contain complete CAD demo executables. Switch to `demo` for WinForms, WPF and Avalonia applications:

```powershell
git switch demo
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"
.\build.ps1 all Release
.\run.ps1 winform
.\run.ps1 wpf
.\run.ps1 avalonia
```

The `demo` README documents `run.ps1` and `publish.ps1` in detail.

## Referencing the bridge from another project

Project references during development:

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
  <!-- Optional WinForms host -->
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.WinForms\OcctNet.WinForms.csproj" />
  <!-- Optional WPF host -->
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.Wpf\OcctNet.Wpf.csproj" />
</ItemGroup>
```

For deployment, keep `OcctNet.dll`, the selected host assembly, `OcctNative.dll`, OCCT runtime DLLs and required third-party runtime DLLs from the same compatible build. Do not mix native and managed outputs from different bridge revisions.

## Compatibility contract

The authoritative values are stored in `bridge-contract.json`:

- OCCT: exactly `7.9.0`
- Managed target: `.NET 8`, Windows x64
- Bridge version: `2.5.0`
- Native ABI version: `2`
- Native and managed ABI compatibility is validated through `OcctBridgeInfo`
- Native sessions own mutable state and should be used from one application thread at a time

Batch color, transparency, visibility, display-mode, material, line-width, redisplay and selection APIs reduce repeated P/Invoke calls for large scenes. Viewport-state snapshots, transformed bounds, selected-object fitting, screen projection helpers and stable `ApplicationTag` identity are intended for reusable CAD application layers.

## API inventory

- [English API inventory](docs/API_COVERAGE.md)
- [中文接口清单](docs/API_COVERAGE.zh-CN.md)

`build.ps1 validate` intentionally fails when declarations, P/Invoke mappings, calling conventions, source organization, documented interface counts, SDK policy, or contract metadata become stale.

## Troubleshooting

**`OCCT_ROOT is not configured`**  
Set `$env:OCCT_ROOT` or pass `-OcctRoot` explicitly.

**`TKernel.lib` / `TKernel.dll` not found**  
Verify that the OCCT installation follows the expected `win64\vc14\lib` and `win64\vc14\bin` layout and is version 7.9.0.

**Managed build works but native loading fails**  
A managed build alone does not deploy the OCCT runtime. Build the native target and make the matching OCCT/third-party runtime DLLs discoverable beside the application or through the configured runtime path.

**Need a runnable example**  
Use the `demo` branch rather than adding application-specific code to `main`.

## License

The project is provided under the [PolyForm Noncommercial License 1.0.0](LICENSE). Open CASCADE Technology and third-party components remain subject to their own licenses.
