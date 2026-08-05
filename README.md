# OCCT 7.9.0 C# WinForms and WPF Demo

[简体中文](README.zh-CN.md) · [Documentation](docs/README.md) · [Reusable SDK on `main`](https://github.com/zly258/OcctCSharpBridge/tree/main)

The `demo` branch is the complete desktop demonstration environment built on the reusable `main` branch. It includes WinForms and WPF CAD applications, a shared command/session layer, Viewer and headless modeling examples, OCAF/TNaming/XDE scenarios, a searchable public API catalog, CI coverage, and a portable Windows x64 publishing script.

## Project structure

```text
CadWinForms ─┐
             ├─ CadCommon ── OcctNet ── OcctNative ── OCCT 7.9.0
CadWpf ──────┘                  │
                                ├─ OcctEngine            Viewer/AIS
                                ├─ OcctModelingSession   Headless modeling
                                └─ OcafDocument          OCAF/TNaming/XDE
```

| Project | Responsibility |
|---|---|
| `src/OcctNative` | C++17 bridge, stable C ABI, Viewer, modeling and OCAF/XDE implementation |
| `src/OcctNet` | Type-safe .NET 8 API, runtime discovery, P/Invoke and shared viewport control |
| `src/CadCommon` | Commands, sessions, undo/redo, localization, examples and API scenarios shared by both UIs |
| `src/CadWinForms` | Classic WinForms CAD application |
| `src/CadWpf` | WPF CAD application using `WindowsFormsHost` for the shared OCCT viewport |
| `tests/OcctNet.ApiCatalog` | Ensures every exported public member appears in the API catalog |
| `tests/OcctNet.Smoke` | Headless, OCAF/XDE, persistence and transfer smoke coverage |

## Main desktop features

Both applications provide:

- Model Explorer and object selection state;
- central OCCT Viewer with standard views and camera controls;
- command panels and property editing;
- object/subshape selection filters;
- point selection and rectangle selection;
- OCCT `AIS_RubberBand` selection overlay without Win32 XOR flicker;
- display mode, visibility, color, transparency, material and line-width operations;
- geometry, solids, transforms, Boolean operations, features, analysis and annotations;
- command replay-based undo/redo in the shared demo layer;
- English and Simplified Chinese UI;
- shared exception logging under `%LOCALAPPDATA%\OcctCSharpBridge\Logs`.

## Camera and display behavior

Creating a Shape displays it and redraws the scene, but does **not** automatically call `Fit` or `FitAll`. Normal command execution therefore keeps the current camera and does not jump after every object creation.

Use explicit view actions when required:

```csharp
engine.Fit(shape);
engine.FitAll();
engine.WindowFit(x1, y1, x2, y2);
```

Multi-object examples use a nestable display batch:

```csharp
using (engine.BeginDisplayBatch())
{
    var box = engine.MakeBox(100, 80, 60);
    var cylinder = engine.MakeCylinder(20, 80, 140, 0, 0);
    engine.SetColor(box, Color.SteelBlue);
    engine.SetColor(cylinder, Color.OrangeRed);

    // Optional explicit camera operation for a gallery/example command.
    engine.FitAll();
}
```

All objects remain separate registry entries for independent selection, properties, deletion and Model Explorer display. The batch only suppresses intermediate OpenGL redraws.

## API Center

WinForms and WPF both expose **API Center → API Catalog and Scenarios**.

The API catalog reflects every exported public `OcctNet` type and member:

- types and enums;
- constructors;
- properties and fields;
- events;
- methods and complete signatures.

Members are classified as:

- automated scenario;
- interactive dependency;
- file dependency;
- environment dependency;
- catalog-only.

This avoids creating hundreds of low-value buttons while still ensuring that newly added public APIs become discoverable automatically.

### Executable scenarios

The shared `CadCommon.ApiDemoCatalog` includes:

1. public API catalog verification;
2. Viewer, camera, projection, precision, material and lighting;
3. CAD primitives, Boolean operations, loft and annotations;
4. headless modeling, topology, mesh, ray analysis, healing and reports;
5. headless Shape transfer to Viewer;
6. BREP roundtrip;
7. OCAF labels, attributes, variables, expressions, relations, transactions and BinXCAF;
8. TNaming evolution and persistent selection;
9. XDE assemblies, components, locations, colors, layers and materials.

Running multiple CAD examples from API Center uses one outer display batch, so nested example batches collapse into one final redraw.

## Interaction

| Input | Behavior |
|---|---|
| Left click | Select object or active subshape type |
| Left drag | Rectangle selection |
| `Ctrl` + selection | Append selection |
| Right drag | Rotate view |
| Middle drag | Pan |
| Mouse wheel | Zoom |
| `Esc` | Clear selection |
| `Ctrl+Z` / `Ctrl+Y` | Undo/redo demo command history |

Selection modes include Object, Vertex, Edge, Wire, Face, Shell and Solid.

## Development environment

- Windows 10/11 x64
- Visual Studio 2022 with Desktop development with C++ and .NET desktop development
- .NET 8 SDK
- CMake 3.21+
- **exactly OCCT 7.9.0 VC++ x64**

Default OCCT root:

```text
D:\tools\occt-vc144-64
```

Override it with `OCCT_ROOT` or `-OcctRoot`.

## Build and run

```powershell
Set-ExecutionPolicy -Scope Process Bypass

# Static native/C#/PInvoke consistency.
.\build.ps1 validate Release

# Managed wrapper and shared Demo layer.
.\build.ps1 managed Release

# OCCT SDK required from here.
.\build.ps1 native Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 winform Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 wpf Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

Run from the development tree:

```powershell
.\run.ps1 winform Release -OcctRoot "D:\tools\occt-vc144-64"
.\run.ps1 wpf Release -OcctRoot "D:\tools\occt-vc144-64"
```

## Portable publishing

`publish.ps1` creates a distributable Windows x64 package for users who should not install or configure the OCCT SDK, CMake, Visual Studio, or the .NET runtime.

Publish both applications:

```powershell
.\publish.ps1 all Release `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -OutputDirectory ".\artifacts\publish" `
  -Zip
```

Publish one application:

```powershell
.\publish.ps1 winform Release -OcctRoot "D:\tools\occt-vc144-64" -Zip
.\publish.ps1 wpf Release -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

The default publish is self-contained and packages:

- the .NET 8 runtime;
- WinForms and/or WPF application files;
- `OcctNative.dll`;
- OCCT runtime DLLs;
- detected OCCT third-party DLLs;
- available Visual C++ runtime DLLs;
- OCCT resource directories for exchange, persistence, shaders, messages, units and textures;
- relative-path launchers;
- SHA-256/version manifest;
- project, OCCT and detected third-party license files.

Generated launchers configure `PATH`, `OCCT_BRIDGE_NATIVE_DIR`, `OCCT_ROOT` and `CASROOT` relative to the extracted package. The recipient runs `Start-WinForms.cmd` or `Start-WPF.cmd` without configuring environment variables.

See [Portable Demo publishing](docs/PUBLISHING_DEMO.md) and [Deployment](docs/DEPLOYMENT.md).

## Package layout

```text
OcctCSharpBridge-Demo-win-x64
├─ apps
│  ├─ winform
│  └─ wpf
├─ runtime
│  ├─ OcctNative.dll
│  ├─ TK*.dll
│  ├─ third-party DLLs
│  └─ Visual C++ runtime DLLs
├─ occt\src\...
├─ licenses
├─ Start-WinForms.cmd
├─ Start-WPF.cmd
├─ runtime-manifest.txt
└─ README.txt
```

Keep the package directory intact; do not send only the EXE.

## Capability overview

| Area | Main coverage |
|---|---|
| Viewer/AIS | camera, projection, view cube, triedron, display, selection, materials, lighting, text and dimensions |
| Geometry | curves, wires, faces, standard solids and compounds |
| Features | Boolean, splitter, extrusion, revolution, sweep, loft, fillet, chamfer, offset, thick solid and transforms |
| Analysis | bounds, mass properties, topology, distance, projection, ray intersection, validation and reports |
| Headless | background modeling, healing, meshing, face triangulation and pure Shape exchange |
| OCAF/TDF | documents, labels, attributes, transactions, undo/redo, variables, expressions and relations |
| TNaming | evolution history and persistent selector workflows |
| XDE | shapes, assemblies, components, locations, names, colors, layers, materials and properties |
| Exchange | BREP/STL/STEP/IGES plus STEPCAF/IGESCAF metadata-preserving workflows |

## Continuous integration

The branch validates:

- C ABI declarations, C++ definitions and C# P/Invoke parity;
- `OcctNet` and `CadCommon` compilation;
- complete public API catalog coverage;
- WinForms and WPF compilation;
- smoke-test compilation;
- `publish.ps1` PowerShell syntax and required project inputs.

GitHub runners do not contain the repository-specific OCCT 7.9.0 SDK. Native linking, real Viewer rendering and complete portable package generation must still be validated on the target Windows development machine.

## Documentation

- [Demo documentation index](docs/README.md)
- [Getting started](docs/GETTING_STARTED.md)
- [Viewer, selection and display updates](docs/VIEWER_AND_DISPLAY.md)
- [Portable Demo publishing](docs/PUBLISHING_DEMO.md)
- [Deployment and runtime layout](docs/DEPLOYMENT.md)
- [API coverage](docs/API_COVERAGE.md)
- [OCAF/XDE coverage](docs/OCAF_COVERAGE.md)
- [Extended OCAF API](docs/OCAF_EXTENDED_API.md)

## License

The repository is provided under the [PolyForm Noncommercial License 1.0.0](LICENSE). OCCT, Microsoft runtime files and third-party components remain subject to their own licenses and redistribution terms. Review the generated `licenses` directory before distributing a package.
