# OcctCSharpBridge Documentation

Current **source** contract: **Bridge 2.7.0 · ABI 4 · OCCT 7.9.0 · .NET 10 / C# 14 · Windows x64**.

`bridge-contract.json` is the source of truth for source code: **349 Native exports, 349 P/Invoke mappings, 117 public .NET types, Viewer 215, Modeling 134**.

> Published Binary SDK status is authoritative only from the tracked `main/dist/win-x64` payload. Read `dist/win-x64/bridge-contract.json` for its actual Bridge/ABI/API contract and `dist/win-x64/bridge-manifest.json` for the exact source commit and hashes; this documentation intentionally does not duplicate a release version that can become stale.

## Guide

1. [Getting Started](01_Getting-Started.md)
2. [Architecture and Boundaries](02_Architecture-and-Boundaries.md)
3. [API Coverage and Design Conventions](03_API-Coverage-and-Design-Conventions.md)
4. [Geometry, Modeling and Topology](04_Geometry-Modeling-and-Topology.md)
5. [Viewer, Selection and Interaction](05_Viewer-Selection-and-Interaction.md)
6. [Mesh and Data Exchange](06_Mesh-and-Data-Exchange.md)
7. [Runtime Deployment and Diagnostics](07_Runtime-Deployment-and-Diagnostics.md)
8. [Build, Test and Publish](08_Build-Test-and-Publish.md)
9. [Generated API Reference](api/README.md)

## Important boundary

XDE is used internally for STEP assembly/product structure and styles. It is not the application document/persistence model. The managed boundary is `OcctAssemblyDocument`; product documents, undo/redo and JSON persistence remain above the Bridge.

## Licensing

OcctCSharpBridge is licensed under **GNU LGPL version 2.1 + OcctCSharpBridge Exception 1.0**.

Commercial and proprietary applications may use the Bridge through .NET assembly references, dynamic linking, P/Invoke, or equivalent runtime linking without requiring the application itself to adopt the GNU LGPL solely because of that use. GNU LGPL obligations still apply to OcctCSharpBridge itself and to modified/derivative versions of the Bridge that are distributed.

See [`LICENSE`](../../LICENSE), [`LICENSE_LGPL_21.txt`](../../LICENSE_LGPL_21.txt), [`OcctCSharpBridge_LGPL_EXCEPTION.txt`](../../OcctCSharpBridge_LGPL_EXCEPTION.txt), and [`COMMERCIAL.md`](../../COMMERCIAL.md).

Open CASCADE Technology and other third-party components remain subject to their own licenses; OCCT keeps its own GNU LGPL 2.1 + Open CASCADE Exception terms.
