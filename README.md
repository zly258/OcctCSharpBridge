# OcctCSharpBridge

[简体中文](README.zh-CN.md) · [English Docs](docs/en-US/README.md) · [中文文档](docs/zh-CN/README.md) · [API Reference](docs/en-US/api/README.md) · [Demo Branch](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge is a Windows x64 bridge from **Open CASCADE Technology 7.9.0** to **.NET 10**. It provides strongly typed C# APIs for OCCT modeling, topology, geometry analysis, meshing, data exchange, AIS/viewer interaction, and three independent reusable WinForms/WPF/Avalonia viewport hosts.

`main` stays at the reusable Bridge boundary. Product-level Document, Feature Tree, Command/Tool, Undo/Redo, snapping, grips, persistence and OCAF/XDE do not belong in the Bridge.

## Current contract

| Item | Current value |
| --- | --- |
| Author | **zly258** |
| Bridge version | **2.6.0** |
| Native ABI | **4** |
| Native exports | **347** |
| Managed P/Invoke mappings | **347** |
| Public .NET types | **110** |
| Viewer / Modeling API | **213 / 134** |
| Open CASCADE Technology | **7.9.0** |
| .NET SDK | **10.0.302** |
| Target Framework | **`net10.0-windows`** |
| C# | **14.0** |
| Native Bridge | **C++17** |
| Avalonia | **12.1.0** |
| Platform | **Windows x64** |

`bridge-contract.json` is the machine-readable source of truth for version, platform and API counts.

## UI host architecture

```text
OcctNet.WinForms ─┐
OcctNet.Wpf      ─┼─> OcctNet -> OcctNative.dll -> OCCT
OcctNet.Avalonia ─┘
```

Each UI adapter depends directly on `OcctNet`; no UI adapter references another UI adapter. `OcctNet.Wpf` uses WPF `HwndHost` to own the OCCT render HWND directly and does not enable or reference Windows Forms.

## Build

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 test Release
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

Managed packages:

```powershell
.\build.ps1 pack Release
```

Complete bilingual Managed + Native API Reference:

```powershell
.\build.ps1 docs Release
```

## Validated Binary SDK

`dist/win-x64` is a tracked release payload, not ordinary build output. Produce it through the Release distribution target:

```powershell
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

The payload contains:

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

`bridge-manifest.json` records the Bridge/ABI/OCCT/.NET contract, source commit and SHA-256 hashes. OCCT `TK*.dll` and third-party runtime DLLs remain external to the Binary SDK payload.

## Publish a release to main and demo

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

The publishing workflow is one-directional:

```text
clean main worktree
→ generate bilingual complete API Reference
→ build Release Native + Managed SDK
→ create and validate dist/win-x64
→ commit/push main
→ create a temporary detached demo worktree
→ synchronize only dist/win-x64
→ validate contract/manifest/SHA-256
→ commit/push demo
```

Managed regression tests and Native Smoke remain explicit `build.ps1 test` / `build.ps1 smoke` targets and do not block Binary SDK publication.

## Usage

```csharp
using OcctNet;

using var model = new OcctModelingSession();
var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -5), OcctVector3d.UnitZ, 8, 20);
var cut = model.Cut(plate, hole);
model.ExportStep(cut.Shape, "plate.step");
```

Use `OcctModelingSession` for headless modeling/topology and `OcctEngine` for AIS/viewer interaction.

## Repository structure

```text
bridge-contract.json            Version/platform/API contract
src/OcctNative                  C++17 OCCT bridge and stable C ABI
src/OcctNet                     Core managed bridge
src/OcctNet.WinForms            Independent WinForms viewport host
src/OcctNet.Wpf                 Independent native WPF HwndHost viewport
src/OcctNet.Avalonia            Independent Avalonia 12.1.0 Windows-HWND host
tests                           Static contracts, managed regression, native smoke
tools/OcctApiDocsGenerator      Complete bilingual Managed + Native API generator
docs/zh-CN                      Chinese conceptual docs + API reference
docs/en-US                      English conceptual docs + API reference
dist/win-x64                    Tracked validated Binary SDK
build.ps1                       Validation/build/test/pack/docs/dist entry point
publish.ps1                     Release/API-doc/main→demo publishing entry point
```

## Branch boundary

`main` is the only Bridge source producer. `demo` is a Binary SDK consumer and does not mirror `src/OcctNative`, `src/OcctNet*`, or Bridge tests. Other applications should consume the validated Binary SDK instead of cloning and rebuilding Bridge source.

## Author

**zly258**  
zhangly1403@gmail.com

## License

OcctCSharpBridge is licensed under the [PolyForm Noncommercial License 1.0.0](LICENSE). Open CASCADE Technology and other third-party dependencies remain subject to their own licenses.
