# OcctCSharpBridge

[简体中文](README.zh-CN.md) · [Documentation](docs/INDEX.md) · [Desktop demos](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge is a Windows x64 bridge from **Open CASCADE Technology 7.9.0** to **.NET 8**. The `main` branch contains the reusable C++ bridge, strict C ABI, type-safe managed wrapper, optional WinForms/WPF viewport hosts, contract checks, native smoke scenarios, and managed SDK packaging. Complete CAD applications are maintained on the `demo` branch.

Bridge **2.6.0 / ABI 3** is a cleanup and expansion release. Compatibility aliases and public raw-ID constructors were removed, naming and ownership were normalized, and the reusable headless API now covers topology analysis, B-Spline curve/surface inspection, mesh provenance, structured runtime diagnostics, OBB, exact trimming, planar offset, faces with holes, whole-shape triangulation, healing, and engineering file exchange.

OCAF/XDE is intentionally excluded. Document entities, command history, undo/redo, JSON persistence, application tools, snapping, and other product concepts belong to the consuming application.

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
src/OcctNative          C++17 native bridge and C ABI
src/OcctNet             Core managed wrapper
src/OcctNet.WinForms    Reusable WinForms viewport host
src/OcctNet.Wpf         Reusable WPF viewport host
tests                   Contract checks, managed tests, native smoke project
docs                    Organized API/integration/runtime guides
build.ps1               Validation/build/pack/smoke entry point
```

The managed wrapper intentionally exposes two façades:

- `OcctEngine`: interactive CAD/AIS/viewer session. It owns displayed objects, selection, appearance, camera, interaction, annotations, and interactive document geometry.
- `OcctModelingSession`: headless geometry/topology kernel for batch processing, services, algorithms, meshing, analysis, healing, history, and engineering file exchange.

These façades may expose equivalent OCCT operations because their object models are different by design. Bridge 2.6 does **not** keep multiple compatibility names inside one façade.

Interactive objects use one public abstraction: `IOcctObject` exposes `Id`, `Kind`, and `IsValid`; actual instances are `OcctShape`, `OcctText`, or `OcctDimension`. Together with headless and host types, the reusable SDK currently exposes **90 public .NET types**.

## Canonical API naming

- Shape queries: `GetShape...`, `IsShape...`, `SetShape...`
- Edge queries: `GetEdge...`, `EvaluateEdge...`, `TrimEdge()`
- Face queries: `GetFace...`, `EvaluateFace...`
- Indexed topology: `...At`, for example `GetSubshapeAt()`
- Construction: `Make...`
- Algorithms: verbs such as `Extrude()`, `OffsetShape()`, `OffsetWire()`
- Mesh: `Triangulate()`, `GetFaceMesh()`, `GetShapeMesh()`, `GetShapeMeshData()`
- Analysis: `AnalyzeEdgeAdjacency()`, `AnalyzeFreeBounds()`

Managed handles are always owned by the engine/session that produced them. Raw IDs are resolved using `GetShape()`, `TryGetShape()`, `GetObject()`, or `TryGetObject()`; they are not used to construct fake managed handles.

## Headless modeling example

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
var topology = model.AnalyzeEdgeAdjacency(cut.Shape);
var meshData = model.GetShapeMeshData(cut.Shape);
model.ExportStep(cut.Shape, "plate.step");
```

For triangle picking or BIM/CAD property mapping, resolve a combined mesh triangle back to its source Face:

```csharp
if (meshData.TryGetFaceForTriangle(hitTriangleIndex, out var face))
{
    // Use face for selection, properties, analysis, or selective export.
}
```

For model-quality checks, use the batch adjacency snapshot first and the stricter free-boundary algorithm when a final opening/gap decision is required:

```csharp
var adjacency = model.AnalyzeEdgeAdjacency(cut.Shape);
var nonManifold = adjacency.NonManifoldEdges;
var freeBounds = model.AnalyzeFreeBounds(cut.Shape, tolerance: 1e-6);
```

B-Spline definitions can be inspected without leaking OCCT handles:

```csharp
var curveData = model.GetBSplineCurveData(edge);
var surfaceData = model.GetBSplineSurfaceData(face);
```

See the [documentation index](docs/INDEX.md) for organized guides on API coverage, B-Splines, topology analysis, mesh provenance, geometry utilities, and runtime diagnostics.

## Build and validation

Clone and configure the local OCCT SDK when Native work is required:

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
| `ci` | Contract checks + managed builds/tests + Smoke compile + package validation | No |
| `native` | Build `OcctNative.dll` with CMake/MSVC | Yes |
| `smoke` | Build and run real OCCT native modeling scenarios | Yes |
| `all` | Build native bridge and reusable managed hosts | Yes |

Preferred no-SDK pre-push check:

```powershell
.\build.ps1 ci Release
```

Create the three local managed SDK packages explicitly with:

```powershell
.\build.ps1 pack Release
```

Packages are written to `artifacts/packages`. Package versions come from `bridge-contract.json`, include XML IntelliSense documentation and symbol packages, and are checked to ensure they do **not** contain `OcctNative.dll`, OCCT `TK*.dll`, or a `runtimes/` native payload. NuGet packaging is intentionally a **main-branch SDK concern only**; the `demo` branch is non-packable and owns complete application publishing instead. See [PACKAGING.md](docs/PACKAGING.md).

Strongest local validation before release:

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

GitHub-hosted CI does not contain the project OCCT SDK. Cloud CI therefore validates static contracts, all Managed projects/tests, Smoke source compilation, and main-branch NuGet packages; actual OCCT geometry/topology execution remains the local Native release gate.

## Runtime deployment and diagnostics

Keep `OcctNet.dll`, the selected viewport host, `OcctNative.dll`, OCCT runtime DLLs, and required third-party DLLs from the **same Bridge build**. Do not mix managed/native binaries from different ABI revisions.

`OcctRuntime.GetDiagnosticReport()` is a side-effect-free human-readable report. `OcctRuntime.GetDiagnosticInfo()` returns structured app-local/configured/loaded bridge and `TKernel.dll` paths, existence states, process/OS architecture, and the same detailed report without configuring or loading the runtime. See [Structured Runtime Diagnostics](docs/RUNTIME_DIAGNOSTICS.md).

## Desktop demos

The `main` branch does not contain complete CAD applications. Use the `demo` branch for WinForms, WPF, and Avalonia examples:

```powershell
git switch demo
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"
.\build.ps1 all Release
.\run.ps1 winform
.\run.ps1 wpf
.\run.ps1 avalonia
```

The Demo release script packages application-local Native dependencies and validates Native loading before producing the final package. Demo projects are deliberately non-packable as NuGet packages.

## Referencing from another project

During development, reference the projects directly:

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
- Native exports: `345`
- Managed P/Invoke declarations: `345`
- Public .NET types: `90`
- Viewer API: `212`
- Modeling API: `133`

`build.ps1 validate` fails when metadata, declarations, P/Invoke mappings, naming/organization contracts, SDK/package policy, or required documentation drift.

## Troubleshooting

**`OCCT_ROOT is not configured`**  
Set `$env:OCCT_ROOT` or pass `-OcctRoot`.

**`TKernel.lib` / `TKernel.dll` not found**  
Verify the expected `win64\vc14\lib` and `win64\vc14\bin` layout and OCCT 7.9.0.

**Managed build succeeds but Native loading fails**  
A managed build or NuGet package does not deploy OCCT. Use the Demo publish process or deploy the matching Native/OCCT/third-party dependency closure beside the executable, then inspect `OcctRuntime.GetDiagnosticInfo()`.

**Need a runnable CAD example**  
Use the `demo` branch; keep application-specific document/tool code out of `main`.

## License

The project is provided under the [PolyForm Noncommercial License 1.0.0](LICENSE). Open CASCADE Technology and third-party components remain subject to their own licenses.

## Contact

Liaoyuan Zhang · [zhangly1403@gmail.com](mailto:zhangly1403@gmail.com)
