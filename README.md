# OCCT 7.9.0 C# Bridge

[简体中文](README.zh-CN.md)

A focused Windows x64 wrapper for Open CASCADE Technology 7.9.0:

```text
C# application
    ↓ ProjectReference
OcctNet (.NET 8)
    ↓ P/Invoke / stable C ABI
OcctNative (C++17 DLL)
    ↓
OCCT 7.9.0
```

The `main` branch contains only the reusable wrapper. Complete WinForms and WPF CAD demonstrations are preserved in the [`demo`](../../tree/demo) branch.

## Repository layout

| Path | Purpose |
|---|---|
| `src/OcctNative` | Native C++ bridge, C ABI, OCCT geometry, topology, AIS, view, annotations, and data exchange |
| `src/OcctNet` | Type-safe C# API, native lifetime management, runtime discovery, and WinForms viewport host |
| `build.ps1` | Builds the native bridge, managed wrapper, or both |

## Requirements

- Windows x64
- Visual Studio 2022 with Desktop development with C++
- .NET 8 SDK
- CMake 3.21 or later
- OCCT 7.9.0 built for Visual C++ x64

The conventional development path is `D:\tools\occt-vc144-64`. A different installation can be supplied through `-OcctRoot` or the `OCCT_ROOT` environment variable.

## Build

```powershell
Set-ExecutionPolicy -Scope Process Bypass

.\build.ps1 all Release
.\build.ps1 native Debug
.\build.ps1 managed Release
.\build.ps1 all Release -OcctRoot "D:\SDK\occt-vc144-64"
```

Outputs:

```text
build\native\bin\<Configuration>\OcctNative.dll
src\OcctNet\bin\x64\<Configuration>\net8.0-windows\OcctNet.dll
```

When target `all` is used, `OcctNative.dll` is also copied beside `OcctNet.dll`.

## Reference from another project

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
</ItemGroup>
```

Configure non-default runtime locations before creating the first engine:

```csharp
using OcctNet;

OcctRuntime.Configure(
    occtRoot: @"D:\SDK\occt-vc144-64",
    nativeBridgeDirectory: @"D:\Libraries\OcctBridge");

using var engine = new OcctEngine();
```

Deploy `OcctNative.dll` with the application. OCCT runtime DLLs and required third-party runtime directories must be available through `PATH`; `OcctRuntime` configures them automatically when a valid OCCT root is found.

## API coverage

- Geometry and topology creation
- Extrude, revolve, sweep, loft, fillet, chamfer, offset, shelling, and drilling
- Boolean operations and section curves
- AIS display, selection, highlighting, camera, standard views, and ViewCube
- Linear, angular, radius, and diameter dimensions
- STEP, IGES, BREP, and STL import/export
- Bounding boxes, mass properties, centroids, distances, topology counts, and validation

## Branch policy

- `main`: reusable native and managed wrapper only
- `demo`: full CAD sample applications and shared demo infrastructure

## License

This project uses the [PolyForm Noncommercial License 1.0.0](LICENSE). OCCT and third-party components remain subject to their own licenses.
