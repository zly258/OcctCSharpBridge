# OcctCSharpBridge

[简体中文](README.zh-CN.md) · [English Docs](docs/en-US/README.md) · [中文文档](docs/zh-CN/README.md) · [Build/Test Guide](docs/en-US/08_Build-Test-and-Publish.md) · [Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo) · [Cross-platform Avalonia](https://github.com/zly258/OcctCSharpBridge/tree/avalonia)

OcctCSharpBridge is the reusable **Open CASCADE Technology 7.9.0 → .NET 10 / C# 14** bridge. `main` owns the formal Native Core, managed API, WinForms/WPF/Avalonia viewport hosts, tests, documentation and platform Binary SDK production.

Bridge 3 is **ABI 5 only**. ABI 4 exports, compatibility shims, legacy handles, compatibility tests, old consumer contracts and old Binary SDK payloads are not part of the current source tree.

> STEP/XDE boundary: XDE is used internally for STEP assembly/product structure, occurrence transforms and presentation metadata. OcctCSharpBridge does not expose OCAF/XDE as the consuming application's document or persistence architecture.

## Current source contract

| Item | Current source |
| --- | --- |
| Bridge | **3.0.0-preview.1** |
| Native ABI | **5 only** |
| API policy | **abi5-only** |
| OCCT | **7.9.0** |
| .NET SDK | **10.0.303** |
| Target Framework | **`net10.0` core / `net10.0-windows` desktop adapters** |
| C# / Native | **14.0 / C++17** |
| UI adapters | **WinForms / WPF / Avalonia** |
| Source platform contract | **Windows x64 / Linux x64** |

`bridge-contract.json` is the machine-readable source of truth. Native declarations, definitions and managed `LibraryImport` bindings are validated directly from current source by `tests/check-api-surface.ps1`; README/docs intentionally do not maintain hard-coded API counts or a generated API reference.

## Architecture

```text
Your CAD / BIM application
  Document · Feature Tree · Command/Tool · Undo/Redo · JSON
                 │
                 ▼
OcctNet.WinForms ─┐
OcctNet.Wpf      ─┼─> OcctNet -> ABI5 C API -> OcctNative -> OCCT 7.9.0
OcctNet.Avalonia ─┘
```

`OcctModelingSession` owns headless modeling/topology resources. `OcctEngine` owns AIS/viewer presentation and interactive scene state. UI adapters depend on `OcctNet` directly and do not reference each other.

Application documents, feature trees, commands/tools, undo/redo, snapping, grips and project persistence remain application responsibilities.

## Build and validation

Recommended full Windows validation:

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

Other common targets:

```powershell
.\build.ps1 validate Release
.\build.ps1 native Release
.\build.ps1 managed Release
.\build.ps1 test Release
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 clean
```

Linux x64:

```bash
./build.sh validate Release
./build.sh managed Release
./build.sh test Release
./build.sh all Release
./build.sh avalonia-smoke Release
./build.sh dist Release
```

See [Build, Test and Publish](docs/en-US/08_Build-Test-and-Publish.md) for every target, the six static contract checks, managed tests, Native Smoke, .NET SDK 10.0.303 diagnostics and publication rules.

The ABI5 contract checks keep Native declarations, definitions and managed `LibraryImport` bindings aligned, reject retired pre-ABI5 handles and compatibility artifacts, and validate tracked platform Binary SDK contracts when such payloads exist.

## Binary SDKs

Development branches must not retain ABI4 Binary SDK payloads. `build.ps1 dist` / `build.sh dist` produce platform packages from the current ABI5 source. Formal publishing must validate the package contract, manifest, source commit and hashes before a `dist/<rid>` payload is tracked on `main`.

## Usage

```csharp
using OcctNet;

using var model = new OcctModelingSession();
var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -5), OcctVector3d.UnitZ, 8, 20);
var cut = model.Cut(plate, hole);
model.ExportStep(cut.Shape, "plate.step");
```

## Branch responsibilities

- `main` — sole formal Bridge SDK source and Binary SDK producer.
- `main-dev` — ABI5 SDK development and validation before PR to `main`.
- `demo` / `demo-dev` — WinForms/WPF consumer applications; no private copy of Core/Native source.
- `avalonia` / `avalonia-dev` — Windows/Linux Avalonia consumer and packaging flow.

## License

OcctCSharpBridge is licensed under **GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0**. See [LICENSE](LICENSE), [LICENSE_LGPL_21.txt](LICENSE_LGPL_21.txt), [OcctCSharpBridge_LGPL_EXCEPTION.txt](OcctCSharpBridge_LGPL_EXCEPTION.txt), [COMMERCIAL.md](COMMERCIAL.md), and [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
