# Getting Started

OcctCSharpBridge 2.6 targets **Windows x64**, **.NET 10 (`net10.0-windows`)**, and **Open CASCADE Technology 7.9.0**. The repository builds with .NET SDK **10.0.302** and C# **14.0**. The core managed API has two entry points:

- `OcctModelingSession`: headless modeling, topology, analysis, meshing, healing, and exchange.
- `OcctEngine`: AIS/viewer, camera, selection, display, annotations, and interactive OCCT objects.

WinForms, WPF, and Avalonia are optional viewport hosts. CAD application responsibilities such as Document, Command, and Tool are outside the bridge; see [Architecture Boundaries](ARCHITECTURE_BOUNDARIES.md).

## 1. Build the managed SDK

```powershell
.\build.ps1 managed Release
```

No OCCT SDK is required. This builds:

```text
OcctNet
OcctNet.WinForms
OcctNet.Wpf
OcctNet.Avalonia
```

The Avalonia host is currently a Windows HWND adapter. Framework-dependent desktop applications require a .NET Desktop Runtime 10.x installation; normal .NET patch roll-forward applies, so one exact `10.0.x` patch is not pinned.

## 2. OCCT SDK and runtime

Native builds use this conventional OCCT root when no override is supplied:

```text
D:\tools\occt-vc144-64
```

If it exists, run directly:

```powershell
.\build.ps1 all Release
```

For another SDK location:

```powershell
.\build.ps1 all Release -OcctRoot "E:\SDK\occt-7.9.0"
```

At application runtime, configure explicitly before creating the first Engine/ModelingSession when the native runtime is not deployed app-local:

```csharp
OcctRuntime.Configure(
    occtRoot: @"D:\tools\occt-vc144-64",
    nativeBridgeDirectory: @"D:\workspace\OcctCSharpBridge\build\native\bin\Release");
```

`OCCT_ROOT`, `CASROOT`, and `OCCT_BRIDGE_NATIVE_DIR` remain supported. For deployment diagnostics:

```csharp
Console.WriteLine(OcctRuntime.GetDiagnosticReport());
```

## 3. Headless modeling

```csharp
using var model = new OcctModelingSession();

var box = model.MakeBox(100, 80, 60);
var hole = model.MakeCylinder(
    new OcctPoint3d(50, 40, -10),
    OcctVector3d.UnitZ,
    radius: 12,
    height: 80);

var result = model.Cut(box, hole);
var bounds = model.GetShapeBounds(result.Shape);
model.Triangulate(result.Shape);
var mesh = model.GetShapeMeshData(result.Shape);
model.ExportStep(result.Shape, @"D:\temp\part.step");
```

`OcctModelShape` values are owned by the session that produced them; do not fabricate handles from raw IDs.

## 4. Interactive viewer

Direct `OcctEngine` use requires a native window handle:

```csharp
using var engine = new OcctEngine();
engine.Initialize(hwnd);
engine.SetView(OcctViewOrientation.Isometric);
engine.SetProjection(OcctProjectionType.Orthographic);
engine.FitAll();
```

Applications normally use the matching reusable host:

```text
WinForms  → OcctViewportControl
WPF       → OcctWpfViewport
Avalonia  → OcctAvaloniaViewport   (Windows HWND)
```

Avoid reimplementing the OCCT HWND lifetime, rectangle selection, and basic viewer input adapter in each consuming application.

## 5. Modeling to viewer

```csharp
using var model = new OcctModelingSession();
var shape = model.MakeBox(100, 80, 60);

using var engine = new OcctEngine();
engine.Initialize(hwnd);
var displayed = engine.Display(model, shape, fit: true);
```

The displayed `OcctShape` belongs to the target `OcctEngine`; the original `OcctModelShape` remains owned by its modeling session.

## 6. Validation

Full Managed gate without an OCCT SDK:

```powershell
.\build.ps1 ci Release
```

It covers static contracts, all four managed SDK assemblies, managed regressions, the public API signature snapshot, Smoke compilation, and NuGet package checks. Managed tests and Smoke compile against the same target framework declared by `bridge-contract.json`.

Real native release gate:

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

Only the native gate proves local OCCT 7.9.0 C++ compile/link, DLL loading, and actual geometry/topology execution.

See [Packaging and Runtime Deployment](PACKAGING.md) for managed package and native-runtime rules.
