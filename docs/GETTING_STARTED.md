# Getting Started

OcctCSharpBridge 2.6 targets **Windows x64**, **.NET 8**, and **Open CASCADE Technology 7.9.0**. The managed API is split into two primary entry points:

- `OcctModelingSession`: headless modeling, topology, geometry analysis, triangulation, healing, and file exchange.
- `OcctEngine`: interactive AIS/viewer, camera, selection, display, annotations, and interactive CAD operations.

## 1. Build the managed SDK

```powershell
.\build.ps1 managed Release
```

This does not require an OCCT installation. Managed assemblies are built with the version from `bridge-contract.json`.

## 2. Configure the OCCT runtime

Configure the runtime before creating the first engine or modeling session when the runtime is not deployed beside the application:

```csharp
OcctRuntime.Configure(
    occtRoot: @"D:\tools\occt-vc144-64",
    nativeBridgeDirectory: @"D:\workspace\OcctCSharpBridge\build\native\bin\Release");
```

For published applications, app-local `OcctNative.dll` is preferred. `OCCT_ROOT`, `CASROOT`, and `OCCT_BRIDGE_NATIVE_DIR` are also supported.

When diagnosing deployment problems such as Win32 error 126, write the runtime report to the application log:

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
var mesh = model.GetShapeMesh(result.Shape);

model.ExportStep(result.Shape, @"D:\temp\part.step");
```

Shapes returned by a modeling session are bound to that session. Persist a native ID only when necessary, and resolve it back through `GetShape` / `TryGetShape` rather than constructing a handle manually.

## 4. Interactive viewer

`OcctEngine` requires a valid native window handle before viewer operations:

```csharp
using var engine = new OcctEngine();
engine.Initialize(hwnd);
engine.SetView(OcctViewOrientation.Isometric);
engine.SetProjection(OcctProjectionType.Orthographic);
engine.FitAll();
```

For WinForms and WPF applications, prefer the reusable viewport hosts in `OcctNet.WinForms` and `OcctNet.Wpf` instead of duplicating HWND integration.

## 5. Modeling to viewer

A headless shape can be copied into an initialized AIS engine:

```csharp
using var model = new OcctModelingSession();
var shape = model.MakeBox(100, 80, 60);

using var engine = new OcctEngine();
engine.Initialize(hwnd);
var displayed = engine.Display(model, shape, fit: true);
```

The displayed `OcctShape` belongs to the target `OcctEngine`; the original `OcctModelShape` remains owned by its modeling session.

## 6. Validation before release

Managed validation:

```powershell
.\build.ps1 ci Release
```

Native validation on a machine with OCCT 7.9.0:

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

The native smoke test covers bridge/ABI compatibility, modeling operations, triangulation, topology, BREP round-trip, and STEP round-trip.

See [PACKAGING.md](PACKAGING.md) for managed package and runtime deployment rules.
