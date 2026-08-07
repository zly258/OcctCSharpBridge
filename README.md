# OcctCSharpBridge Demo

[Main SDK branch](https://github.com/zly258/OcctCSharpBridge/tree/main) · [简体中文](README.zh-CN.md) · [API inventory and usage guide](docs/API_COVERAGE.md)

The `demo` branch adds WinForms, WPF, and Avalonia reference applications to the reusable OCCT C# bridge. `src/OcctNative`, `src/OcctNet`, and the reusable UI host assemblies follow the same layered design as `main`; application UI, scenarios, run scripts, publishing, and package validation remain demo-only.

The managed wrapper is split into:

- `OcctNet`: UI-independent viewer, modeling, topology, analytic geometry, differential geometry, analysis, healing, mesh, and exchange APIs.
- `OcctNet.WinForms`: reusable `OcctViewportControl` bound directly to a Win32 HWND.
- `OcctNet.Wpf`: reusable `OcctWpfViewport` with WPF dependency properties, event forwarding, DPI synchronization, and native resize coordination.
- `OcctNet.Avalonia`: isolated Avalonia `NativeControlHost` integration. The current implementation targets Windows x64, creates its own child HWND and initializes `OcctEngine` directly on it. Mouse interaction is handled by the child HWND window procedure and render DPI is synchronized with `GetDpiForWindow`; no WinForms or WPF hosting layer is involved.

`OcctModelingSession` is organized by responsibility: lifecycle, shape queries, topology, geometry queries, analytic geometry, differential geometry, construction, algorithms, analysis, mesh, exchange, and operation history. Canonical names describe the subject and parameter semantics; existing ambiguous names remain compatibility aliases.

OCAF/XDE is not included. Documents, JSON persistence, undo/redo, and command history belong to the consuming application.

## Preview

<table>
  <tr>
    <th>WinForms · English</th>
    <th>WPF · English</th>
  </tr>
  <tr>
    <td><img src="assets/previews/winform-demo-en.webp" alt="OCCT CAD WinForms English demo" width="100%"></td>
    <td><img src="assets/previews/wpf-demo-en.webp" alt="OCCT CAD WPF English demo" width="100%"></td>
  </tr>
</table>

Avalonia now uses the same shared `CadSession` and `CadCommandCatalog` application layer as WPF. Its CAD demo includes the complete command menus, parameter input, undo/redo, file exchange, model explorer, properties, command log, view/display controls, selection tools, analysis commands, samples, shortcuts, and bilingual UI while retaining the native Avalonia `NativeControlHost` viewport.

## Features

- Dedicated WinForms, WPF, and Avalonia OCCT viewport hosts
- Point, rectangle, directional crossing, multi-selection, and subshape selection
- Avalonia native child-HWND input handling, DPI synchronization, and deterministic OCCT/window teardown ordering
- Viewport-state snapshots, camera persistence, Z-up views, selected-object fitting, and screen-to-plane projection
- Batch color, transparency, visibility, display mode, line width, material, redisplay, and selection operations
- Exact line, circle, ellipse, plane, cylinder, cone, sphere, and torus parameter queries
- Exact edge parameter ranges, first/second derivatives, tangent, normal, curvature, and center of curvature
- Surface periodicity, first/second partial derivatives, oriented normals, principal/mean/Gaussian curvature, principal directions, and umbilic state
- Configurable selected and hover highlight colors
- Solid or gradient backgrounds, MSAA, render resolution, shadows, ray tracing, and multi-light presets
- Curves, primitive solids, Boolean operations, features, transforms, topology queries, mesh access, and analysis
- Complex gear, multi-port manifold, and twisted-duct scenarios
- BRep vector text plus length, angle, radius, and diameter annotations
- STEP, IGES, BREP, and STL exchange
- English and Simplified Chinese UI

Complex scenarios use display batching and remove profiles, cutters, paths, and construction geometry after completion so only final results remain in the scene.

## Avalonia host notes

The current native OCCT viewer uses Windows `WNT_Window`, so `OcctNet.Avalonia` is explicitly a **Windows x64 / HWND** host today. Avalonia itself is cross-platform, but this repository has not yet implemented Linux `Xw_Window` or macOS native-window backends and does not claim a cross-platform OCCT viewer at this stage.

The OCCT viewport inside `NativeControlHost` is a separate native compositing layer, so normal Avalonia transparent controls should not be placed over the native 3D viewport. Rectangle selection remains rendered inside OCCT with `AIS_RubberBand`.

The teardown order is fixed: cancel interaction/capture → dispose `OcctEngine` and the OCCT viewer → restore the child HWND WndProc → destroy the child HWND. This avoids destroying the native target before OCCT releases its graphics resources.

## Compatibility

- OCCT: exactly `7.9.0`
- .NET: `8.0`, Windows x64
- Avalonia: `12.1.0`
- Bridge version: `2.5.0`
- Bridge ABI: `2`
- API count: Native `339`, P/Invoke `339`
- Viewer and interaction APIs: `221`
- Modeling APIs: `118`
- Public core .NET types: `80`
- Deploy `OcctNet.dll`, the selected UI host assembly, and `OcctNative.dll` from the same build
- A native session owns mutable state and must be used from one application thread at a time

## First-time setup

Clone the repository, switch to the demo branch, then configure the OCCT 7.9.0 SDK:

```powershell
git clone https://github.com/zly258/OcctCSharpBridge.git
cd OcctCSharpBridge
git switch demo
$env:OCCT_ROOT = "D:\\tools\\occt-vc144-64"
```

### PowerShell scripts

`build.ps1` is the build/validation entry point. Supported targets are `validate`, `native`, `managed`, `smoke`, `winform`, `wpf`, `avalonia`, and `all`. `validate` does not require an OCCT SDK; native/demo/smoke targets do.

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 avalonia Release
.\build.ps1 all Release
.\build.ps1 smoke Release
```

`run.ps1` starts an **already-built** executable; it does not rebuild the project. Syntax:

```powershell
.\run.ps1 <winform|wpf|avalonia> [Release] [-OcctRoot <path>]
```

Examples:

```powershell
.\run.ps1 winform
.\run.ps1 wpf
.\run.ps1 avalonia
```

`publish.ps1` creates deployment-complete packages for WinForms and WPF. Avalonia is currently covered by build/run/CI but is not yet part of the formal publish target.

```powershell
.\publish.ps1 all Release -Zip -OcctRoot "D:\\tools\\occt-vc144-64"
.\publish.ps1 winform Release -Zip -OcctRoot "D:\\tools\\occt-vc144-64"
.\publish.ps1 wpf Release -Zip -OcctRoot "D:\\tools\\occt-vc144-64"
```

### Display defaults

WinForms, WPF, and Avalonia start in shaded mode with face edges enabled. Use **View → Visual Styles → Shaded with Edges** to toggle face-boundary drawing independently from Shaded/Wireframe.

## Build and run

Set the OCCT SDK location once or pass it explicitly:

```powershell
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"
.\build.ps1 all Release
.\run.ps1 winform
.\run.ps1 wpf
.\run.ps1 avalonia
```

Build and run only the Avalonia demo:

```powershell
.\build.ps1 avalonia Release -OcctRoot "D:\tools\occt-vc144-64"
.\run.ps1 avalonia Release -OcctRoot "D:\tools\occt-vc144-64"
```

The managed-only path does not require an OCCT SDK:

```powershell
.\build.ps1 managed Release
```

Validation covers the bridge version, API organization, analytic geometry, differential geometry, native declarations and implementations, Cdecl and exact symbol spelling, selection, WinForms/WPF/Avalonia viewport hosts, native source boundaries, and deployment package contract.

Native compilation and runtime smoke testing:

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

## Troubleshooting

- If `run.ps1` starts an old executable, rebuild the relevant target first; the runner does not compile.
- If Avalonia exits during startup, inspect `src\CadAvalonia\bin\x64\<Configuration>\net8.0-windows\CAD-Avalonia.log`.
- If native loading fails, rebuild with the correct `-OcctRoot` and make sure OCCT/third-party runtime DLLs are available.
- `build.ps1 validate` is the fastest check after API/menu/host changes; `build.ps1 smoke` verifies real native modeling.

## Publish

The existing `publish.ps1` continues to produce deployment-complete WinForms and WPF packages. Avalonia is now a full CAD demo in build, run, and CI; adding it to the formal publishing flow remains a separate packaging task.

The default command publishes both WinForms and WPF as self-contained Windows x64 applications. Target computers do not need a separate .NET installation.

The package is deployment-complete: each executable embeds its .NET runtime, while `runtime` contains `OcctNative.dll`, the recursively resolved OCCT/third-party/Visual C++ DLL closure, and `occt/src` contains the required OCCT resources. Publishing fails when a required native dependency or OCCT resource is missing. `package-contract.json` and `native-dependencies.txt` describe the generated package.

```powershell
.\publish.ps1 -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

Publish only one application:

```powershell
.\publish.ps1 winform Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 wpf Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

Create a smaller framework-dependent package only for computers that already have the .NET 8 Desktop Runtime:

```powershell
.\publish.ps1 all Release -FrameworkDependent -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

`dotnet publish` includes the matching `OcctNet` and UI-host assemblies through project references. `publish.ps1` then adds the complete native dependency closure and required OCCT resources. Enable `-FullResources` or `-Diagnostics` only when needed.
