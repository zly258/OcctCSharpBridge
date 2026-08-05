# OCCT 7.9.0 C# Engineering Bridge

[简体中文](README.zh-CN.md) · [Documentation](docs/README.md) · [WinForms/WPF demo branch](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge exposes Open CASCADE Technology **7.9.0** to .NET 8 through a C++17 native DLL, a stable C ABI, and type-safe C# wrappers. The `main` branch is the reusable SDK. The `demo` branch adds complete WinForms and WPF applications, shared CAD commands, an API catalog, executable scenarios, and portable publishing scripts.

## Design goals

- Keep OCCT C++ handles, labels, and implementation classes behind a stable C ABI.
- Provide engineering workflows instead of mirroring every OCCT class one-to-one.
- Separate Viewer/AIS, headless modeling, and OCAF/XDE document lifetimes.
- Preserve the caller's camera: creating or editing a shape does **not** automatically call `FitAll`.
- Support efficient multi-object updates through nestable display batches.
- Require exactly OCCT 7.9.0 so OCAF, TNaming, XDE, and persistence behavior is deterministic.

## Architecture

```text
Application
├─ OcctEngine              HWND Viewer, AIS objects, selection, camera and annotations
├─ OcctModelingSession     Headless geometry, topology, algorithms, healing and mesh
└─ OcafDocument            OCAF/TDF/TNaming/XDE, assemblies, metadata and persistence
          ↓
OcctNet (.NET 8, Windows x64)
          ↓ P/Invoke / stable C ABI
OcctNative (C++17 DLL)
          ↓
Open CASCADE Technology 7.9.0
```

The three high-level sessions own independent native registries. Shapes are copied between registries; raw `Handle(...)`, `TopoDS_Shape*`, `TDF_Label*`, and persistence-driver objects never cross the ABI.

## Which API should I use?

| Requirement | API |
|---|---|
| Interactive 3D viewport, picking, camera, colors and dimensions | `OcctEngine` |
| Server-side or background modeling without a window | `OcctModelingSession` |
| Persistent product structure, parameters, metadata and undo/redo | `OcafDocument` |
| Build a shape headlessly and display it later | `OcctModelingSession` + `OcctEngine.AddShape(...)` |
| Preserve assemblies, names, colors, layers and materials in exchange | `OcafDocument` STEPCAF/IGESCAF APIs |

## Viewer quick start

```csharp
using OcctNet;

OcctRuntime.Configure(
    occtRoot: @"D:\tools\occt-vc144-64",
    nativeBridgeDirectory: AppContext.BaseDirectory);

using var engine = new OcctEngine();
engine.Initialize(viewportHandle);

var box = engine.MakeBox(100, 80, 60);
engine.SetColor(box, Color.SteelBlue);

// Creation keeps the current camera unchanged.
// Fit explicitly only when the workflow needs it.
engine.FitAll();
```

### Efficient multi-object display

```csharp
using (engine.BeginDisplayBatch())
{
    var box = engine.MakeBox(100, 80, 60);
    var cylinder = engine.MakeCylinder(20, 80, 150, 0, 0);
    engine.SetMaterial(box, OcctMaterial.Plastified);
    engine.SetColor(cylinder, Color.OrangeRed);

    // Optional. Without this call, the camera is preserved.
    engine.FitAll();
}
```

During a batch, `Display`, `Redisplay`, visibility, color, material, delete, and other scene updates are accumulated. The outermost batch performs one final redraw. Batches can be nested.

## Headless modeling quick start

```csharp
using var model = new OcctModelingSession();
var body = model.MakeBox(100, 80, 60);
var hole = model.MakeCylinder(
    new OcctPoint3d(50, 40, -10),
    OcctVector3d.UnitZ,
    12,
    80);
var result = model.Cut(body, hole);

if (!result.Succeeded || !model.IsValid(result.Shape))
    throw new InvalidOperationException(result.Report);

model.ExportStep(result.Shape, @"D:\output\result.step");
```

## OCAF/XDE quick start

```csharp
using var model = new OcctModelingSession();
var body = model.MakeBox(100, 80, 60);

using var document = new OcafDocument(OcafDocumentFormats.BinaryXde)
{
    UndoLimit = 20
};

using (var command = document.BeginCommand())
{
    var product = document.AddShape(model, body);
    document.SetName(product, "Housing");
    document.SetColor(product, OcafColorType.Surface,
        new OcafColor(0.2, 0.45, 0.8));
    var layer = document.AddLayer("Equipment");
    document.SetLayer(product, layer);
    document.SetMaterial(product, "Steel", "Structural steel", 7.85);
    command.Commit();
}

document.SaveAs(@"D:\output\assembly.xbf");
document.ExportStep(@"D:\output\assembly.step");
```

## Capability overview

| Area | Main capabilities |
|---|---|
| Viewer and AIS | HWND initialization, camera, orthographic/perspective projection, view cube, triedron, lighting, antialiasing, materials, colors, transparency, text and dimensions |
| Selection | Object and subshape filters, point selection, rectangle selection, persistent selection state and OCCT overlay rubber band |
| Geometry | Vertex, line, arc, circle, ellipse, Bezier, B-spline, wire, face and standard solids |
| Features | Boolean, splitter, extrusion, revolution, sweep, loft, fillet, chamfer, offset, thick solid, drilling and transforms |
| Analysis | Bounds, mass properties, topology, distance, projection, ray intersection, classification, validation and reports |
| Mesh and healing | Explicit meshing, face triangulation access, BRepCheck and ShapeFix workflows |
| OCAF/TDF | Documents, labels, transactions, undo/redo, scalar/reference/array/position/shape attributes, variables, expressions and relations |
| TNaming | Generated/Modify/Delete/Select evolution, named-shape history and selector solve workflows |
| XDE | Free shapes, assemblies, components, locations, names, colors, visibility, layers, materials and validation properties |
| Exchange | BREP/STL/STEP/IGES pure shapes plus STEPCAF/IGESCAF metadata-preserving workflows |

## Version contract

This repository requires **exactly OCCT 7.9.0**:

1. CMake parses `Standard_Version.hxx` and rejects a different version.
2. Native code contains compile-time version assertions.
3. Managed OCAF initialization validates the loaded bridge at runtime.

Do not silently substitute 7.8, 7.9.1/7.9.3, or 8.x binaries.

## Build

Requirements:

- Windows 10/11 x64
- Visual Studio 2022 with **Desktop development with C++**
- .NET 8 SDK
- CMake 3.21+
- OCCT 7.9.0 VC++ x64 SDK

```powershell
Set-ExecutionPolicy -Scope Process Bypass

# Validate C header, C++ implementation and P/Invoke parity.
.\build.ps1 validate Release

# Build reusable managed assemblies.
.\build.ps1 managed Release

# Build native bridge and run smoke coverage.
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

## Referencing the SDK

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
</ItemGroup>
```

Deploy `OcctNative.dll` beside the application or set `OCCT_BRIDGE_NATIVE_DIR`. OCCT runtime DLLs and required third-party DLLs must be discoverable through the application directory or `PATH`; OCCT resource directories must be available through `CASROOT`/`OCCT_ROOT` when exchange or persistence uses them.

The `demo` branch contains `publish.ps1`, which produces a self-contained Windows x64 package with managed runtime, native bridge, OCCT DLLs, third-party DLLs, resource directories, launchers, manifests, and licenses.

## Documentation

- [Documentation index](docs/README.md)
- [Getting started](docs/GETTING_STARTED.md)
- [Viewer, selection and display updates](docs/VIEWER_AND_DISPLAY.md)
- [Deployment and runtime layout](docs/DEPLOYMENT.md)
- [Complete API coverage](docs/API_COVERAGE.md)
- [OCAF/XDE coverage](docs/OCAF_COVERAGE.md)
- [Extended OCAF API](docs/OCAF_EXTENDED_API.md)

## Boundary and extension policy

The bridge intentionally does not expose raw OCCT ownership objects, concrete TDF delta subclasses, persistence-driver internals, or arbitrary custom C++ callbacks. New typed modules should be added behind the C ABI without leaking implementation pointers or breaking existing signatures.

Advanced GD&T, view, note, clipping-plane, and PBR visual-material sections can currently be inspected through section labels and generic attribute JSON. Dedicated typed CRUD modules may be added incrementally.

## License

The project is provided under the [PolyForm Noncommercial License 1.0.0](LICENSE). OCCT, Microsoft runtime components, and third-party libraries remain subject to their own licenses and redistribution terms.
