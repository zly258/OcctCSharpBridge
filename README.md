# OcctCSharpBridge Demo

[Main SDK branch](https://github.com/zly258/OcctCSharpBridge/tree/main) · [简体中文](README.zh-CN.md) · [API inventory](docs/API_COVERAGE.md)

The `demo` branch adds complete WinForms, WPF, and Avalonia CAD reference applications on top of the reusable bridge maintained with `main`. Shared native/.NET wrapper code and contract metadata are continuously compared with `main`; application UI, demo scenarios, run scripts, publishing, and package validation remain demo-only.

OCAF/XDE is intentionally not used as the application document layer. Documents, JSON persistence, command history, undo/redo, tools, and domain entities belong to the consuming application.

## Toolchain and contract

- Windows x64
- Open CASCADE Technology **7.9.0**, VC14 x64 layout
- .NET SDK **8.0.423**, pinned by `global.json`
- C# 12.0
- CMake 3.21+
- Avalonia `12.1.0`
- Bridge version `2.5.0`, native ABI `2`

`bridge-contract.json` is shared with `main` and is the authoritative source for Bridge/ABI/OCCT/.NET/API metadata. `global.json` fixes the SDK, while `Directory.Build.props` fixes the C# language/compiler policy. The `Wrapper Branch Sync` workflow verifies that these files and reusable wrapper sources stay synchronized with `main`.

## Layering

- `OcctNet`: UI-independent viewer, modeling, topology, geometry, analysis, mesh, healing, and exchange APIs.
- `OcctNet.WinForms`: reusable native-HWND `OcctViewportControl`.
- `OcctNet.Wpf`: reusable `OcctWpfViewport` with WPF event/DPI/resize integration.
- `OcctNet.Avalonia`: Windows x64 Avalonia `NativeControlHost` using its own child HWND.
- `CadCommon`: shared application/session/command layer used by the desktop demos.
- `CadWinForms`, `CadWpf`, `CadAvalonia`: runnable reference applications.

Avalonia remains a native Windows HWND host today; this repository does not claim Linux/macOS OCCT viewer support.

## Preview

<table>
  <tr>
    <th>WinForms</th>
    <th>WPF</th>
  </tr>
  <tr>
    <td><img src="assets/previews/winform-demo-en.webp" alt="OCCT CAD WinForms English demo" width="100%"></td>
    <td><img src="assets/previews/wpf-demo-en.webp" alt="OCCT CAD WPF English demo" width="100%"></td>
  </tr>
</table>

Avalonia uses the same `CadSession` and `CadCommandCatalog` application layer as WPF and exposes the same main CAD workflow: model creation, selection, model explorer, properties, undo/redo, file exchange, annotations, analysis, view/display controls, samples, shortcuts, and bilingual UI.

## First-time setup

```powershell
git clone https://github.com/zly258/OcctCSharpBridge.git
cd OcctCSharpBridge
git switch demo
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"
```

The expected OCCT layout contains `inc`, `win64\vc14\lib`, `win64\vc14\bin`, and optionally `3rdparty-vc14-64`.

## Build and validation

`build.ps1` is the single build entry point:

| Target | Purpose | OCCT SDK |
| --- | --- | --- |
| `validate` | Contract/source/package checks only | No |
| `managed` | Core wrapper + WinForms/WPF/Avalonia hosts + `CadCommon` | No |
| `ci` | Same managed build used by GitHub Actions, including all 3 demos and Smoke project compilation | No |
| `native` | Build `OcctNative.dll` | Yes |
| `winform` / `wpf` / `avalonia` | Build the selected runnable demo | Yes |
| `smoke` | Build native bridge and run real OCCT modeling Smoke scenarios | Yes |
| `all` | Build native bridge, all demos, and Smoke project | Yes |

```powershell
# Fast source/API contract check
.\build.ps1 validate Release

# Reproduce the normal GitHub Actions build locally
.\build.ps1 ci Release

# Complete native/demo build
.\build.ps1 all Release

# Strongest native runtime check
.\build.ps1 smoke Release
```

GitHub Actions calls `build.ps1 ci Release` directly, so local pre-push validation and hosted CI use the same managed build path instead of duplicating project-by-project commands.

## Run

`run.ps1` starts an already-built application; it does not silently rebuild it.

```powershell
.\run.ps1 winform
.\run.ps1 wpf
.\run.ps1 avalonia
```

If a demo is stale, build the relevant target first.

## Publish

`publish.ps1` currently produces deployment-complete WinForms and WPF packages. Avalonia is fully covered by build/run/CI but has not yet been added to the formal publishing target.

```powershell
.\publish.ps1 all Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 winform Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 wpf Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

Published packages include the selected application, matching managed wrapper/host assemblies, `OcctNative.dll`, recursively resolved OCCT/third-party runtime dependencies, required OCCT resources, `package-contract.json`, and `native-dependencies.txt`. Use `-FrameworkDependent` only when the target machine already has the matching .NET 8 Desktop Runtime.

## Main capabilities exercised by the demos

- Point, rectangle, directional crossing, multi-selection, and subshape selection
- Camera/view state, Z-up views, fitting, screen/world projection, view cube and triedron
- Shaded/wireframe/shaded-with-edges display and configurable precision/material/lighting
- Stable application tags, transforms, visibility, color, transparency, and batch operations
- Primitive and feature modeling, Boolean operations, sweep/loft, topology and geometry queries
- Analytic/differential geometry, mass properties, distance/ray/projection and mesh access
- BRep text and length/angle/radius/diameter annotations
- STEP, IGES, BREP, and STL exchange
- English and Simplified Chinese desktop UI

## Troubleshooting

- `OCCT_ROOT is not configured`: set `$env:OCCT_ROOT` or pass `-OcctRoot` to a native target.
- Native DLL load failure: rebuild with the correct OCCT SDK and ensure matching runtime dependencies are present.
- Avalonia startup issue: inspect `src\CadAvalonia\bin\x64\<Configuration>\net8.0-windows\CAD-Avalonia.log`.
- After API/host/menu changes, run `build.ps1 validate`; before pushing, prefer `build.ps1 ci`; use `build.ps1 smoke` when a real OCCT SDK is available.

## License

The project uses the [PolyForm Noncommercial License 1.0.0](LICENSE). Open CASCADE Technology and third-party components remain subject to their own licenses.
