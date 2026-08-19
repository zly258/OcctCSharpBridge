# OcctCSharpBridge

[简体中文](README.zh-CN.md) · [English Docs](docs/en-US/README.md) · [中文文档](docs/zh-CN/README.md) · [Third-party SDK Guide](docs/en-US/09_Third-Party-SDK-Consumption.md) · [第三方 SDK 接入](docs/zh-CN/09_第三方项目消费SDK.md) · [Unified Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge is a reusable **Open CASCADE Technology 7.9.0 → .NET 8-10 / C# 14** bridge for Windows x64 and Linux x64. `main` owns the formal Native Core, managed API, WinForms/WPF/Avalonia adapters, tests, documentation, and platform SDK production.

Bridge 3 is **ABI 5 only**. ABI 4 exports, compatibility shims, legacy handles, compatibility tests, old consumer contracts, and old Binary SDK payloads are not part of the current source tree.

> STEP/XDE boundary: XDE may be used internally for STEP assembly/product structure, occurrence transforms, and presentation metadata. OcctCSharpBridge does not expose OCAF/XDE as the consuming application's document or persistence architecture.

## Current contract

| Item | Contract |
| --- | --- |
| Bridge | **3.0.0-preview.1** |
| Native ABI | **5 only** |
| API policy | **abi5-only** |
| OCCT | **7.9.0** |
| Build SDK | **stable .NET 10 SDK, baseline `10.0.100`, `latestFeature` roll-forward** |
| Binary SDK TFM | **`net8.0` Core/Avalonia · `net8.0-windows` WinForms/WPF** |
| Supported consumer TFMs | **.NET 8 / .NET 9 / .NET 10** |
| C# / Native | **14.0 / C++17** |
| UI adapters | **WinForms / WPF / Avalonia** |
| Platforms | **Windows x64 / Linux x64** |

`bridge-contract.json` is the machine-readable source of truth. Native declarations/definitions and managed `LibraryImport` bindings are checked from source; documentation intentionally does not maintain generated API counts.

The build toolchain and consumer runtime baseline are intentionally different: the source uses a stable .NET 10 SDK for C# 14, while the distributed managed assemblies target .NET 8 so one Binary SDK can be referenced by .NET 8, .NET 9, and .NET 10 applications.

## Architecture

```text
Your CAD / BIM application
  Document · Feature Tree · Commands · Undo/Redo · Persistence
                 │
                 ▼
OcctNet.WinForms ─┐
OcctNet.Wpf      ─┼─> OcctNet -> ABI5 C API -> OcctNative -> OCCT 7.9.0
OcctNet.Avalonia ─┘
```

`OcctModelingSession` owns headless modeling/topology resources. `OcctEngine` owns AIS/viewer presentation and interactive scene state. Application documents, feature trees, command systems, undo/redo, snapping, grips, and project persistence remain application responsibilities.

## SDK production has two levels

### Fast consumer artifact build

Use `dist` when a local consumer such as the Demo, an integration test project, or a controlled third-party build only needs a fresh SDK from a known source revision:

```powershell
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

```bash
./build.sh dist Release
```

`dist` performs the source/contract checks required to create a reproducible Binary SDK, then builds Native + Managed and writes `dist/<rid>`. It intentionally **does not run** the consumer matrix, ManagedTests, Core Native Smoke, or UI viewport smoke tests.

### Full Bridge validation and publication

Use the complete gate when validating or publishing Bridge itself:

```powershell
.\build.ps1 sdk Release -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

```bash
./build.sh all Release
./publish.sh
```

The Windows `sdk`/publish path retains the .NET 8/9/10 consumer compilation matrix, ManagedTests, Core Native Smoke, and WinForms/WPF/Avalonia viewport smoke tests. Linux formal publication retains the headless validation gate; graphical Avalonia smoke remains an explicit display-dependent test.

Consumer synchronization must not repeat this full Bridge QA gate on every SDK refresh. The Demo therefore uses the `dist` fast path on a cache miss and validates `sourceCommit` plus SHA-256 before accepting the result.

## Binary SDK and Portable SDK

The minimal Binary SDK is intended for compile-time references and controlled automation:

```text
Windows: dist/win-x64/
  OcctNative.dll
  OcctNet.dll
  OcctNet.WinForms.dll
  OcctNet.Wpf.dll
  OcctNet.Avalonia.dll
  bridge-contract.json
  bridge-manifest.json

Linux: dist/linux-x64/
  libOcctNative.so
  OcctNet.dll
  OcctNet.Avalonia.dll
  bridge-contract.json
  bridge-manifest.json
```

`dist/` is fully generated and ignored by Git.

For application deployment and external distribution, use the **Portable SDK** produced by `publish.ps1` / `publish.sh` or another reviewed artifact created from the same source commit. The Portable SDK adds `runtime/`, `occt/resources/`, licenses/notices, and a recursive package manifest. It does not bundle the consuming application's .NET runtime.

Third-party projects should start with [Third-party SDK Consumption](docs/en-US/09_Third-Party-SDK-Consumption.md) / [第三方项目消费 SDK](docs/zh-CN/09_第三方项目消费SDK.md).

## Minimal use

```csharp
using OcctNet;

OcctRuntime.Configure();

using var model = new OcctModelingSession();
var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -5), OcctVector3d.UnitZ, 8, 20);
var cut = model.Cut(plate, hole);
model.ExportStep(cut.Shape, "plate.step");
```

Call `OcctRuntime.Configure()` before the first `OcctEngine` or `OcctModelingSession` when deploying with the Portable SDK layout.

## Branch responsibilities

- `main` — formal Bridge source and release SDK producer.
- `main-dev` — Bridge development and candidate validation.
- `demo` — formal Binary/Portable SDK consumer.
- `demo-dev` — development consumer; normally follows `main-dev`.
- `website` — bilingual project website.

Generated Binary SDKs and Portable SDKs are artifacts, not source-controlled payloads. Formal external distribution should use reviewed `main` artifacts rather than `main-dev` development output.
