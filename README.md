# OcctCSharpBridge · OcctScript

[简体中文](README.zh-CN.md) · [main branch](https://github.com/zly258/OcctCSharpBridge/tree/main) · [demo branch](https://github.com/zly258/OcctCSharpBridge/tree/demo)

The `script` branch provides a usable first preview of **OcctScript**, a lightweight parametric CAD script editor built on the reusable `OcctCSharpBridge` wrapper.

It keeps the low-level OCCT wrapper synchronized with `main`, while adding a JSON document model, parameters and expressions, dependency-driven command history, undo/redo, a WPF editor, bilingual UI, examples, and script smoke tests.

OcctScript intentionally does **not** use OCAF/XDE. Documents, history and persistence belong to the application layer and are stored as readable JSON.

## What is included

- Windows x64, .NET 8 and Open CASCADE Technology 7.9.0.
- WPF editor with embedded OCCT viewport.
- English UI by default; switch to Simplified Chinese at runtime.
- Versioned `.json` / `.ocsproj` document format.
- Named parameters and expressions without `${...}` syntax.
- Expression operators `+ - * / ^`, constants `PI` / `E`, and common math functions.
- Metadata-driven command registry and property editor.
- Command references, dependency sorting and circular-reference detection.
- Topology-aware validation between commands.
- Full rebuild from document history and document-level undo/redo.
- Ready-to-open JSON samples under `samples/Scripts`.
- `Help → About OcctScript`.
- Script smoke tests using real `OcctModelingSession` geometry.

## First-preview command set

**Curves and wires:** `Vertex`, `Line`, `Polyline`, `Circle`, `Arc`, `Ellipse`, `RegularPolygon`, `Bezier`, `BSpline`, `Rectangle`, `Wire`

**Surfaces:** `Face`, `PlaneFace`

**Primitive and topology solids:** `Box`, `Cylinder`, `Cone`, `Sphere`, `Torus`, `Wedge`, `Compound`, `Sew`, `SolidFromShell`

**Features:** `Extrude`, `Revolve`, `Sweep`, `Loft`, `Fillet`, `Chamfer`, `Offset`, `Shell`

**Boolean:** `Fuse`, `Cut`, `Common`, `Section`

**Explicit transforms:** `Move`, `RotateShape`, `ScaleShape`, `Mirror`

Every command also has a generic post-build transform with X/Y/Z translation, X/Y/Z rotation and uniform scale.

## Typical modeling chains

```text
Line / Arc / Bezier / BSpline
              ↓
             Wire
              ↓
             Face
              ↓
           Extrude
              ↓
          Solid Body
              ↓
 Fillet / Chamfer / Shell
              ↓
       Boolean / Transform
```

Other supported flows include edge → extruded face, profile → revolve, profile + spine → sweep, and multiple sections → loft.

## Repository layout

```text
src/OcctNative              C++17 OCCT bridge and stable C ABI
src/OcctNet                 UI-independent managed wrapper
src/OcctNet.WinForms        WinForms HWND viewport host
src/OcctNet.Wpf             WPF viewport host

src/OcctScript.Domain       JSON document and command metadata
src/OcctScript.Expressions  expression parser/evaluator
src/OcctScript.Serialization JSON persistence
src/OcctScript.Application  validation, parameters and undo/redo
src/OcctScript.Geometry     dependency graph and OCCT builders
src/OcctScript.Editor       WPF parametric editor

samples/Scripts             ready-to-open JSON examples
tests/OcctScript.Smoke      script/modeling smoke scenarios
docs/script                 concise OcctScript documentation
```

## Requirements

- Windows x64.
- Visual Studio 2022 / MSVC.
- .NET 8 SDK.
- CMake 3.16 or newer.
- Open CASCADE Technology 7.9.0 built for VC14 x64.
- `OCCT_ROOT` pointing to the OCCT installation, or pass `-OcctRoot`.

## Build the usable OcctScript preview

```powershell
# Validates the bridge, builds native/managed/UI projects,
# builds OcctScript.Editor, then builds and runs OcctScript.Smoke.
.\build.ps1 script Release -OcctRoot "D:\tools\occt-vc144-64"
```

Editor output:

```text
src\OcctScript.Editor\bin\x64\Release\net8.0-windows\
```

`build.ps1 script` copies the matching `OcctNative.dll` into the editor and script-smoke output directories. Keep `OCCT_ROOT` configured when running so OCCT runtime DLLs can be resolved.

Build only the reusable managed wrapper with `./build.ps1 managed Release`.

## Run the editor

```powershell
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"
.\src\OcctScript.Editor\bin\x64\Release\net8.0-windows\OcctScript.Editor.exe
```

The editor starts in English. Use **Language → 中文** to switch to Simplified Chinese.

## Sample JSON

Open any file in [`samples/Scripts`](samples/Scripts/README.md): `01-Curves.json`, `02-Extrude.json`, `03-Revolve.json`, `04-Sweep.json`, `05-Loft.json`, `06-Booleans.json`, `07-Primitives-Transforms.json`, `08-Edge-Features.json`.

A minimal document is versioned and readable:

```json
{
  "format": "OcctScript.Document",
  "version": 1,
  "name": "Example",
  "lengthUnit": "mm",
  "angleUnit": "deg",
  "parameters": [],
  "commands": [],
  "outputCommandIds": []
}
```

See [JSON format](docs/script/JSON_FORMAT.md) and [command reference](docs/script/COMMANDS.md).

## Design boundaries of this first preview

Included now: the basic line / surface / solid / feature workflow needed to create and reproduce ordinary parametric models.

Intentionally excluded: steel/profile-specific generators; complex transition solids and specialized engineering sections; linear/circular/irregular arrays; a geometric-constraint sketch solver; OCAF/XDE document storage; assembly/product-structure authoring.

These can be added later without changing the core JSON command model.

## Bridge compatibility

Bridge version: 2.5.0; ABI: 2.

The synchronized wrapper targets OCCT `7.9.0`, .NET `8`, Windows x64, managed wrapper `2.5.0`, native ABI `2`.

`OcctEngine` owns the interactive AIS/viewer session. `OcctModelingSession` owns headless geometry/topology/algorithm objects. OcctScript builds geometry in `OcctModelingSession` and copies resulting shapes into the WPF viewport.

## Documentation

- [OcctScript overview](docs/script/README.md)
- [Command reference](docs/script/COMMANDS.md)
- [JSON format](docs/script/JSON_FORMAT.md)
- [OcctCSharpBridge API inventory](docs/API_COVERAGE.md)
- [中文接口清单](docs/API_COVERAGE.zh-CN.md)

## License

The project is provided under the [PolyForm Noncommercial License 1.0.0](LICENSE). OCCT and third-party components remain subject to their own licenses.