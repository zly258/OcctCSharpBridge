# 06 Mesh and Data Exchange

The Bridge keeps file exchange and mesh operations behind strongly typed managed APIs while OCCT performs the native work.

## Mesh

Meshing APIs accept explicit quality/tolerance parameters and operate on owner-aware shapes. Mesh data crossing the ABI uses structured/bulk transfer rather than exposing OCCT triangulation objects directly.

## STEP and IGES

STEP and IGES import/export use OCCT data-exchange toolkits through the stable native ABI. The legacy `ImportStep()` API still returns the first managed display shape for source compatibility.

For assembly-aware STEP work, use:

```csharp
OcctAssemblyDocument document = engine.ImportStepDocument("assembly.step");

foreach (OcctAssemblyNode root in document.Roots)
{
    // Traverse root.Children.
}
```

`OcctAssemblyDocument` is an XDE occurrence-tree snapshot. It keeps stable XDE node IDs, Assembly/Instance/Part roles, reference names, local/global transforms, visibility, surface RGBA, curve color and explicitly styled subshapes. A STEP Part is never inferred from the number of contained solids; one legitimate Part may contain multiple solids.

The compatibility `step-path:` application tag may still be emitted for older consumers, but it is not the source of truth for the assembly API.

When a STEP document is opened into an empty viewer scene, non-geometric edits such as object name, color, transparency and visibility are synchronized back to the imported XDE document. Saving the unchanged geometry therefore preserves the original assembly/reference structure and untouched subshape styles. Topology-changing operations invalidate that pristine document and require a reconstructed export.

## BREP and STL

BREP supports direct topology persistence; STL supports triangulated exchange. Applications choose the format according to whether exact topology or tessellated geometry is required.

## Runtime resources

Data exchange may require OCCT resource directories in addition to DLLs. `OcctRuntime` can configure standard runtime locations from `OCCT_ROOT`/`CASROOT`; portable application publishers should also include the required OCCT resource folders.

See the generated API Reference for exact import/export and mesh signatures.
