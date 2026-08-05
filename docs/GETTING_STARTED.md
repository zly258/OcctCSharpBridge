# Getting Started

This guide covers the reusable `main` branch. For complete desktop applications, switch to `demo`.

## 1. Requirements

- Windows 10 or Windows 11 x64
- Visual Studio 2022 with Desktop development with C++
- .NET 8 SDK
- CMake 3.21 or newer
- Open CASCADE Technology 7.9.0 built for MSVC x64

A typical OCCT layout is:

```text
D:\tools\occt-vc144-64
├─ inc
├─ win64\vc14\lib
├─ win64\vc14\bin
├─ 3rdparty-vc14-64
└─ src
```

The project validates `Standard_Version.hxx`; another OCCT version is rejected.

## 2. Configure runtime discovery

Use either an environment variable:

```powershell
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"
```

or configure the managed runtime before creating any session:

```csharp
OcctRuntime.Configure(
    occtRoot: @"D:\tools\occt-vc144-64",
    nativeBridgeDirectory: AppContext.BaseDirectory);
```

`OcctNative.dll` is searched in the application directory and in `OCCT_BRIDGE_NATIVE_DIR`. Its dependent OCCT and third-party DLLs must be in the application directory or `PATH`.

## 3. Build

```powershell
Set-ExecutionPolicy -Scope Process Bypass

.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 native Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

`validate` checks C declarations, C++ definitions, and C# P/Invoke names. `smoke` exercises headless modeling, OCAF transactions, persistence, XDE metadata, and shape transfer.

## 4. First Viewer program

`OcctEngine` requires a valid Windows HWND. Initialize it after the control has created its handle and call `Resize()` when the host size changes.

```csharp
using OcctNet;
using System.Drawing;

using var engine = new OcctEngine();
engine.Initialize(hwnd);
engine.SetGradientBackground(Color.White, Color.LightSteelBlue);
engine.SetTriedronVisible(true);
engine.SetViewCubeVisible(true);

var box = engine.MakeBox(100, 80, 60);
engine.SetColor(box, Color.SteelBlue);
engine.SetMaterial(box, OcctMaterial.Plastified);

// Shape creation does not change the camera.
engine.SetView(OcctViewOrientation.Isometric);
engine.FitAll();
```

Do not create and destroy `OcctEngine` for every command. One engine should normally live with one viewport.

## 5. Multiple Viewer objects

Use a display batch whenever one logical command creates or modifies several presentations:

```csharp
using (engine.BeginDisplayBatch())
{
    var basePlate = engine.MakeBox(200, 120, 12);
    var column1 = engine.MakeCylinder(15, 100, 30, 30, 12);
    var column2 = engine.MakeCylinder(15, 100, 170, 90, 12);

    engine.SetColor(basePlate, Color.SlateGray);
    engine.SetColor(column1, Color.Goldenrod);
    engine.SetColor(column2, Color.Goldenrod);

    engine.FitAll(); // optional and explicit
}
```

Without `FitAll()`, the batch ends with one redraw and preserves the current camera.

## 6. First headless program

`OcctModelingSession` does not initialize OpenGL or require a window.

```csharp
using var model = new OcctModelingSession();

var body = model.MakeBox(100, 80, 60);
var cutter = model.MakeCylinder(
    new OcctPoint3d(50, 40, -10),
    OcctVector3d.UnitZ,
    12,
    80);

var operation = model.Cut(body, cutter);
if (!operation.Succeeded)
    throw new InvalidOperationException(operation.Report);

var result = operation.Shape;
if (!model.IsValid(result))
    throw new InvalidOperationException("The Boolean result is invalid.");

var bounds = model.GetBounds(result);
var mass = model.GetVolumeProperties(result);
model.ExportStep(result, @"D:\output\result.step");
```

Dispose headless sessions when their registered shapes are no longer required.

## 7. Transfer a headless shape to the Viewer

```csharp
using var model = new OcctModelingSession();
var source = model.MakeBox(100, 80, 60);

var displayed = engine.AddShape(model, source);
engine.SetColor(displayed, Color.CornflowerBlue);
engine.FitAll();
```

The shape is copied; the Viewer does not retain a pointer into the headless registry.

## 8. First OCAF/XDE document

```csharp
using var model = new OcctModelingSession();
var body = model.MakeBox(100, 80, 60);

using var document = new OcafDocument(OcafDocumentFormats.BinaryXde)
{
    UndoLimit = 20
};

using (var command = document.BeginCommand())
{
    var label = document.AddShape(model, body);
    document.SetName(label, "Housing");
    document.SetComment(label, "Main equipment body");
    document.SetColor(label, OcafColorType.Surface,
        new OcafColor(0.2, 0.45, 0.8));
    command.Commit();
}

document.SaveAs(@"D:\output\housing.xbf");
```

A command that is not committed should be aborted or disposed according to the API contract. Use document-level Undo/Redo rather than trying to retain native Label pointers.

## 9. Error handling

Native failures are converted to `OcctException` or operation result objects. Validate inputs before calling expensive algorithms and inspect reports when an algorithm exposes one.

```csharp
try
{
    var result = engine.MakeBox(100, 80, 60);
}
catch (OcctException ex)
{
    logger.LogError(ex, "OCCT operation failed");
}
```

## 10. Threading

Viewer operations should run on the UI thread that owns the HWND. Do not call one `OcctEngine` concurrently from multiple threads. For background geometry, create a separate `OcctModelingSession` and copy completed shapes to the Viewer on the UI thread.

## Next steps

- [Viewer and display](VIEWER_AND_DISPLAY.md)
- [Deployment](DEPLOYMENT.md)
- [API coverage](API_COVERAGE.md)
- [OCAF/XDE coverage](OCAF_COVERAGE.md)
