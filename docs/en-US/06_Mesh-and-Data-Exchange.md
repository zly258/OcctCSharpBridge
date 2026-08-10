# 06 Mesh and Data Exchange

The Bridge keeps file exchange and mesh operations behind strongly typed managed APIs while OCCT performs the native work.

## Mesh

Meshing APIs accept explicit quality/tolerance parameters and operate on owner-aware shapes. Mesh data crossing the ABI uses structured/bulk transfer rather than exposing OCCT triangulation objects directly.

## STEP and IGES

STEP and IGES import/export use OCCT data-exchange toolkits through the stable native ABI. Callers receive managed shapes and normal Bridge errors instead of OCCT reader/writer objects.

## BREP and STL

BREP supports direct topology persistence; STL supports triangulated exchange. Applications choose the format according to whether exact topology or tessellated geometry is required.

## Runtime resources

Data exchange may require OCCT resource directories in addition to DLLs. `OcctRuntime` can configure standard runtime locations from `OCCT_ROOT`/`CASROOT`; portable application publishers should also include the required OCCT resource folders.

See the generated API Reference for exact import/export and mesh signatures.