# OCCT 7.9.0 C# Engineering Bridge

[简体中文](README.zh-CN.md)

This repository wraps Open CASCADE Technology 7.9.0 through a **C++17 native DLL, stable C ABI, and .NET 8 P/Invoke**. The `main` branch contains only the reusable bridge; complete WinForms and WPF applications are preserved in the [`demo`](https://github.com/zly258/OcctCSharpBridge/tree/demo) branch.

## Architecture

```text
Consumer application
├─ OcctModelingSession      Headless modeling, topology, healing, mesh and exchange
└─ OcctEngine               AIS / V3d viewer, selection, presentation and annotations
          ↓
OcctNet (.NET 8, x64)
          ↓ P/Invoke / C ABI
OcctNative (C++17)
          ↓
OCCT 7.9.0
```

`OcctModelingSession` requires no HWND and creates no `AIS_InteractiveContext`, so it can be used by batch jobs, services and unit tests. A headless shape can be copied into an existing viewer when presentation is required:

```csharp
using var model = new OcctModelingSession();
var box = model.MakeBox(100, 80, 60);

using var viewer = new OcctEngine();
viewer.Initialize(hwnd);
var displayed = viewer.Display(model, box, fit: true);
```

The sessions own independent registries. Display interop copies the `TopoDS_Shape`; the viewer does not retain pointers into the modeling registry.

## Coverage

| Area | Main capabilities |
|---|---|
| Headless core | Shape registry, lifecycle, location, orientation, hash, bounds and mass properties |
| Geometry | Points, lines, polylines, circles, arcs, ellipses, Bezier, interpolated BSpline, polygons and planar profiles |
| Primitives | Box, cylinder, cone, sphere, torus, wedge, compound, wire, sewing and shell-to-solid |
| Topology | Subshapes, outer/inner wires, ancestor relationships, edge/face evaluation and geometry types |
| Modeling | Boolean operations, splitter, extrude, revolve, sweep, loft, fillet, chamfer, offset, thick solid and same-domain unification |
| Boolean controls | Fuzzy value, parallel mode, non-destructive mode, glue, inverted-solid checks and result simplification |
| History | Generated, modified and removed topology plus operation reports, without OCAF/TNaming |
| Healing | Detailed validity report and `ShapeFix_Shape` with tolerance controls |
| Analysis | Distance, point projection, ray intersections and solid point classification |
| Mesh | Explicit meshing, triangulation cleanup and face nodes/triangles/UV/normals |
| Exchange | Headless STEP, IGES, BREP and STL import/export |
| Viewer | HWND viewer, AIS presentation, selection, camera, materials, lighting, text and basic dimensions |

See [API coverage](docs/API_COVERAGE.md) for the detailed boundary.

## Headless example

```csharp
using OcctNet;

OcctRuntime.Configure(
    occtRoot: @"D:\tools\occt-vc144-64",
    nativeBridgeDirectory: @"D:\libs\OcctBridge");

using var model = new OcctModelingSession();

var body = model.MakeBox(100, 80, 60);
var hole = model.MakeCylinder(
    new OcctPoint3d(50, 40, -10),
    OcctVector3d.UnitZ,
    radius: 12,
    height: 80);

var cut = model.Cut(body, hole);
var faces = model.GetSubshapes(cut.Shape, OcctShapeType.Face);

model.Mesh(cut.Shape);
var mesh = model.GetFaceMesh(faces[0]);

var generated = model.GetGeneratedShapes(cut.OperationId, body);
model.ExportStep(cut.Shape, @"D:\output\result.step");
```

## Build and smoke test

Requirements:

- Windows x64;
- Visual Studio 2022 with Desktop development with C++;
- .NET 8 SDK;
- CMake 3.21 or later;
- OCCT 7.9.0 built for Visual C++ x64.

```powershell
Set-ExecutionPolicy -Scope Process Bypass

.\build.ps1 native Release
.\build.ps1 managed Release
.\build.ps1 all Release
.\build.ps1 smoke Release

.\build.ps1 smoke Debug -OcctRoot "D:\SDK\occt-vc144-64"
```

The smoke target exercises headless Boolean operations, topology, mesh extraction, ray intersections, loft, healing, same-domain unification and a BREP round trip.

## Reference

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
</ItemGroup>
```

Deploy `OcctNative.dll` beside the application or configure `OCCT_BRIDGE_NATIVE_DIR`. OCCT and third-party runtime DLLs must be discoverable through `PATH`.

## Intentional exclusions

The bridge does not wrap OCAF documents, labels, attributes, OCAF undo/redo, TNaming or XDE document tools. STEP and IGES therefore provide pure `TopoDS_Shape` geometry exchange and do not promise XDE assembly hierarchy, instances, names, colors or layers.

The project also does not attempt a one-to-one mapping of every OCCT C++ class. Specialized surface filling, variable fillets, PipeShell, every curve/surface intersection combination, and glTF/OBJ providers remain modular extension points rather than dependencies of the core ABI.

## License

The project is provided under the [PolyForm Noncommercial License 1.0.0](LICENSE). OCCT and third-party components remain subject to their own licenses.
