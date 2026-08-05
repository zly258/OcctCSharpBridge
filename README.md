# OCCT 7.9.0 C# CAD Demo

[简体中文](README.zh-CN.md)

The `demo` branch is the complete example environment built on top of the reusable `main` wrapper. It exposes Open CASCADE Technology **7.9.0** through a **C++17 native DLL, stable C ABI, and .NET 8 P/Invoke**, with WinForms, WPF, shared CAD commands, headless modeling, OCAF/TNaming/XDE, and API coverage tooling.

## Architecture

```text
CadWinForms ─┐
             ├─ CadCommon ── OcctNet ── OcctNative ── OCCT 7.9.0
CadWpf ──────┘                  │
                                ├─ OcctEngine            Viewer/AIS
                                ├─ OcctModelingSession   Headless modeling
                                └─ OcafDocument          OCAF/XDE
```

| Project | Purpose |
|---|---|
| `src/OcctNative` | C++17 Viewer, modeling, OCAF/XDE, and stable C ABI |
| `src/OcctNet` | Type-safe .NET 8 API, P/Invoke, and viewport control |
| `src/CadCommon` | Shared commands, session, replay undo/redo, localization, and API scenarios |
| `src/CadWinForms` | Conventional WinForms CAD application |
| `src/CadWpf` | WPF CAD application hosting the OCCT viewport through `WindowsFormsHost` |
| `tests/OcctNet.Smoke` | Headless, OCAF/XDE, and persistence smoke coverage |

## API Center

Both desktop applications contain an **API Center** menu. It:

- discovers every public `OcctNet` type, constructor, property, field, event, and method through reflection;
- supports searching by area, type, method, signature, or prerequisite;
- classifies members as automated, interactive, file-dependent, environment-dependent, or catalog-only;
- updates automatically when public APIs are added to `main`;
- shares one `CadCommon.ApiDemoCatalog` implementation between WinForms and WPF.

Included executable scenarios cover:

1. public API catalog auditing;
2. Viewer, camera, projection, display precision, materials, and lighting;
3. existing CAD primitive, Boolean, loft, and annotation samples;
4. headless modeling, topology, mesh, analysis, healing, and algorithm reports;
5. headless Shape transfer into the Viewer;
6. temporary BREP round trips;
7. OCAF labels, attributes, variables, expressions, relations, transactions, and BinXCAF;
8. TNaming history and persistent selection;
9. XDE assemblies, components, colors, layers, and reusable materials.

“Every interface is covered” means every public member appears in the searchable catalog. Members requiring mouse input, selected topology, user files, or a specific document state expose that prerequisite instead of being called with meaningless default arguments.

## Requirements

- Windows x64
- Visual Studio 2022 with **Desktop development with C++** and **.NET desktop development**
- .NET 8 SDK
- CMake 3.21+
- **OCCT 7.9.0 VC++ x64**

The default OCCT root is:

```text
D:\tools\occt-vc144-64
```

Set `OCCT_ROOT` or pass `-OcctRoot` to override it.

## Build

```powershell
Set-ExecutionPolicy -Scope Process Bypass

# No OCCT SDK required
.\build.ps1 validate Release
.\build.ps1 managed Release

# OCCT 7.9.0 required
.\build.ps1 native Release
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 smoke Release
.\build.ps1 all Release

.\build.ps1 all Release -OcctRoot "D:\SDK\occt-vc144-64"
```

| Target | Result |
|---|---|
| `validate` | Compare C declarations, C++ definitions, and C# P/Invoke names |
| `managed` | Build `OcctNet` and `CadCommon` |
| `native` | Build `OcctNative.dll` |
| `winform` | Native + WinForms |
| `wpf` | Native + WPF |
| `smoke` | Native + compile and run the smoke test |
| `all` | Native, WinForms, WPF, and smoke-test compilation |

Run:

```powershell
.\run.ps1 winform Release
.\run.ps1 wpf Release
```

## CAD interaction

Both applications use a Model Explorer on the left, an OCCT viewport in the center, Properties/Command Line on the right, and command, selection, and world-coordinate status at the bottom.

| Input | Action |
|---|---|
| Left click | Select an object or subshape |
| Left drag | Rectangle selection |
| `Ctrl` + selection | Append to selection |
| Right drag | Orbit |
| Middle drag | Pan |
| Mouse wheel | Zoom |
| `Esc` | Clear selection |
| `Ctrl+Z` / `Ctrl+Y` | Replay-based undo/redo |

Selection filters include Object, Vertex, Edge, Wire, Face, Shell, and Solid. English is the default UI language; Simplified Chinese is available from the `Language` menu.

## Wrapper coverage

| Area | Main capabilities |
|---|---|
| Viewer/AIS | HWND Viewer, display, visibility, selection, subshapes, camera, projection, materials, lighting, text, and dimensions |
| Headless | Geometry, solids, Booleans, Splitter, features, healing, topology, distance, projection, rays, mesh, and pure-Shape exchange |
| OCAF/TDF | Documents, persistence, transactions, Undo/Redo, labels, scalars, arrays, references, variables, expressions, and relations |
| TNaming | Generated/Modify/Delete/Select, NamedShape history, and Selector workflows |
| XDE | Shapes, assemblies, components, instance locations, colors, layers, materials, validation properties, and length units |
| Exchange | STEP/IGES/BREP/STL plus metadata-preserving STEPCAF/IGESCAF |

Detailed boundaries:

- `docs/API_COVERAGE.md`
- `docs/OCAF_COVERAGE.md`
- `docs/OCAF_EXTENDED_API.md`

## Validation and runtime boundary

GitHub Actions validates 491 C ABI entry points across declarations, C++ definitions, and P/Invoke, then compiles `OcctNet`, `CadCommon`, WinForms, WPF, and the smoke project.

Native linking, Viewer rendering, and actual BinXCAF/STEPCAF/IGESCAF execution still require the Windows target machine with the OCCT 7.9.0 SDK:

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

Application crash logs are written to:

```text
%LOCALAPPDATA%\OcctCSharpBridge\Logs
```

## License

The project uses the [PolyForm Noncommercial License 1.0.0](LICENSE). OCCT and third-party components remain subject to their respective licenses.
