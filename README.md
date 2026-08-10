# OcctCSharpBridge

[简体中文](README.zh-CN.md) · [Documentation (中文)](docs/00_文档索引.md) · [API Coverage](docs/03_API覆盖与设计约定.md) · [Demo Branch](https://github.com/zly258/OcctCSharpBridge/tree/demo)

## Description

OcctCSharpBridge is a Windows x64 bridge from **Open CASCADE Technology 7.9.0** to **.NET 10**. It provides a strongly typed C# API for OCCT modeling, topology, geometry analysis, meshing, data exchange, AIS/viewer interaction, and reusable WinForms/WPF/Avalonia viewport hosts.

`main` deliberately stays at the reusable bridge boundary. It does not implement an application Document model, feature tree, Command/Tool framework, Undo/Redo, snapping, grips, project persistence, or OCAF/XDE. Product-level CAD behavior belongs in an application layer such as the `demo` branch.

Current contract:

- Bridge `2.6.0`, Native ABI `4`
- OCCT `7.9.0`
- .NET SDK `10.0.302`
- Target framework `net10.0-windows`
- C# `14.0`
- Windows x64

The managed API has two primary façades:

- `OcctEngine` — AIS/viewer, registered display objects, selection, camera, appearance, annotations, and interactive viewport operations.
- `OcctModelingSession` — headless construction, topology, algorithms, analysis, meshing, history, and STEP/IGES/BREP/STL exchange.

## Prerequisites

- Windows x64
- .NET SDK `10.0.302`
- Visual Studio 2022 / MSVC toolchain
- CMake `3.21+`
- Open CASCADE Technology `7.9.0`, VC14 x64 layout

Default OCCT location:

```text
D:\tools\occt-vc144-64
```

A different location can be passed through `-OcctRoot` or `OCCT_ROOT`.

## Build managed packages

```powershell
.\build.ps1 pack Release
```

Packages are written to `artifacts\packages`:

```text
OcctNet
OcctNet.WinForms
OcctNet.Wpf
OcctNet.Avalonia
```

Managed packages intentionally do not bundle `OcctNative.dll` or OCCT `TK*.dll`. Deploy the matching native bridge and OCCT runtime with the application.

## Build the complete bridge

```powershell
.\build.ps1 all Release
```

For another OCCT installation:

```powershell
.\build.ps1 all Release -OcctRoot "E:\SDK\occt-7.9.0"
```

## Produce the tracked Binary SDK

`dist/win-x64` is not ordinary build output. It is the validated Binary SDK consumed directly by `demo`, OCStation, and other applications. The directory is intentionally tracked by Git and is exempt from the repository-wide `*.dll` ignore rule.

```powershell
.\dist.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

`dist.ps1` runs the Release build, managed tests, and native smoke validation before refreshing:

```text
dist/win-x64/
├─ OcctNative.dll
├─ OcctNet.dll
├─ OcctNet.WinForms.dll
├─ OcctNet.Wpf.dll
├─ OcctNet.Avalonia.dll
├─ bridge-contract.json
└─ bridge-manifest.json
```

The manifest records the Bridge/ABI/OCCT/.NET contract, source commit, and SHA-256 hash of every distributed file so managed and native binaries cannot be mixed accidentally.

OCCT `TK*.dll` and third-party runtime DLLs are intentionally not committed to `dist`; consumers resolve them through `OCCT_ROOT`, `CASROOT`, or explicit runtime configuration.

## Usage

### Headless modeling

```csharp
using OcctNet;

using var model = new OcctModelingSession();

var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(
    new OcctPoint3d(50, 40, -5),
    OcctVector3d.UnitZ,
    8,
    20);

var cut = model.Cut(plate, hole);
var inertia = model.GetVolumeInertiaProperties(cut.Shape);
var inspection = model.InspectShape(cut.Shape);
model.ExportStep(cut.Shape, "plate.step");
```

### Structured Edge/Edge intersection

```csharp
var first = model.MakeLine(
    new OcctPoint3d(0, 0, 0),
    new OcctPoint3d(100, 0, 0));
var second = model.MakeLine(
    new OcctPoint3d(50, -20, 0),
    new OcctPoint3d(50, 20, 0));

var intersections = model.IntersectEdges(first, second);
```

### Topology reference

```csharp
var faces = model.GetSubshapes(cut.Shape, OcctShapeType.Face);
var reference = model.CreateTopologyReference(cut.Shape, faces[0]);
var resolved = model.ResolveTopologyReference(cut.Shape, reference);
```

A topology reference is a geometric/topological fingerprint. Its runtime subshape index is only a hint and is not persistent identity.

## Project Structure

```text
bridge-contract.json     Version, platform and API contract
src/OcctNative           C++17 OCCT bridge and C ABI
src/OcctNet              Core .NET wrapper
src/OcctNet.WinForms     WinForms viewport host
src/OcctNet.Wpf          WPF viewport host
src/OcctNet.Avalonia     Avalonia Windows-HWND viewport host
tests                    Static contracts, managed regression and native smoke
docs                     Authoritative numbered bridge documentation
build.ps1                Local validation/build/test/pack/smoke entry point
dist.ps1                 Full-validation Binary SDK publisher
dist/win-x64             Tracked validated DLL/contract/manifest payload
```

`OcctNet.Avalonia` currently hosts the native viewer through a Windows child HWND, so it is a Windows x64 adapter rather than a cross-platform OCCT viewer backend.

## Local Validation

Without an OCCT SDK:

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 test Release
```

Build native and managed bridge code:

```powershell
.\build.ps1 all Release
```

Run the real native gate:

```powershell
.\build.ps1 smoke Release
```

The repository does not use GitHub Actions as a substitute for local validation. Native validation uses `/W4 /WX`, the real OCCT 7.9.0 headers/libraries, `bridge-contract.json`, and Native/PInvoke parity checks.

## main / demo boundary

`main` is the Bridge source producer. `demo` is a real Binary SDK consumer and no longer mirrors `src/OcctNative`, `src/OcctNet*`, or Bridge tests.

```text
main Bridge change
→ local build/test/smoke
→ dist.ps1
→ commit main/dist/win-x64
→ demo/sync-dist.ps1
→ build demo applications
```

If demo code no longer matches the current Bridge, update the demo caller instead of restoring legacy aliases, wrappers, or compatibility layers.

## Contributing

1. Keep public APIs strongly typed and owner-aware; do not add legacy aliases or compatibility wrappers.
2. Do not recreate deleted aggregate headers, wrappers, or experimental APIs as compatibility shells. Update callers to the current dependency instead.
3. Prefer bulk C ABI transfer for collections instead of repeated `Count + At` boundary calls.
4. Keep OCCT implementation inside the bridge and application-specific Document/Command/Tool behavior outside `main`.
5. Do not introduce OCAF/XDE into the reusable bridge.
6. Before committing, run `build.ps1 validate`, `build.ps1 managed`, and `build.ps1 test`; for native changes also run `build.ps1 all` and `build.ps1 smoke`.
7. Update `dist/win-x64` only through `dist.ps1` after Release build, managed tests, and native smoke all pass.
8. `demo` synchronizes the Binary SDK, not Bridge source; GitHub Actions are not used to overwrite branches.
9. Bridge technical documentation is maintained only under `main/docs`; `demo` does not duplicate the SDK documentation set.

## License

OcctCSharpBridge is licensed under the [PolyForm Noncommercial License 1.0.0](LICENSE).

Open CASCADE Technology and other third-party dependencies remain subject to their own licenses.
