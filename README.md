# OcctCSharpBridge Demo

[简体中文](README.zh-CN.md) · [Main SDK](https://github.com/zly258/OcctCSharpBridge) · [Demo Maintenance](docs/README.md) · [English API Reference](https://github.com/zly258/OcctCSharpBridge/tree/main/docs/en-US/api)

The `demo` branch is the reference **Binary SDK consumer** for OcctCSharpBridge. It contains application/demo code only and does not mirror the Bridge native or managed source tree.

## Project information

| Item | Current value |
| --- | --- |
| Author | **zly258** |
| Demo / Bridge version | **2.6.0** |
| Native ABI | **4** |
| Open CASCADE Technology | **7.9.0** |
| .NET SDK | **10.0.302** |
| Target framework | **`net10.0-windows`** |
| C# | **14.0** |
| Native Bridge | **C++17** |
| Avalonia | **12.1.0** |
| Platform | **Windows x64** |
| SDK surface | **344 Native exports / 344 P/Invoke / 105 public .NET types** |

The Binary SDK contract in `dist/win-x64/bridge-contract.json` and `bridge-manifest.json` is authoritative for the consumed Bridge build.

## Applications

```text
src/OcctDemo.Common
src/OcctDemo.WinForms
src/OcctDemo.Wpf
src/OcctDemo.Avalonia
```

All three desktop demos share the same product metadata and About information through `OcctDemo.Common/DemoProductInfo.cs`, so author, version and technology baseline cannot drift between WinForms, WPF and Avalonia.

## Shared interaction and modeling coverage

WinForms, WPF and Avalonia use the same demo behavior and cover the following interaction and modeling scenarios:

- object, vertex, edge, wire, face, shell and solid selection filters; the WinForms toolbar selector is wired directly to the Viewer selection mode;
- rectangle/Ctrl multi-selection shows only the selected object count instead of presenting the first object as the active property object;
- all three Viewports consume the Binary SDK `ZoomSensitivity` API directly, defaulting to `1.0`, with mouse-wheel sensitivity configurable from the View menu;
- Undo/Redo is driven by the shared history model; Avalonia menu and toolbar states refresh from `HistoryChanged` and `ModelChanged`;
- the Avalonia main window, property area, log, status bar and parameter dialog use `Microsoft YaHei UI` consistently;
- the Samples menu includes a **B-Spline Surface Test** that creates a real non-ruled loft and validates degrees, poles, weights, knots and multiplicities;
- the Samples menu includes a **Mesh Generation Test** that calls `GetShapeMeshData` and validates face count, nodes, triangles and contiguous per-face provenance ranges.

These are executable Bridge API tests rather than static screenshots; results are written to the demo command log.

## Binary SDK

`main/publish.ps1` publishes the validated SDK into this branch:

```text
dist/win-x64/
├─ OcctNative.dll
├─ OcctNet.dll
├─ OcctNet.WinForms.dll
├─ OcctNet.Wpf.dll
├─ OcctNet.Avalonia.dll
├─ bridge-contract.json
└─ bridge-manifest.json
```

The demo branch does **not** own a reverse synchronization script. SDK publication always starts from `main`; the main script already provides the normal default OCCT root:

```powershell
# main branch
.\publish.ps1
```

The main publish flow generates the bilingual API Reference, runs the Release native/managed build, creates the Binary SDK, then synchronizes `dist/win-x64` to `demo` through a temporary worktree. Managed regression and Native Smoke remain explicit test targets and no longer block Binary SDK publication.

## Requirements

To build the demos:

- Windows 10/11 x64
- .NET SDK `10.0.302`
- a valid `dist/win-x64` Binary SDK

CMake and MSVC are **not** required to build the demo applications. They are required only when producing the Bridge Binary SDK on `main`.

To run the demos, OCCT 7.9.0 runtime dependencies must also be available through `OCCT_ROOT`, `CASROOT`, or explicit `OcctRuntime` configuration.

## Build

```powershell
.\build.ps1 validate Release
.\build.ps1 all Release
```

Individual applications:

```powershell
.\build.ps1 common Release
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 avalonia Release
```

`validate` checks the Binary SDK contract, manifest and SHA-256 hashes.

## Run

```powershell
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"

.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

`OcctNative.dll` is copied beside each executable. The run script configures OCCT and third-party runtime search paths.

## Publish demo applications

```powershell
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

All three demos are merged into one package directory and share one .NET, Bridge and OCCT runtime instead of creating three duplicated runtime trees:

```text
artifacts/publish/OcctCSharpBridge-Demo-all-win-x64/
├─ CAD-Winform.exe
├─ CAD-WPF.exe
├─ CAD-Avalonia.exe
├─ OcctDemo.Common.dll
├─ OcctNet*.dll
├─ OcctNative.dll
├─ required OCCT 7.9 runtime modules
└─ shared .NET/Avalonia runtime
```

The publisher copies only the OCCT toolkits used by the Bridge and their required transitive runtime dependencies. It no longer recursively copies the full OCCT/third-party SDK, so Qt, VTK, Tcl/Tk, Draw/Test and debug DLLs are excluded. A small CAF/XCAF runtime subset remains because OCCT's STEP/IGES toolkits depend on it internally; this does not mean the Demo or Bridge uses OCAF/XDE as its document architecture.

Self-contained remains the default. When the target machine already has the .NET 10 Desktop Runtime, use a smaller framework-dependent package:

```powershell
.\publish.ps1 all Release -FrameworkDependent -Zip
```

The demo `publish.ps1` packages demo applications only. It consumes the Binary SDK and never builds Bridge source.

## Structure

```text
dist/win-x64/               Validated Binary SDK published by main
src/OcctDemo.Common/        Shared demo behavior and product metadata
src/OcctDemo.WinForms/      WinForms demo
src/OcctDemo.Wpf/           WPF demo
src/OcctDemo.Avalonia/      Avalonia 12.1.0 demo
assets/previews/            Canonical bilingual screenshots
docs/README.md              Demo maintenance contract
OcctDemo.sln                Demo-only solution
build.ps1                   Demo build entry point
run.ps1                     Local runner
publish.ps1                 Demo application publisher
```

## Dependency rules

- Demo projects reference `dist/win-x64/OcctNet*.dll`, never Bridge `.csproj` files.
- Demo contains no `src/OcctNative`, `src/OcctNet*`, Bridge tests, CMake build, or ABI producer scripts.
- Bridge conceptual docs and complete bilingual Managed + Native API Reference are maintained under `main/docs/zh-CN` and `main/docs/en-US`.
- Removed legacy aliases and compatibility wrappers are not restored for Demo convenience.
- GitHub Actions are not used for build, validation, or branch synchronization.

## Runtime troubleshooting

For local `run.ps1`, if `DllNotFoundException` or Win32 error 126 occurs, verify:

```text
application/OcctNative.dll
%OCCT_ROOT%/win64/vc14/bin/TKernel.dll
%OCCT_ROOT%/3rdparty-vc14-64/**/bin/required runtime files
```

The formal `publish.ps1` output places the required native runtime beside the three executables, so the target machine does not need a complete OCCT SDK installation.

The Avalonia host uses a Windows child HWND, so WinForms, WPF and Avalonia demos all target Windows x64.

## Author

**zly258**  
zhangly1403@gmail.com

## License

OcctCSharpBridge is licensed under the [PolyForm Noncommercial License 1.0.0](LICENSE).
