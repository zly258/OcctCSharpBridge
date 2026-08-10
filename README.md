# OcctCSharpBridge Demo

[简体中文](README.zh-CN.md) · [Main SDK](https://github.com/zly258/OcctCSharpBridge) · [API Coverage](docs/API_COVERAGE.md)

## Description

The `demo` branch contains reference CAD applications for **OcctCSharpBridge 2.6.0**, Open CASCADE Technology **7.9.0**, and .NET SDK **10.0.302** on Windows x64. It demonstrates how the reusable bridge can be integrated into WinForms, WPF, and Avalonia applications without moving product-level CAD architecture into `main`.

The branch includes three desktop applications plus shared demo code for command dispatch, history, localization, object/property panels, runtime diagnostics, file exchange, and common CAD interaction examples. The reusable bridge remains the source of geometry, topology, modeling, selection, viewer, mesh, P0 inertia, P1 structured intersection, and P2 topology-reference capabilities.

### Preview

<p align="center"><img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/winform-demo-en.png" alt="WinForms demo" width="88%"></p>
<p align="center"><img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/wpf-demo-en.png" alt="WPF demo" width="88%"></p>
<p align="center"><img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/avalonia-demo-en.png" alt="Avalonia demo" width="88%"></p>

## Installation

### Requirements

- Windows x64
- .NET SDK `10.0.302`
- Visual Studio 2022 / MSVC toolchain
- CMake `3.21+`
- OCCT `7.9.0`, VC14 x64 layout

The default OCCT root is:

```text
D:\tools\occt-vc144-64
```

Use another installation with `-OcctRoot` or the `OCCT_ROOT` environment variable.

### Build all demos

```powershell
.\build.ps1 all Release
```

Build one UI host/application:

```powershell
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 avalonia Release
```

Static and managed validation can run without the OCCT SDK:

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
```

The authoritative native validation uses the real OCCT runtime:

```powershell
.\build.ps1 smoke Release
```

## Usage Example

Run a built application with the branch runner:

```powershell
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

The three demos share the same bridge API and CAD demo behavior while using framework-specific hosts. Typical operations include primitive creation, Boolean/feature modeling, selection and subshape selection, object properties, annotations, STEP/IGES/BREP/STL exchange, topology analysis, meshing, and viewport interaction.

The underlying `OcctModelingSession` also exposes the P0–P2 APIs directly:

```csharp
using OcctNet;

using var model = new OcctModelingSession();

var box = model.MakeBox(100, 80, 20);
var inertia = model.GetVolumeInertiaProperties(box);

var first = model.MakeLine(new OcctPoint3d(0, 0, 0), new OcctPoint3d(100, 0, 0));
var second = model.MakeLine(new OcctPoint3d(50, -20, 0), new OcctPoint3d(50, 20, 0));
var intersections = model.IntersectEdges(first, second);

var faces = model.GetSubshapes(box, OcctShapeType.Face);
var reference = model.CreateTopologyReference(box, faces[0]);
var resolved = model.ResolveTopologyReference(box, reference);
```

## Project Structure

```text
src/OcctNative           Shared native OCCT bridge
src/OcctNet              Shared .NET bridge, non-packable on demo
src/OcctNet.WinForms     WinForms viewport host
src/OcctNet.Wpf          WPF viewport host
src/OcctNet.Avalonia     Avalonia Windows-HWND viewport host
src/OcctDemo.Common      Shared demo behavior
src/OcctDemo.WinForms    CAD-Winform
src/OcctDemo.Wpf         CAD-WPF
src/OcctDemo.Avalonia    CAD-Avalonia
assets/previews           Branch-specific UI previews
tests                     Shared bridge contracts plus demo-specific checks
```

The demo wrapper projects are intentionally non-packable. NuGet SDK packaging belongs to `main`.

## Native Startup Troubleshooting

If a demo fails with `DllNotFoundException` or Win32 error 126, check the application-local runtime first. The diagnostic report explicitly shows entries such as:

```text
OcctNative.dll [missing]
TKernel.dll [missing]
```

For published packages, also inspect `native-dependencies.txt`. Application crash/startup logs are written under:

```text
%LOCALAPPDATA%\OcctCSharpBridge\Logs
```

The Avalonia host uses a Windows child HWND and is therefore a Windows x64 host, not a cross-platform OCCT viewer backend.

## Contributing

1. Keep reusable OCCT/native/managed changes aligned with `main`; keep demo-only UI/application behavior on `demo`.
2. Do not add compatibility aliases or duplicate old APIs; this repository is maintained as a clean new bridge.
3. Keep demo wrapper and application projects non-packable.
4. Prefer shared `OcctDemo.Common` behavior over copying business logic across WinForms, WPF, and Avalonia.
5. Run `build.ps1 validate` and `build.ps1 managed`; with OCCT installed, also run `build.ps1 smoke` and the relevant demo build/run target.
6. Preserve the branch-specific preview images and runtime-diagnostic guidance when changing the desktop UI.

## License

OcctCSharpBridge is licensed under the [PolyForm Noncommercial License 1.0.0](LICENSE).

Open CASCADE Technology and other third-party dependencies remain subject to their own licenses.
