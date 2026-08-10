# OcctCSharpBridge Demo

[简体中文](README.zh-CN.md) · [Main SDK](https://github.com/zly258/OcctCSharpBridge) · [Demo Maintenance](docs/README.md) · [Main Technical Docs](https://github.com/zly258/OcctCSharpBridge/tree/main/docs)

## Description

The `demo` branch contains Windows x64 reference applications for **OcctCSharpBridge 2.6.0**, Open CASCADE Technology **7.9.0**, and .NET SDK **10.0.302**. It demonstrates how the reusable bridge can be integrated into WinForms, WPF, and Avalonia applications without moving product-level CAD architecture into `main`.

The branch contains three desktop applications plus shared demo code for command dispatch, history, localization, object/property panels, runtime diagnostics, file exchange, and common CAD interaction. Geometry, topology, modeling, selection, viewer, mesh, inertia, structured intersection, and topology-reference capabilities come from the shared bridge.

Bridge technical documentation is maintained only under `main/docs`. The demo branch no longer duplicates API Coverage, B-Spline, Topology, Runtime, or other SDK documentation in parallel English/Chinese files.

### Preview

<p align="center"><img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/winform-demo-en.png" alt="WinForms demo" width="88%"></p>
<p align="center"><img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/wpf-demo-en.png" alt="WPF demo" width="88%"></p>
<p align="center"><img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/avalonia-demo-en.png" alt="Avalonia demo" width="88%"></p>

## Requirements

- Windows x64
- .NET SDK `10.0.302`
- Visual Studio 2022 / MSVC toolchain
- CMake `3.21+`
- OCCT `7.9.0`, VC14 x64 layout

Default OCCT root:

```text
D:\tools\occt-vc144-64
```

Use another installation with `-OcctRoot` or `OCCT_ROOT`.

## Build and Run

Basic local validation:

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
```

Build all demos:

```powershell
.\build.ps1 all Release
```

Build one application:

```powershell
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 avalonia Release
```

Run the real native gate:

```powershell
.\build.ps1 smoke Release
```

Run a built application:

```powershell
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

The repository does not use GitHub Actions as a substitute for local compilation, runtime validation, or branch synchronization.

## Usage Example

The shared `OcctModelingSession` API is available directly:

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
src/OcctDemo.Common      Shared demo application behavior
src/OcctDemo.WinForms    WinForms demo
src/OcctDemo.Wpf         WPF demo
src/OcctDemo.Avalonia    Avalonia demo
assets/previews           Branch-specific UI previews
docs/README.md            Demo-specific maintenance notes
tests                     Shared bridge contracts plus demo checks
run.ps1                   Local application runner
publish.ps1               Publish entry point
```

Demo wrapper and application projects remain non-packable. NuGet SDK packaging belongs to `main`.

## Synchronization with main

Reusable bridge source is synchronized manually from `main` after local validation. Shared source is synchronized selectively; demo-specific `build.ps1`, project files, application source, README/docs, run/publish scripts, and preview assets are not overwritten wholesale. See [Demo Maintenance](docs/README.md) for the exact scope.

If demo code no longer matches the current bridge, update the demo caller. Do not reintroduce legacy aliases, compatibility wrappers, or deleted aggregate internal headers.

## Native Startup Troubleshooting

For `DllNotFoundException` or Win32 error 126, check the application-local runtime first:

```text
OcctNative.dll
TKernel.dll
other OCCT TK*.dll
```

Published packages also contain `native-dependencies.txt`. Startup/crash logs are written under:

```text
%LOCALAPPDATA%\OcctCSharpBridge\Logs
```

The Avalonia host uses a Windows child HWND and is therefore still a Windows x64 host.

## Contributing

1. Keep reusable OCCT/native/managed source aligned with `main`; keep demo-only application behavior on `demo`.
2. Do not add compatibility aliases, duplicate old APIs, or compatibility aggregate headers.
3. Keep demo wrapper and application projects non-packable.
4. Prefer `OcctDemo.Common` for shared demo behavior instead of copying business logic across UI frameworks.
5. Before committing, run local `build.ps1 validate` and `build.ps1 managed`; with OCCT installed also run `build.ps1 all`, `build.ps1 smoke`, and the relevant demo build/run target.
6. Maintain bridge technical documentation only under `main/docs`; keep demo docs application-specific.

## License

OcctCSharpBridge is licensed under the [PolyForm Noncommercial License 1.0.0](LICENSE).

Open CASCADE Technology and other third-party dependencies remain subject to their own licenses.
