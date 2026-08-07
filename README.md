# OcctCSharpBridge Demo

[Main SDK branch](https://github.com/zly258/OcctCSharpBridge/tree/main) · [简体中文](README.zh-CN.md) · [API inventory](docs/API_COVERAGE.md)

The `demo` branch adds complete WinForms, WPF, and Avalonia CAD reference applications on top of the reusable bridge maintained with `main`. Shared native/.NET wrapper code and contract metadata are continuously compared with `main`; application UI, demo scenarios, run scripts, publishing, and package validation remain demo-only.

OCAF/XDE is intentionally not used as the application document layer. Documents, JSON persistence, command history, undo/redo, tools, and domain entities belong to the consuming application.

## Toolchain and contract

- Windows x64
- Open CASCADE Technology **7.9.0**, VC14 x64 layout
- .NET SDK **10.0.302** for this branch, pinned by `global.json`
- Target framework remains **`net8.0-windows`**
- C# 12.0
- CMake 3.21+
- Avalonia `12.1.0`
- Bridge version `2.5.0`, native ABI `2`

`bridge-contract.json` is shared with `main` and is the authoritative source for Bridge/ABI/OCCT/.NET/API metadata. It records both the core SDK used by `main` and the newer demo SDK required by Avalonia 12 analyzers. `global.json` is intentionally branch-specific: `main` stays on .NET SDK 8.0.423, while `demo` uses 10.0.302. The target framework and C# language level remain .NET 8 and C# 12.

## Layering

- `OcctNet`: UI-independent viewer, modeling, topology, geometry, analysis, mesh, healing, and exchange APIs.
- `OcctNet.WinForms`: reusable native-HWND `OcctViewportControl`.
- `OcctNet.Wpf`: reusable `OcctWpfViewport` with WPF event/DPI/resize integration.
- `OcctNet.Avalonia`: Windows x64 Avalonia `NativeControlHost` using its own child HWND.
- `CadCommon`: shared application/session/command layer used by the desktop demos.
- `CadWinForms`, `CadWpf`, `CadAvalonia`: runnable reference applications.

Avalonia remains a native Windows HWND host today; this repository does not claim Linux/macOS OCCT Viewer support.

## Preview

<table>
  <tr><th>WinForms</th><th>WPF</th></tr>
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
| `ci` | Hosted-CI managed build, all 3 demos and Smoke project compilation | No |
| `native` | Build `OcctNative.dll` | Yes |
| `winform` / `wpf` / `avalonia` | Build the selected runnable demo | Yes |
| `smoke` | Build native bridge and run real OCCT modeling scenarios | Yes |
| `all` | Build native bridge, all demos, and Smoke project | Yes |

```powershell
.\build.ps1 validate Release
.\build.ps1 ci Release
.\build.ps1 all Release
.\build.ps1 smoke Release
```

Normal GitHub Actions calls `build.ps1 ci Release`. `smoke` is intentionally stronger and requires a real OCCT SDK because it actually loads the native bridge and executes OCCT modeling operations.

## Run

`run.ps1` starts an already-built application; it does not silently rebuild it.

```powershell
.\run.ps1 winform
.\run.ps1 wpf
.\run.ps1 avalonia
```

## Publish

`publish.ps1` formally supports **WinForms, WPF, and Avalonia**. `all` publishes all three applications.

```powershell
.\publish.ps1 all Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 winform Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 wpf Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 avalonia Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

The publisher resolves the complete PE dependency closure of `OcctNative.dll`, OCCT modules, third-party DLLs, and Visual C++ runtime components, including `vcomp140.dll` when required. The resolved native DLL set is copied **beside every application executable** instead of relying only on a sibling `runtime` directory. The runtime resolver therefore prefers the application directory first.

Before a package is accepted, `publish.ps1` performs two checks: a static `dumpbin` closure check and a fresh-process `LoadLibraryExW` probe with a restricted DLL search path. A package that would produce a clean-machine `Win32 126` native-load failure is rejected during publishing. The top-level `runtime` directory remains as the canonical dependency/diagnostic copy, while `apps\winform`, `apps\wpf`, and `apps\avalonia` are directly runnable.

Published packages also include required OCCT resources, `package-contract.json`, `native-dependencies.txt`, and available license notices. Use `-FrameworkDependent` only when the target machine already has the .NET 8 Desktop Runtime.

## Tests

The PowerShell tests are contract/static regression checks. Obsolete duplicate checks that were no longer called by `build.ps1` have been removed. `tests/OcctNet.Smoke` is retained deliberately: it is the functional native integration test and is the only test layer that actually loads OCCT and performs real modeling operations.

Recommended cadence:

- `build.ps1 validate`: every API/source change.
- `build.ps1 ci`: before every push.
- `build.ps1 smoke`: after native/modeling/runtime changes, and before a release when an OCCT SDK is available.
- `publish.ps1 ...`: before distributing a package; it includes the dedicated portability/load probe.

## Main capabilities exercised by the demos

- Point, rectangle, directional crossing, multi-selection, and subshape selection
- Camera/view state, Z-up views, fitting, screen/world projection, ViewCube and triedron
- Shaded/wireframe/shaded-with-edges display and configurable precision/material/lighting
- Stable application tags, transforms, visibility, color, transparency, and batch operations
- Primitive and feature modeling, Boolean operations, sweep/loft, topology and geometry queries
- Analytic/differential geometry, mass properties, distance/ray/projection and mesh access
- BRep text and length/angle/radius/diameter annotations
- STEP, IGES, BREP, and STL exchange
- English and Simplified Chinese desktop UI

## Troubleshooting

- `OCCT_ROOT is not configured`: set `$env:OCCT_ROOT` or pass `-OcctRoot` to a native target.
- `Unable to load OcctNative.dll ... Win32 126`: do not redistribute an old package. Republish with the current `publish.ps1`; the application directory must contain `OcctNative.dll` and its native dependency closure. Publishing now fails if the restricted native-load probe cannot load it.
- Avalonia analyzer/compiler mismatch: use the branch-pinned SDK from `global.json`; do not downgrade the target framework or disable analyzers.
- Avalonia startup issue: inspect `src\CadAvalonia\bin\x64\<Configuration>\net8.0-windows\CAD-Avalonia.log`.

## License

The project uses the [PolyForm Noncommercial License 1.0.0](LICENSE). Open CASCADE Technology and third-party components remain subject to their own licenses.

## Contact

Liaoyuan Zhang · [zhangly1403@gmail.com](mailto:zhangly1403@gmail.com)
