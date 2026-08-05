# OcctCSharpBridge Demo

OcctCSharpBridge wraps Open CASCADE Technology 7.9.0 through a native C++ DLL, a stable C ABI, and a type-safe .NET 8 API.

The `demo` branch contains WinForms and WPF reference applications. The reusable bridge remains under `src/OcctNative` and `src/OcctNet`.

## Demo features

- WinForms and WPF OCCT viewers
- Object and subshape selection
- Ctrl-click toggle selection: add an object, then Ctrl-click it again to remove it from the selection
- Rectangle selection
- Configurable selected and hover highlight colors
- Solid and gradient scene backgrounds
- Ambient, camera, sun, and fill lights with Neutral, Studio, Sunlight, and Flat presets
- Standard views, projection, fit, pan, zoom, and rotation
- Geometry creation, topology inspection, Boolean and feature operations
- STEP, IGES, BREP, and STL exchange
- OCAF, TNaming, and XDE wrapper examples
- English and Simplified Chinese UI

## Build

```powershell
.\build.ps1 Release
```

## Run

```powershell
.\run.ps1 winform
.\run.ps1 wpf
```

## Publish

The default package is lean, framework-dependent, and WinForms-first:

```powershell
.\publish.ps1
```

Use the publish script options for WPF, self-contained runtime, full OCCT resources, diagnostics, or ZIP output.

## API inventory

- [English API inventory](docs/API_COVERAGE.md)
- [中文接口清单](docs/API_COVERAGE.zh-CN.md)

## Requirements

- Windows x64
- .NET 8 SDK for source builds
- OCCT 7.9.0 for native builds
- Visual Studio C++ build tools and CMake for native compilation

See the repository license and the notices included in published packages before redistribution.
