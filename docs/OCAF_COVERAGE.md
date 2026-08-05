# OCAF / XDE Coverage (OCCT 7.9.0)

## Version contract

The OCAF bridge is intentionally compiled against **exactly OCCT 7.9.0**.

- CMake reads `Standard_Version.hxx` and rejects any major/minor/maintenance version other than `7.9.0`.
- Native code contains a compile-time `static_assert` for the same version.
- `OcafDocument` checks the loaded native version before creating or opening a document.

This strict contract avoids silently accepting changed OCAF, TNaming, persistence, or XDE signatures from another OCCT release.

## Architecture

```text
OcafDocument (.NET 8)
        ↓ UTF-8 P/Invoke
OcctOcaf.h stable C ABI
        ↓
TDocStd / TDF / TDataStd / TDataXtd / TNaming / XCAF / STEPCAF / IGESCAF
        ↓
OCCT 7.9.0
```

No `Handle(...)`, `TDF_Label`, `TDF_Attribute`, or other C++ object pointer crosses the ABI. Labels are represented by stable TDF entry strings such as `0:1:2`; shapes are copied through the existing `OcctModelingSession` registry.

## Implemented interface groups

### Document and persistence

- create, open, save, save-as and close documents;
- `BinXCAF`, `XmlXCAF`, `BinOcaf` and `XmlOcaf` storage formats;
- saved/changed/empty/valid state, storage format and JSON diagnostic dump;
- exact native version and capability diagnostics.

### Transactions and history

- new/open/commit/abort command;
- configurable undo limit;
- undo, redo, available stack sizes and stack clearing;
- nested transaction mode, transaction-only modification mode and empty-label saving mode;
- disposable `OcafCommandScope` with automatic abort when not committed.

### TDF labels and generic attributes

- root/main labels, label lookup/creation, child creation and traversal;
- father, tag, depth, root/imported state;
- generic attribute enumeration including runtime type, GUID and `DumpJson()` output;
- forget one attribute by GUID or all attributes recursively.

### Standard and geometric attributes

- `TDataStd_Name`, `Comment`, `AsciiString`, `Integer`, `Real`, `UAttribute`;
- `TDF_Reference`;
- integer, real, boolean, byte and extended-string arrays with arbitrary lower bounds;
- `TDataXtd_Position`;
- `TDataXtd_Shape` with safe `OcctModelingSession` shape transfer.

### TNaming

- primitive/generated/generated-from/modified/deleted/selected evolution recording;
- current named shape, evolution, version and old/new pair enumeration;
- `TNaming_Selector` select, solve and identification checks.

### XDE shapes and assemblies

- shapes and free-shape enumeration;
- add, replace, remove and find shapes;
- component creation/removal, referred shape, location and assembly update;
- shape classification: simple shape, assembly, component, reference, free shape and subshape;
- document length unit in metres.

### XDE metadata

- RGBA general/surface/curve colors, visibility and color-by-layer;
- layer definitions, assignments and visibility;
- legacy physical/material definitions, density and shape assignment;
- validation area, volume and centroid;
- section labels for shapes, colors, layers, physical materials, visual materials, geometric tolerances, views, clipping planes and notes.

### Metadata-preserving exchange

- STEP import/export through `STEPCAFControl_Reader/Writer`;
- IGES import/export through `IGESCAFControl_Reader/Writer`;
- native XDE assembly, name, color, layer and material data are retained where supported by the exchange format and OCCT translators.

## Managed example

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

## Deliberate ABI boundaries

“Complete” here means a coherent engineering facade for the public OCAF/XDE workflows above, with every declared C API implemented in native code, P/Invoke, and the public managed layer. It does **not** mean exposing every OCCT C++ implementation class one-for-one.

The following remain deliberately outside the stable ABI:

- raw `Handle(...)`, `TDF_Label`, attribute and delta pointers;
- persistence driver internals and storage/retrieval callback classes;
- custom `TFunction_Driver` registration callbacks and application-specific drivers;
- concrete `TDF_Delta` implementation classes;
- specialized CRUD models for every GD&T, view, note, clipping-plane and PBR visual-material field.

Advanced XDE sections are still accessible as labels and through generic attribute enumeration/JSON inspection. They can be added as typed workflow modules without breaking the ABI.

## Validation

The source tree contains matching declarations across:

1. `OcctOcaf.h`;
2. all `OcctOcaf*.cpp` implementations;
3. all `OcafNativeMethods.*.cs` declarations;
4. the public `OcafDocument` facade.

The smoke test covers transactions, undo/redo, labels, standard attributes, XDE shape/name/color/layer/material/validation data, binary XCAF persistence, reopen verification, and shape transfer back to `OcctModelingSession`.

Run on the target Windows machine:

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```
