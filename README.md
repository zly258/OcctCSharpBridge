# OcctCSharpBridge

[简体中文](README.zh-CN.md) · [English Docs](docs/en-US/README.md) · [中文文档](docs/zh-CN/README.md) · [API Reference](docs/en-US/api/README.md) · [Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo) · [Cross-platform Avalonia](https://github.com/zly258/OcctCSharpBridge/tree/avalonia) · [Website](https://github.com/zly258/OcctCSharpBridge/tree/website)

OcctCSharpBridge `main` is the formal SDK source from **Open CASCADE Technology 7.9.0** to **.NET 10 / C# 14**. It exposes strongly typed C# APIs for headless modeling, topology and geometry analysis, meshing, engineering file exchange, AIS/viewer interaction, first-class point presentation, and independent WinForms/WPF/Avalonia viewport hosts.

The [`demo`](https://github.com/zly258/OcctCSharpBridge/tree/demo) and [`avalonia`](https://github.com/zly258/OcctCSharpBridge/tree/avalonia) branches are consumer examples and packaging flows. Formal `OcctNative`, `OcctNet`, and UI-host implementations are owned by `main`.

`main` builds the shared Windows/Linux Native Core; the current tracked binary distribution remains Windows x64. Product-level document models, feature trees, commands/tools, undo/redo, snapping, grips and project persistence remain application responsibilities.

> STEP/XDE boundary: XDE is used internally for STEP assembly/product structure, occurrence transforms and presentation metadata. OcctCSharpBridge does **not** expose OCAF/XDE as the consuming application's document/persistence architecture. Assembly-aware consumers use the managed `OcctAssemblyDocument` snapshot instead.

## Current source contract

| Item | Current source |
| --- | --- |
| Bridge | **3.0.0-preview.1** |
| Native ABI | **5 current / 4 compatible** |
| Native exports / P/Invoke | **431 / 431** |
| Public .NET types | **145** |
| Viewer / Modeling API | **292 / 139** |
| OCCT | **7.9.0** |
| .NET SDK | **10.0.302** |
| Target Framework | **`net10.0` core / `net10.0-windows` desktop** |
| C# / Native | **14.0 / C++17** |
| UI adapters | **WinForms / WPF / Avalonia** |
| Platform | **Windows x64** |

`bridge-contract.json` is the machine-readable source of truth for the **main source** contract.

### Published Binary SDK status

The authoritative Windows Binary SDK is the tracked `main/dist/win-x64` payload. Read [`dist/win-x64/bridge-contract.json`](dist/win-x64/bridge-contract.json) for its actual Bridge/ABI/API contract and [`dist/win-x64/bridge-manifest.json`](dist/win-x64/bridge-manifest.json) for the exact source commit and file hashes.

`publish.ps1` replaces those files only after a validated Windows/MSVC + OCCT 7.9 Release build. Documentation deliberately does not hard-code a second “published version” value that can become stale immediately after a release.

## Highlights in 2.7 source

- first-class `OcctAssemblyDocument` / `OcctAssemblyNode` STEP-XDE occurrence model;
- stable assembly item IDs, Assembly/Instance/Part roles, local/global transforms, visibility, surface RGBA, curve colors and subshape styles;
- valid multi-solid STEP Parts remain one Part instead of being flattened into artificial `Part_###` objects;
- non-geometric STEP edits can round-trip through the pristine imported XDE document while geometry is unchanged;
- first-class `OcctPoint` / `OcctPointMarker` backed by `AIS_Point`;
- WPF uses a no-redraw native surface resize path and coalesces presentation at render priority instead of redrawing from `WM_PAINT`.

## Windows demo previews

<p align="center">
  <img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/winform-demo-en.png" alt="WinForms demo" width="49%" />
  <img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/wpf-demo-en.png" alt="WPF demo" width="49%" />
</p>

The full WinForms/WPF demo sources are maintained on the [`demo`](https://github.com/zly258/OcctCSharpBridge/tree/demo) branch. Cross-platform Avalonia remains on the dedicated [`avalonia`](https://github.com/zly258/OcctCSharpBridge/tree/avalonia) branch.

## Architecture

```text
Your CAD / BIM application
  Document · Feature Tree · Command/Tool · Undo/Redo · JSON
                 │
                 ▼
OcctNet.WinForms ─┐
OcctNet.Wpf      ─┴─> OcctNet -> stable C ABI -> OcctNative -> OCCT 7.9.0
```

`OcctModelingSession` owns headless modeling/topology. `OcctEngine` owns AIS/viewer presentation and interactive scene state. Windows UI adapters depend on `OcctNet` directly and do not reference each other.

Cross-platform Avalonia is developed separately on the `avalonia` branch so the Windows-only source and release contract on `main` stay compact and deterministic.

## Build

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 test Release
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 docs Release
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

## Publish the tracked Windows Binary SDK

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

The Windows Binary SDK release flow is one-directional:

```text
clean, up-to-date main
→ generate bilingual API reference
→ build/validate Release Binary SDK
→ commit/push main dist/win-x64
→ demo users run demo/sync.ps1 locally
```

`demo/dist` is intentionally ignored by Git. The demo branch is a consumer, not a second Binary SDK repository.

## Usage

```csharp
using OcctNet;

using var model = new OcctModelingSession();
var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -5), OcctVector3d.UnitZ, 8, 20);
var cut = model.Cut(plate, hole);
model.ExportStep(cut.Shape, "plate.step");
```

Assembly-aware STEP import in the 2.7 source:

```csharp
using var engine = new OcctEngine();
OcctAssemblyDocument assembly = engine.ImportStepDocument("assembly.step");
foreach (OcctAssemblyNode root in assembly.Roots)
{
    // Traverse root.Children.
}
```

## Branches

- `main` — Windows x64 Bridge source, WinForms/WPF adapters, tests, documentation and tracked `dist/win-x64` Binary SDK producer.
- `demo` — Windows WinForms/WPF demo applications consuming a **local ignored** `dist/win-x64` copied from the currently published `main` SDK.
- `avalonia` — cross-platform Avalonia Bridge variant for Windows and Linux, with platform-specific native viewer backends hidden behind one Avalonia API.
- `website` — static project website/GitHub Pages source.

## License

OcctCSharpBridge is licensed under **GNU LGPL version 2.1 + OcctCSharpBridge Exception 1.0**. Commercial and proprietary applications may use the Bridge through .NET assembly references, dynamic linking, P/Invoke, or equivalent runtime linking without requiring the application itself to adopt the GNU LGPL solely because of that use.

The LGPL obligations continue to apply to **OcctCSharpBridge itself** and to modified/derivative versions of the Bridge that are distributed. See [LICENSE](LICENSE), [LICENSE_LGPL_21.txt](LICENSE_LGPL_21.txt), [OcctCSharpBridge_LGPL_EXCEPTION.txt](OcctCSharpBridge_LGPL_EXCEPTION.txt), [COMMERCIAL.md](COMMERCIAL.md), and [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

Open CASCADE Technology and other third-party dependencies remain subject to their own licenses. OCCT keeps its own GNU LGPL 2.1 + Open CASCADE Exception terms.
