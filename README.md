# OCCT 7.9.0 C# Engineering Bridge

[简体中文](README.zh-CN.md)

This repository wraps Open CASCADE Technology **7.9.0** through a **C++17 native DLL, stable C ABI, and .NET 8 P/Invoke**. The `main` branch contains the reusable bridge; complete WinForms/WPF applications are preserved in the [`demo`](https://github.com/zly258/OcctCSharpBridge/tree/demo) branch.

## Architecture

```text
Consumer application
├─ OcctModelingSession      Headless modeling, topology, healing, mesh and pure-shape exchange
├─ OcafDocument             OCAF/TDF/TNaming/XDE, assemblies, metadata and persistence
└─ OcctEngine               AIS/V3d viewer, selection, presentation and annotations
          ↓
OcctNet (.NET 8, x64)
          ↓ P/Invoke / stable C ABI
OcctNative (C++17)
          ↓
OCCT 7.9.0
```

The three sessions own independent native lifetimes. Shapes are copied between the headless registry, OCAF/XDE documents and the Viewer; no OCCT C++ handle or label pointer is exposed to managed code.

## Exact version contract

OCAF, TNaming, persistence and XDE APIs are version-sensitive. The bridge therefore requires **exactly OCCT 7.9.0**:

- CMake parses `Standard_Version.hxx` and rejects another version;
- native code contains a compile-time version assertion;
- `OcafDocument` verifies the loaded native DLL at runtime.

## Coverage

| Area | Main capabilities |
|---|---|
| Headless modeling | Shape lifecycle, geometry, primitives, topology, Booleans, features, healing, analysis and mesh |
| OCAF document | Create/open/save, BinXCAF/XmlXCAF/BinOcaf/XmlOcaf, transactions and undo/redo |
| TDF/TData | Label hierarchy, generic attribute inspection, common scalar/reference/array/position/shape attributes |
| TNaming | Evolution records, current named shape, old/new history pairs and selector workflows |
| XDE assemblies | Shapes, free shapes, assemblies, components, references, locations and assembly updates |
| XDE metadata | RGBA colors, visibility, layers, physical materials, area, volume, centroid and length units |
| Exchange | Pure-shape STEP/IGES/BREP/STL plus metadata-preserving STEPCAF/IGESCAF |
| Viewer | HWND viewer, AIS presentation, selection, camera, materials, lighting, text and dimensions |

See [API coverage](docs/API_COVERAGE.md) and [OCAF/XDE coverage](docs/OCAF_COVERAGE.md).

## OCAF/XDE example

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
    document.SetColor(product, OcafColorType.Surface, new OcafColor(0.2, 0.45, 0.8));
    var layer = document.AddLayer("Equipment");
    document.SetLayer(product, layer);
    document.SetMaterial(product, "Steel", "Structural steel", 7.85);
    command.Commit();
}

document.SaveAs(@"D:\output\assembly.xbf");
document.ExportStep(@"D:\output\assembly.step");
```

## Build and smoke test

Requirements: Windows x64, Visual Studio 2022 with Desktop development with C++, .NET 8 SDK, CMake 3.21+, and **OCCT 7.9.0 VC++ x64**.

```powershell
Set-ExecutionPolicy -Scope Process Bypass

.\build.ps1 native Release
.\build.ps1 managed Release
.\build.ps1 all Release
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

The smoke target covers headless modeling plus OCAF transactions, undo/redo, TDF attributes, XDE shape/name/color/layer/material data, BinXCAF persistence/reopen, and shape transfer back to `OcctModelingSession`.

## Reference

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
</ItemGroup>
```

Deploy `OcctNative.dll` beside the application or configure `OCCT_BRIDGE_NATIVE_DIR`. OCCT and third-party runtime DLLs must be discoverable through `PATH`.

## Boundary

The bridge provides stable engineering workflows rather than a one-to-one projection of every OCCT implementation class. Raw handles, label/attribute pointers, persistence-driver internals, concrete TDF delta classes and custom TFunction driver callbacks do not cross the C ABI. Advanced GD&T, view, note, clipping-plane and PBR visual-material sections remain available through section labels and generic attribute JSON inspection; dedicated typed CRUD modules can be added without breaking the core ABI.

## License

The project is provided under the [PolyForm Noncommercial License 1.0.0](LICENSE). OCCT and third-party components remain subject to their own licenses.
