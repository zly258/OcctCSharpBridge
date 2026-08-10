# OcctCSharpBridge

[简体中文](README.zh-CN.md) · [Documentation (中文)](docs/00_文档索引.md) · [API Coverage](docs/03_API覆盖与设计约定.md) · [Demo Branch](https://github.com/zly258/OcctCSharpBridge/tree/demo)

## Description

OcctCSharpBridge is a Windows x64 bridge from **Open CASCADE Technology 7.9.0** to **.NET 10**. It provides a strongly typed C# API for OCCT modeling, topology, geometry analysis, meshing, data exchange, AIS/viewer interaction, and reusable WinForms/WPF/Avalonia viewport hosts.

The repository deliberately stays at the OCCT bridge boundary. `main` does not implement an application Document model, feature tree, Command/Tool framework, Undo/Redo, snapping, grips, project persistence, or OCAF/XDE. Product-level CAD behavior belongs in an application layer such as the `demo` branch.

Current contract:

- Bridge `2.6.0`, Native ABI `3`
- OCCT `7.9.0`
- .NET SDK `10.0.302`
- Target framework `net10.0-windows`
- C# `14.0`
- Windows x64

The managed API has two primary façades:

- `OcctEngine` — AIS/viewer, registered display objects, selection, camera, appearance, annotations, and interactive viewport operations.
- `OcctModelingSession` — headless construction, topology, algorithms, analysis, meshing, history, and STEP/IGES/BREP/STL exchange.

P0–P3 capabilities include full inertia properties, structured Edge/Edge intersection results, versioned topology references, and bulk-only collection transfer for high-cardinality modeling data.

## Installation

### 1. Prerequisites

Install:

- Windows x64
- .NET SDK `10.0.302`
- Visual Studio 2022 / MSVC toolchain
- CMake `3.21+`
- Open CASCADE Technology `7.9.0`, VC14 x64 layout

The default OCCT location used by the repository is:

```text
D:\tools\occt-vc144-64
```

A different location can be passed with `-OcctRoot` or through `OCCT_ROOT`.

### 2. Build managed packages

```powershell
.\build.ps1 pack Release
```

Packages are written to:

```text
artifacts\packages
```

The repository produces:

```text
OcctNet
OcctNet.WinForms
OcctNet.Wpf
OcctNet.Avalonia
```

To consume the locally built core package:

```powershell
dotnet add package OcctNet --version 2.6.0 --source .\artifacts\packages
```

Add only the UI host required by the application, for example:

```powershell
dotnet add package OcctNet.Wpf --version 2.6.0 --source .\artifacts\packages
```

Managed packages intentionally do not bundle `OcctNative.dll` or OCCT `TK*.dll`. Deploy the matching native bridge and OCCT runtime with the application.

### 3. Build from source with OCCT

```powershell
.\build.ps1 all Release
```

For another OCCT installation:

```powershell
.\build.ps1 all Release -OcctRoot "E:\SDK\occt-7.9.0"
```

## Usage Example

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

var resolved = model.ResolveTopologyReference(
    cut.Shape,
    reference);
```

A topology reference is a geometric/topological fingerprint. Its runtime index is only a hint and is not treated as persistent identity.

## Project Structure

```text
bridge-contract.json     Version, platform and API contract
src/OcctNative           C++17 OCCT bridge and C ABI
src/OcctNet              Core .NET wrapper
src/OcctNet.WinForms     WinForms viewport host
src/OcctNet.Wpf          WPF viewport host
src/OcctNet.Avalonia     Avalonia Windows-HWND viewport host
tests                    Contract, managed regression and native smoke tests
docs                     Numbered technical documentation
build.ps1                Local validation/build/pack/smoke entry point
```

`OcctNet.Avalonia` currently hosts the native viewer through a Windows child HWND, so it is a Windows x64 adapter rather than a cross-platform OCCT viewer backend.

## Local Validation

Managed/static validation does not require an OCCT SDK:

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
```

The authoritative native validation uses the real local OCCT SDK:

```powershell
.\build.ps1 smoke Release
```

The native build uses `/W4 /WX`, exact OCCT 7.9.0 headers/libraries, and the API contract in `bridge-contract.json`.

## Contributing

Contributions should keep the bridge focused and non-redundant:

1. Create a focused branch for one responsibility.
2. Keep public APIs strongly typed and owner-aware; do not add legacy aliases or compatibility wrappers.
3. Prefer bulk C ABI transfer for collections instead of repeated `Count + At` calls.
4. Keep OCCT-specific implementation inside the bridge and application-specific Document/Command/Tool behavior outside `main`.
5. Do not introduce OCAF/XDE into the reusable bridge.
6. Run `build.ps1 validate`, `build.ps1 managed`, and, when OCCT is available, `build.ps1 smoke` before submitting changes.
7. Keep reusable source changes synchronized with the `demo` branch.

## License

OcctCSharpBridge is licensed under the [PolyForm Noncommercial License 1.0.0](LICENSE).

Open CASCADE Technology and other third-party dependencies remain subject to their own licenses.
