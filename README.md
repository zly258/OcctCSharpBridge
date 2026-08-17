# OcctCSharpBridge

[简体中文](README.zh-CN.md) · [English Docs](docs/en-US/README.md) · [中文文档](docs/zh-CN/README.md) · [Build/Test Guide](docs/en-US/08_Build-Test-and-Publish.md) · [Unified Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge is the reusable **Open CASCADE Technology 7.9.0 → .NET 8-10 / C# 14** bridge. `main` owns the formal Native Core, managed API, WinForms/WPF/Avalonia adapters, tests, documentation and platform Binary SDK production.

Bridge 3 is **ABI 5 only**. ABI 4 exports, compatibility shims, legacy handles, compatibility tests, old consumer contracts and old Binary SDK payloads are not part of the current source tree.

> STEP/XDE boundary: XDE is used internally for STEP assembly/product structure, occurrence transforms and presentation metadata. OcctCSharpBridge does not expose OCAF/XDE as the consuming application's document or persistence architecture.

## Current source contract

| Item | Current source |
| --- | --- |
| Bridge | **3.0.0-preview.1** |
| Native ABI | **5 only** |
| API policy | **abi5-only** |
| OCCT | **7.9.0** |
| Build SDK | **stable .NET 10 SDK, baseline `10.0.100`, `latestFeature` roll-forward** |
| Binary SDK Target Framework | **`net8.0` core/Avalonia · `net8.0-windows` WinForms/WPF** |
| Supported consumer TFMs | **.NET 8 / .NET 9 / .NET 10** |
| C# / Native | **14.0 / C++17** |
| UI adapters | **WinForms / WPF / Avalonia** |
| Source platforms | **Windows x64 / Linux x64** |

`bridge-contract.json` is the machine-readable source of truth. Native declarations, definitions and managed `LibraryImport` bindings are validated directly from current source by `tests/check-api-surface.ps1`; README/docs intentionally do not maintain hard-coded API counts or a generated API reference.

The repository intentionally separates the **build SDK** from the **consumer runtime baseline**. A stable .NET 10 SDK is used to compile C# 14 source, while the distributed managed assemblies target .NET 8 so the same Binary SDK can be consumed by .NET 8, .NET 9 and .NET 10 applications without maintaining three duplicate DLL sets. The Windows gate also compiles dedicated Core/Avalonia and WinForms/WPF consumer projects for every supported TFM; `tests/check-consumer-matrix.ps1` keeps those project TFMs identical to the lists declared by `bridge-contract.json`.

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

## Viewport host contract

WinForms, WPF and Avalonia now share one platform-neutral host/input lifecycle instead of exposing framework-specific interaction requirements to applications:

- `OcctViewportInteractionFeatures` enables hover detection, point/rectangle selection, rotate, pan and zoom independently;
- `PreviewPointerInput / PointerInput` and `PreviewKeyInput / KeyInput` normalize Windows and Linux input and support preview cancellation through `Handled`;
- `OcctViewportHostState`, `HostStateChanged`, `Faulted`, `EngineGeneration`, `EngineDisposing` and `EngineRecreated` define native-host recreation safely;
- `OcctViewportInitializationOptions`, `RenderReady` and `FirstFrameRendered` allow background/view/projection/Triedron/ViewCube configuration before the first presented frame;
- `HoverHitChanged` provides owner/subshape identity changes without application-side repeated detection queries;
- `NativeHandleChanged` reports HWND/XID replacement for advanced hosting/diagnostics; ordinary application interaction should not depend on the native handle;
- existing `BeginDisplayBatch()` remains the single batched refresh API; no duplicate `BeginUpdate`/`DeferRefresh` surface is introduced;
- `ProjectPointToEdge` and `ProjectPointToFace` provide nearest point plus reusable edge parameter or face UV for snapping/work-plane style application features.

See [Viewer, Selection and Interaction](docs/en-US/05_Viewer-Selection-and-Interaction.md) for event ordering and usage boundaries.

## Build and validation

Windows full validation without producing `dist`:

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

`all` includes the .NET 8/9/10 consumer compilation matrix, managed regression tests, Core Native Smoke and WinForms/WPF/Avalonia Viewport Host Smoke. The focused consumer and viewport targets are:

```powershell
.\build.ps1 consumer Release
.\build.ps1 viewport-smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

For a **validated Windows Binary SDK**, use the Release gate:

```powershell
.\build.ps1 sdk Release -OcctRoot "D:\tools\occt-vc144-64"
```

`sdk` runs the full Windows gate and only then writes `dist/win-x64` from those already validated Native/Managed outputs. `dist` remains available as a lower-level Release packaging target, but it does not run the consumer matrix, regression tests or smoke tests and is not the preferred release/consumer-sync entry point.

Linux x64:

```bash
./build.sh validate Release
./build.sh managed Release
./build.sh test Release
./build.sh all Release
./build.sh avalonia-smoke Release
./build.sh dist Release
```

The repository accepts any stable .NET 10 SDK selected from the `10.0.100` baseline by `latestFeature` roll-forward. For example, `10.0.302` is valid; an exact patch/feature-band match is not required.

See [Build, Test and Publish](docs/en-US/08_Build-Test-and-Publish.md) for target details, static contract checks, consumer compatibility compilation, managed tests, Native Smoke and publication rules.

## Binary SDK policy

`dist/win-x64` and `dist/linux-x64` are generated Release artifacts, not source-controlled SDK payloads. Source branches do **not** commit Binary SDK files. Each package records its source commit and SHA-256 hashes in `bridge-manifest.json` and is validated before local consumption or external distribution. The manifest also records the actual resolved build SDK separately from the rolling SDK baseline.

The unified `demo` branch consumes these generated SDKs by `sourceCommit` and manifest hash. Windows source synchronization invokes the validated `sdk` Release gate rather than the lower-level `dist` target, and the Demo copies only the exact manifest-controlled Binary SDK payload. Formal binary distribution can use reviewed GitHub Release assets or another controlled artifact channel; no GitHub Actions pipeline is required.

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
- `main-dev` — Bridge SDK development and validation before PR to `main`.
- `demo` / `demo-dev` — the single Binary SDK consumer: Windows x64 has WinForms, WPF and Avalonia; Linux x64 has Avalonia only.
- `website` — bilingual project website.
- historical backup branches, when present, remain outside normal development and are left unchanged.
