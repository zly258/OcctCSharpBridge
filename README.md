# OcctCSharpBridge

[简体中文](README.zh-CN.md) · [Documentation](docs/INDEX.md) · [Architecture boundaries](docs/ARCHITECTURE_BOUNDARIES.md) · [Desktop demos](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge is a Windows x64 **Open CASCADE Technology 7.9.0 → .NET 10** bridge. `main` contains only reusable OCCT native/.NET wrappers, WinForms/WPF/Avalonia viewport hosts, contract tests, Native Smoke scenarios, and managed SDK packaging. Complete CAD applications and upper-layer CAD frameworks live on `demo`.

Bridge **2.6.0 / Native ABI 3** follows one deliberate boundary: **the bridge exposes OCCT capabilities and viewport adapters, not an application CAD framework.** OCAF/XDE, Document, Feature/Entity, Command, Tool, Undo/Redo, Snap/Grip, project persistence, and product UI do not belong in `main`.

## Requirements

- Windows x64
- Visual Studio 2022 / MSVC v143-compatible toolchain
- .NET SDK **10.0.302** (`global.json`)
- .NET Desktop Runtime **10.x** for framework-dependent desktop applications
- Target framework **`net10.0-windows`**
- C# **14.0**
- CMake 3.21+
- Open CASCADE Technology **7.9.0**, VC14 x64 layout
- PowerShell 5.1+ or PowerShell 7+

A newer .NET 10 patch runtime is valid through normal framework patch roll-forward; the repository does not pin consuming applications to one exact `10.0.x` runtime patch.

The conventional OCCT root is:

```text
D:\tools\occt-vc144-64\
├─ inc\
├─ win64\vc14\bin\
├─ win64\vc14\lib\
└─ 3rdparty-vc14-64\
```

If OCCT is installed there, `native`, `smoke`, and `all` do not require `OCCT_ROOT`. Other locations can be supplied through `$env:OCCT_ROOT` or `-OcctRoot`. `validate`, `managed`, `pack`, and `ci` do not require an OCCT SDK.

## `main` repository structure

```text
bridge-contract.json     Bridge / ABI / OCCT / .NET / API contract
src/OcctNative           C++17 OCCT bridge and stable C ABI
src/OcctNet              Core managed wrapper
src/OcctNet.WinForms     WinForms HWND viewport host
src/OcctNet.Wpf          WPF viewport host
src/OcctNet.Avalonia     Avalonia + Windows HWND viewport host
tests                    Contract checks, Managed regression, Native Smoke
docs                     API, architecture, deployment, diagnostics
build.ps1                validate/build/pack/smoke entry point
```

`main` must not contain `OcctDemo.*`, complete CAD applications, DocumentManager, CommandBus, ToolManager, or similar product-layer implementations. See [Architecture Boundaries](docs/ARCHITECTURE_BOUNDARIES.md).

## Managed façades

### `OcctEngine`

Interactive AIS/Viewer/Object façade for views, cameras, selection, displayed objects, appearance, transforms, annotations, and geometry used in the viewer lifecycle.

### `OcctModelingSession`

Headless modeling façade for construction, topology, Boolean/feature algorithms, analysis, meshing, healing, operation history, and STEP/IGES/BREP/STL exchange.

Some construction operations intentionally exist on both façades because their ownership/lifetime models differ. They are not merged merely to remove superficial API duplication.

## UI hosts

`main` formally provides three reusable adapters:

- `OcctNet.WinForms`
- `OcctNet.Wpf`
- `OcctNet.Avalonia`

The Avalonia adapter creates a Windows child HWND through `NativeControlHost`; it is therefore still a **Windows x64 host** and does not imply Linux/macOS native-viewer support.

WinForms and Avalonia share only host-neutral selection/threshold/throttling/default-zoom decisions. Window lifecycle, DPI, mouse capture, WPF hosting, and Win32 subclassing remain framework-specific.

## API and compatibility contract

Authoritative values come from `bridge-contract.json`:

- Bridge: `2.6.0`
- Native ABI: `3`
- OCCT: exactly `7.9.0`
- .NET SDK: `10.0.302`
- Target framework: `net10.0-windows`
- C#: `14.0`
- Native exports: `348`
- Managed P/Invoke declarations: `348`
- Public .NET types: `99`
- Compatibility .NET types: `1`
- Viewer API: `214`
- Modeling API: `134`

`OcctObject` is the only separately tracked Bridge 2.5 compatibility public type. It remains during 2.x for source compatibility but receives no new legacy API; new code should use owner-aware `OcctShape`, `OcctText`, `OcctDimension`, and `IOcctObject` paths.

Structured viewer subshape indices are runtime topology indices, not persistent naming.

See [API Coverage](docs/API_COVERAGE.md) for the complete categorized surface.

## Headless example

```csharp
using var model = new OcctModelingSession();

var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(
    new OcctPoint3d(50, 40, -5),
    OcctVector3d.UnitZ,
    8,
    20);

var cut = model.Cut(plate, hole);
var inspection = model.InspectShape(cut.Shape);
var mesh = model.GetShapeMeshData(cut.Shape);
model.ExportStep(cut.Shape, "plate.step");
```

The bridge also covers B-Spline data, analytic/differential geometry, projection/ray/classification, batched Edge/Face analysis, free bounds, OBB, trim/offset, healing, triangulation, and mesh Face provenance.

## Build

```powershell
.\build.ps1 <target> <configuration> [-OcctRoot <path>]
```

| Target | Purpose | OCCT SDK |
|---|---|---|
| `validate` | version/API/organization/PInvoke/UI-host/package/branch contracts | No |
| `managed` | build Core + WinForms + WPF + Avalonia | No |
| `pack` | build and validate managed NuGet/symbol packages | No |
| `ci` | contracts + Managed build/tests + public API snapshot + Smoke compile + package checks | No |
| `native` | CMake/MSVC build of `OcctNative.dll` | Yes |
| `smoke` | build and execute real OCCT native scenarios | Yes |
| `all` | Native + all reusable managed hosts | Yes |

Without an OCCT SDK:

```powershell
.\build.ps1 ci Release
```

With the conventional OCCT directory:

```powershell
.\build.ps1 all Release
```

For another SDK location:

```powershell
.\build.ps1 smoke Release -OcctRoot "E:\SDK\occt-7.9.0"
```

GitHub-hosted CI does not contain this project's real OCCT SDK. Cloud CI therefore validates static contracts, Managed builds/tests, the public API signature snapshot, Smoke source compilation, and NuGet packages. Actual C++ compile/link, DLL loading, and geometry/topology execution remain the responsibility of local `smoke`.

## NuGet

`main` produces four managed SDK packages:

```text
OcctNet
OcctNet.WinForms
OcctNet.Wpf
OcctNet.Avalonia
```

```powershell
.\build.ps1 pack Release
```

Packages are written to `artifacts/packages`. Managed packages intentionally do not bundle `OcctNative.dll`, OCCT `TK*.dll`, or a `runtimes/` native payload. Applications deploy a matching Native Bridge/OCCT runtime separately. See [Packaging](docs/PACKAGING.md).

## Referencing projects directly

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
  <!-- Pick the host(s) you actually use -->
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.WinForms\OcctNet.WinForms.csproj" />
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.Wpf\OcctNet.Wpf.csproj" />
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.Avalonia\OcctNet.Avalonia.csproj" />
</ItemGroup>
```

## `demo`

Complete WinForms/WPF/Avalonia CAD examples live on `demo`:

```powershell
git switch demo
.\build.ps1 all Release
.\run.ps1 winform
.\run.ps1 wpf
.\run.ps1 avalonia
```

`demo` contains the explicitly named `OcctDemo.Common` orchestration layer and three `OcctDemo.*` applications. Those are reference-application code, not `OcctNet` public API and not a reusable CAD framework.

## Runtime diagnostics

`OcctRuntime.GetDiagnosticInfo()` and `GetDiagnosticReport()` inspect application-local/configured/loaded `OcctNative.dll` and `TKernel.dll` paths, architecture, and dependency state without forcing native loading. See [Structured Runtime Diagnostics](docs/RUNTIME_DIAGNOSTICS.md).

## License

The project is provided under the [PolyForm Noncommercial License 1.0.0](LICENSE). Open CASCADE Technology and third-party components remain subject to their own licenses.

## Contact

Liaoyuan Zhang · [zhangly1403@gmail.com](mailto:zhangly1403@gmail.com)
