# OcctCSharpBridge

[简体中文](README.zh-CN.md) · [Documentation](docs/INDEX.md) · [Desktop demos](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge is a Windows x64 bridge from **Open CASCADE Technology 7.9.0** to **.NET 8**. The `main` branch contains the reusable C++ bridge, strict C ABI, type-safe managed wrapper, optional WinForms/WPF viewport hosts, contract checks, Native Smoke scenarios, and managed SDK packaging. Complete CAD applications are maintained on the `demo` branch.

Bridge **2.6.0 / ABI 3** is a cleanup and expansion release: compatibility aliases and public raw-ID construction were removed; naming, ownership, and deployment contracts were normalized; and the wrapper now includes structured selected/detected AIS identity, batched topology/Face analysis, strict free-boundary analysis, structured Shape inspection, B-Spline curve/surface inspection, mesh Face provenance, structured runtime diagnostics, OBB, trimming/offset, healing, triangulation, and engineering file exchange.

OCAF/XDE is intentionally excluded. Application Documents, domain Entities, Command/Tool systems, undo/redo, snapping, and JSON persistence belong to the consuming application.

## Requirements

- Windows x64
- Visual Studio 2022 / MSVC v143-compatible toolchain
- .NET SDK **8.0.423**, pinned by `global.json`
- C# 12.0
- CMake 3.21+
- Open CASCADE Technology **7.9.0**, VC14 x64 layout
- PowerShell 5.1+ or PowerShell 7+

Typical OCCT layout:

```text
D:\tools\occt-vc144-64\
├─ inc\
├─ win64\vc14\bin\
├─ win64\vc14\lib\
└─ 3rdparty-vc14-64\
```

## Repository structure

```text
bridge-contract.json    Bridge/ABI/OCCT/.NET/API source of truth
global.json             Pinned .NET SDK
Directory.Build.props   Shared compiler policy
src/OcctNative          C++17 Native bridge and C ABI
src/OcctNet             Core managed wrapper
src/OcctNet.WinForms    Reusable WinForms viewport host
src/OcctNet.Wpf         Reusable WPF viewport host
tests                   Contract checks, Managed tests, Native Smoke project
docs                    Organized API/integration/runtime guides
build.ps1               Validation/build/pack/smoke entry point
```

The managed wrapper intentionally exposes two façades:

- `OcctEngine`: interactive CAD/AIS/viewer session for displayed objects, structured selection identity, appearance, camera, interaction, and annotations.
- `OcctModelingSession`: headless geometry/topology kernel for batch processing, services, algorithms, meshing, inspection, healing, history, and engineering file exchange.

Equivalent OCCT operations may exist in both façades because the ownership models are intentionally different. Bridge 2.6 does **not** keep multiple compatibility names inside one façade. The reusable SDK currently exposes **95 public .NET types**.

## Canonical API naming

- Shape queries: `GetShape...`, `IsShape...`, `SetShape...`
- Edge queries: `GetEdge...`, `EvaluateEdge...`, `TrimEdge()`
- Face queries: `GetFace...`, `EvaluateFace...`
- Batch analysis: `AnalyzeEdgeAdjacency()`, `AnalyzeFaces()`, `AnalyzeFreeBounds()`
- Structured inspection: `InspectShape()`
- Indexed topology: `...At`, for example `GetSubshapeAt()`
- Construction: `Make...`
- Algorithms: operation verbs such as `Extrude()`, `OffsetShape()`, `OffsetWire()`
- Mesh: `Triangulate()`, `GetFaceMesh()`, `GetShapeMesh()`, `GetShapeMeshData()`

Managed handles are always owned by the engine/session that produced them. Raw IDs are resolved through `GetShape()`, `TryGetShape()`, `GetObject()`, or `TryGetObject()`; callers cannot construct fake public handles from a `long`.

## Interactive structured selection

`OcctEngine` can return selected/detected AIS identity without exposing OCCT owners or forcing applications to reverse-map raw IDs:

```csharp
var hits = engine.GetSelectedHits();
if (engine.TryGetDetectedHit(out var hover) && hover.IsSubshape)
{
    Console.WriteLine($"{hover.Owner.Id}: {hover.SubshapeType} #{hover.SubshapeIndex}");
}
```

`GetSelectedHits()` uses a two-call batch Native ABI rather than one P/Invoke per selected entity. `OcctSelectionHit` exposes only `Owner`, `SubshapeType`, and runtime `SubshapeIndex`; it deliberately does not expose a placeholder hit point. Runtime indices follow the same topology ordering as `GetSubshapeAt()` but are **not persistent naming**. See [Structured Viewer Selection Hits](docs/SELECTION_HITS.md).

## Headless modeling and inspection

```csharp
using var model = new OcctModelingSession();

var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(
    new OcctPoint3d(50, 40, -5),
    OcctVector3d.UnitZ,
    8,
    20);

var cut = model.Cut(plate, hole);
var bounds = model.GetShapeOrientedBounds(cut.Shape, optimal: true);
var adjacency = model.AnalyzeEdgeAdjacency(cut.Shape);
var faces = model.AnalyzeFaces(cut.Shape);
var inspection = model.InspectShape(cut.Shape);
var meshData = model.GetShapeMeshData(cut.Shape);

model.ExportStep(cut.Shape, "plate.step");
```

`AnalyzeFaces()` batches common Face metadata—surface type, orientation, area, tolerance, U/V bounds, AABB, edge count, and wire count—into one Native result instead of repeatedly crossing P/Invoke per Face/property.

`InspectShape()` composes shape validity/closure/tolerance, check report, topology counts, batched edge adjacency, batched Face analysis, strict free bounds, and optional mesh statistics into `OcctShapeInspectionReport`. It deliberately returns **facts**, not project-specific pass/fail policy. Mesh statistics are opt-in because they invoke triangulation.

For triangle picking or BIM/CAD property mapping:

```csharp
if (meshData.TryGetFaceForTriangle(hitTriangleIndex, out var face))
{
    // Use the source Face for selection, properties, analysis, or selective export.
}
```

For model-quality checks, use batched adjacency as the inexpensive first pass and strict free-boundary analysis for opening/gap decisions:

```csharp
var nonManifold = adjacency.NonManifoldEdges;
var freeBounds = model.AnalyzeFreeBounds(cut.Shape, tolerance: 1e-6);
```

B-Spline definitions remain managed snapshots rather than leaked OCCT handles:

```csharp
var curveData = model.GetBSplineCurveData(edge);
var surfaceData = model.GetBSplineSurfaceData(face);
```

See the [documentation index](docs/INDEX.md) for selection hits, API coverage, geometry utilities, B-Splines, topology, Shape inspection, mesh provenance, and runtime diagnostics.

## Build and validation

When Native work is required:

```powershell
git clone https://github.com/zly258/OcctCSharpBridge.git
cd OcctCSharpBridge
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"
```

General syntax:

```powershell
.\build.ps1 <target> <configuration> [-OcctRoot <path>]
```

| Target | Purpose | OCCT SDK |
|---|---|---|
| `validate` | API/version/organization/PInvoke/host/package contracts | No |
| `managed` | Build reusable managed wrapper + hosts | No |
| `pack` | Build and validate local managed NuGet + symbol packages | No |
| `ci` | Contract checks + Managed builds/tests + Smoke compile + package validation | No |
| `native` | Build `OcctNative.dll` with CMake/MSVC | Yes |
| `smoke` | Build and run real OCCT Native scenarios | Yes |
| `all` | Build Native bridge and reusable managed hosts | Yes |

Preferred no-SDK pre-push gate:

```powershell
.\build.ps1 ci Release
```

Create the three local managed SDK packages explicitly with:

```powershell
.\build.ps1 pack Release
```

Packages are written to `artifacts/packages`. Package versions come from `bridge-contract.json`, include XML IntelliSense documentation and symbol packages, and are checked to ensure they do **not** contain `OcctNative.dll`, OCCT `TK*.dll`, or a `runtimes/` Native payload. NuGet packaging is intentionally a **main-only SDK concern**; the `demo` branch owns complete application publishing instead. See [PACKAGING.md](docs/PACKAGING.md).

The strongest release gate is local because GitHub-hosted CI does not contain this project's OCCT SDK:

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

Cloud CI validates static contracts, all Managed projects/tests, Smoke source compilation, and main-branch NuGet packages. It does not claim to execute real OCCT geometry without the SDK/runtime.

The purpose of every test project and PowerShell contract script is documented in [`tests/README.md`](tests/README.md); small scripts are retained only when they protect a distinct contract.

## Runtime deployment and diagnostics

Keep `OcctNet.dll`, the selected viewport host, `OcctNative.dll`, OCCT runtime DLLs, and required third-party DLLs from the **same Bridge build**. Do not mix managed/native binaries across ABI revisions.

`OcctRuntime.GetDiagnosticReport()` is a side-effect-free human-readable report. `OcctRuntime.GetDiagnosticInfo()` returns structured app-local/configured/loaded bridge and `TKernel.dll` paths, existence states, process/OS architecture, and the detailed report without configuring or loading the runtime. See [Structured Runtime Diagnostics](docs/RUNTIME_DIAGNOSTICS.md).

## Desktop demos

The `main` branch does not contain complete CAD applications. Use `demo` for WinForms, WPF, and Avalonia examples:

```powershell
git switch demo
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"
.\build.ps1 all Release
.\run.ps1 winform
.\run.ps1 wpf
.\run.ps1 avalonia
```

Demo publishing resolves application-local Native dependencies and validates Native loading before producing a distributable package. Demo projects are deliberately non-packable as NuGet packages.

## Referencing from another project

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
  <!-- Optional -->
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.WinForms\OcctNet.WinForms.csproj" />
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.Wpf\OcctNet.Wpf.csproj" />
</ItemGroup>
```

For a local NuGet feed, run `build.ps1 pack` on `main` and add `artifacts/packages` as a package source. The consuming application must still deploy the matching Native Bridge/OCCT runtime.

## Compatibility contract

Authoritative metadata is in `bridge-contract.json`:

- Bridge: `2.6.0`
- Native ABI: `3`
- OCCT: exactly `7.9.0`
- Target: `.NET 8`, Windows x64
- Native exports: `348`
- Managed P/Invoke declarations: `348`
- Public .NET types: `95`
- Viewer API: `214`
- Modeling API: `134`

`build.ps1 validate` fails when metadata, declarations, P/Invoke mappings, naming/organization contracts, SDK/package policy, or required documentation drift.

## Troubleshooting

**`OCCT_ROOT is not configured`**  
Set `$env:OCCT_ROOT` or pass `-OcctRoot`.

**`TKernel.lib` / `TKernel.dll` not found**  
Verify the expected `win64\vc14\lib` and `win64\vc14\bin` layout and OCCT 7.9.0.

**Managed build succeeds but Native loading fails**  
A Managed build or NuGet package does not deploy OCCT. Use the Demo publish process or deploy the matching Native/OCCT/third-party dependency closure beside the executable, then inspect `OcctRuntime.GetDiagnosticInfo()`.

**Need a runnable CAD example**  
Use the `demo` branch; keep application-specific Document/Tool code out of `main`.

## License

The project is provided under the [PolyForm Noncommercial License 1.0.0](LICENSE). Open CASCADE Technology and third-party components remain subject to their own licenses.

## Contact

Liaoyuan Zhang · [zhangly1403@gmail.com](mailto:zhangly1403@gmail.com)
