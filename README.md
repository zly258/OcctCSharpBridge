# OcctCSharpBridge Demo

[Main SDK branch](https://github.com/zly258/OcctCSharpBridge/tree/main) · [简体中文](README.zh-CN.md) · [API inventory and usage guide](docs/API_COVERAGE.md)

The `demo` branch adds WinForms and WPF reference applications to the reusable OCCT C# bridge. `src/OcctNative`, `src/OcctNet`, `src/OcctNet.WinForms`, `src/OcctNet.Wpf`, and the shared API documentation stay synchronized with `main`; application UI, scenarios, and publishing remain demo-only.

The managed wrapper is split into:

- `OcctNet`: UI-independent viewer, modeling, analysis, healing, mesh, and exchange APIs.
- `OcctNet.WinForms`: reusable `OcctViewportControl` bound directly to a Win32 HWND.
- `OcctNet.Wpf`: reusable `OcctWpfViewport` with WPF dependency properties, event forwarding, DPI synchronization, and native resize coordination.

The WPF demo references `OcctNet.Wpf` directly and no longer constructs `WindowsFormsHost` in its application XAML.

OCAF/XDE is not included. Documents, JSON persistence, undo/redo, and command history belong to the consuming application.

## Features

- Dedicated WinForms and WPF OCCT viewport hosts
- Point, rectangle, directional crossing, multi-selection, and subshape selection
- Viewport-state snapshots, camera persistence, Z-up views, selected-object fitting, and screen-to-plane projection
- Batch color, transparency, visibility, display-mode, line-width, material, redisplay, and selection operations
- Configurable selected and hover highlight colors
- Solid or gradient backgrounds, MSAA, render resolution, shadows, ray tracing, and multi-light presets
- Curves, primitive solids, Boolean operations, features, transforms, topology queries, mesh access, and analysis
- Complex gear, multi-port manifold, and twisted-duct scenarios
- BRep vector text plus length, angle, radius, and diameter annotations
- STEP, IGES, BREP, and STL exchange
- English and Simplified Chinese UI

Complex scenarios use display batching and remove profiles, cutters, paths, and construction geometry after completion so only final results remain in the scene.

## Compatibility

- OCCT: exactly `7.9.0`
- .NET: `8.0`, Windows x64
- Bridge version: `2.3.0`
- Bridge ABI: `2`
- API count: Native `313`, P/Invoke `313`
- Deploy `OcctNet.dll`, the selected UI host assembly, and `OcctNative.dll` from the same build
- Native session disposal is idempotent and finalizer-safe; a session must still be used from one application thread at a time

## Build and run

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
.\run.ps1 winform
.\run.ps1 wpf
```

The managed-only validation path does not require the OCCT SDK:

```powershell
.\build.ps1 managed Release
```

It validates the native source list, 313-entry API surface, selection behavior, WinForms/WPF host contracts, and deployment package contract before building the core wrapper, both reusable UI hosts, and the shared demo layer.

## Publish

The default command publishes both WinForms and WPF as self-contained Windows x64 applications. Target computers do not need a separate .NET installation.

The package is deployment-complete: each executable embeds its .NET runtime, while `runtime` contains `OcctNative.dll`, the recursively resolved OCCT/third-party/Visual C++ DLL closure, and `occt/src` contains the required OCCT resources. Publishing fails when a required native dependency or OCCT resource is missing. `package-contract.json` and `native-dependencies.txt` describe the generated package.

```powershell
.\publish.ps1 -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

Publish only one application when needed:

```powershell
.\publish.ps1 winform Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 wpf Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

Create a smaller framework-dependent package only for computers that already have the .NET 8 Desktop Runtime:

```powershell
.\publish.ps1 all Release -FrameworkDependent -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

`dotnet publish` includes `OcctNet`, `OcctNet.WinForms`, and `OcctNet.Wpf` through project references. `publish.ps1` then adds the complete native dependency closure and required OCCT resources. Enable `-FullResources` or `-Diagnostics` only when needed.
