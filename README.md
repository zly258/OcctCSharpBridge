# OcctCSharpBridge

[简体中文](README.zh-CN.md) · [Desktop demos](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge is a Windows x64 bridge from **Open CASCADE Technology 7.9.0** to **.NET 8**. The `main` branch contains the reusable C++ bridge, strict C ABI, type-safe managed wrapper, optional WinForms/WPF viewport hosts, contract checks, native smoke scenarios, and the managed SDK packaging definition. Complete CAD applications are maintained on the `demo` branch.

Bridge **2.6.0 / ABI 3** is a breaking cleanup release: compatibility aliases and public raw-ID handle constructors were removed, managed native flags were replaced by `bool`/enums, naming was normalized, and the headless modeling API gained OBB, topology identity, planar faces with holes, exact edge trimming, planar wire offset, and whole-shape mesh extraction.

OCAF/XDE is intentionally excluded. Document entities, command history, undo/redo, JSON persistence, tools, snapping, and other application concepts belong to the consuming application.

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
docs                    API coverage, getting started, packaging/runtime guides
build.ps1               Validation/build/pack/smoke entry point
```

The managed wrapper intentionally exposes two façades:

- `OcctEngine`: interactive CAD/AIS/viewer session. It owns displayed objects, selection, appearance, camera, interaction, annotations, and interactive document geometry.
- `OcctModelingSession`: headless geometry/topology kernel for batch processing, services, algorithms, meshing, analysis, healing, history, and engineering file exchange.

These two façades may expose equivalent OCCT operations because their object models are different by design. Bridge 2.6 does **not** keep multiple compatibility names inside the same façade.

Interactive objects use one public abstraction: `IOcctObject` exposes `Id`, `Kind`, and `IsValid`; actual instances are `OcctShape`, `OcctText`, or `OcctDimension`. There is no generic object wrapper and no public raw-ID object constructor. Together with the headless types, the reusable SDK currently exposes **81 public .NET types**.

## Canonical API naming

- Shape queries: `GetShape...`, `IsShape...`, `SetShape...`
- Edge queries: `GetEdge...`, `EvaluateEdge...`, `TrimEdge()`
- Face queries: `GetFace...`, `EvaluateFace()`
- Indexed topology: `...At`, for example `GetSubshapeAt()`
- Construction: `Make...`
- Algorithms: verbs such as `Extrude()`, `OffsetShape()`, `OffsetWire()`
- Mesh: `Triangulate()`, `ClearTriangulation()`, `GetFaceMesh()`, `GetShapeMesh()`

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
var mesh = model.GetShapeMesh(cut.Shape);
model.ExportStep(cut.Shape, "plate.step");
```

Direct planar-hole construction is also available:

```csharp
var outer = model.MakeRectangleWire(100, 80);
var inner = model.MakeRectangleWire(20, 20, new OcctPoint3d(40, 30, 0));
var face = model.MakePlanarFace(outer, new[] { inner });
var offset = model.OffsetWire(outer, 5.0, joinType: OcctJoinType.Arc);
```

See [API_COVERAGE.md](docs/API_COVERAGE.md) for the organized capability guide and [GETTING_STARTED.md](docs/GETTING_STARTED.md) for a compact integration walkthrough.

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

GitHub-hosted CI cannot provide the project-specific OCCT SDK, so the repository deliberately does not keep a permanently skipped cloud Native workflow. Native execution is a local release gate; cloud CI validates the complete managed/static contract and main-branch managed packages.

## Runtime deployment

Keep `OcctNet.dll`, the selected viewport host, `OcctNative.dll`, OCCT runtime DLLs, and required third-party DLLs from the **same Bridge build**. Do not mix managed/native binaries from different ABI revisions.

`OcctRuntime.GetDiagnosticReport()` reports native bridge candidates, configured OCCT paths, and resource variables. It is intended for diagnosing deployment failures such as Win32 error 126.

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
- Native exports: `336`
- Managed P/Invoke declarations: `336`
- Public .NET types: `81`
- Viewer API: `212`
- Modeling API: `124`

`build.ps1 validate` fails when these values, declarations, P/Invoke mappings, naming/organization contracts, SDK/package policy, or documentation drift.

## Troubleshooting

**`OCCT_ROOT is not configured`**  
Set `$env:OCCT_ROOT` or pass `-OcctRoot`.

**`TKernel.lib` / `TKernel.dll` not found**  
Verify the expected `win64\vc14\lib` and `win64\vc14\bin` layout and OCCT 7.9.0.

**Managed build succeeds but Native loading fails**  
A managed build or NuGet package does not deploy OCCT. Use the Demo publish process or deploy the matching Native/OCCT/third-party dependency closure beside the executable.

**Need a runnable CAD example**  
Use the `demo` branch; keep application-specific document/tool code out of `main`.

## License

The project is provided under the [PolyForm Noncommercial License 1.0.0](LICENSE). Open CASCADE Technology and third-party components remain subject to their own licenses.

## Contact

Liaoyuan Zhang · [zhangly1403@gmail.com](mailto:zhangly1403@gmail.com)