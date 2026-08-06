# OcctCSharpBridge Demo

[Main SDK branch](https://github.com/zly258/OcctCSharpBridge/tree/main) · [简体中文](README.zh-CN.md) · [API inventory](docs/API_COVERAGE.md)

The `demo` branch adds WinForms and WPF reference applications to the reusable OCCT C# bridge. `src/OcctNative`, `src/OcctNet`, `docs`, and `tests` stay synchronized with `main`; UI, scenarios, and publishing remain demo-only.

## Features

- WinForms and WPF OCCT viewers
- Point, rectangle, Ctrl-toggle multi-selection, and subshape selection
- Configurable selected and hover highlight colors
- Solid or gradient backgrounds and multi-light presets
- Curves, primitive solids, Boolean operations, features, transforms, and analysis
- Complex gear, multi-port manifold, and twisted-duct scenarios
- BRep vector text plus length, angle, radius, and diameter annotations
- STEP, IGES, BREP, and STL exchange
- English and Simplified Chinese UI

Complex scenarios use display batching and remove profiles, cutters, paths, and construction geometry after completion so only final results remain in the scene.

## Compatibility

- OCCT: exactly `7.9.0`
- .NET: `8.0`, Windows x64
- Bridge ABI: `1`
- API count: Native `517`, P/Invoke `517`
- Deploy `OcctNet.dll` and `OcctNative.dll` from the same build

## Build and run

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
.\run.ps1 winform
.\run.ps1 wpf
```

## Publish

Create the smaller framework-dependent WinForms package:

```powershell
.\publish.ps1 winform Release -OcctRoot "D:\tools\occt-vc144-64"
```

Publish both applications:

```powershell
.\publish.ps1 all Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

Create a self-contained package for machines without the .NET 8 Desktop Runtime:

```powershell
.\publish.ps1 all Release -SelfContained -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

`publish.ps1` copies the native dependency closure and required OCCT resources. Enable `-FullResources` or `-Diagnostics` only when needed.
