# OcctCSharpBridge Documentation

This directory is the English documentation set for `OcctCSharpBridge/main`.

Current baseline: Bridge 2.6.0, Native ABI 4, OCCT 7.9.0, .NET SDK 10.0.302, `net10.0-windows`, C# 14, Windows x64.

## Reading order

1. [Getting Started](01_Getting-Started.md)
2. [Architecture and Boundaries](02_Architecture-and-Boundaries.md)
3. [API Coverage and Design Conventions](03_API-Coverage-and-Design-Conventions.md)
4. [Geometry, Modeling and Topology](04_Geometry-Modeling-and-Topology.md)
5. [Viewer, Selection and Interaction](05_Viewer-Selection-and-Interaction.md)
6. [Mesh and Data Exchange](06_Mesh-and-Data-Exchange.md)
7. [Runtime, Deployment and Diagnostics](07_Runtime-Deployment-and-Diagnostics.md)
8. [Build, Test and Publish](08_Build-Test-and-Publish.md)
9. [Complete API Reference](api/README.md)

`main` is a reusable OCCT bridge, not a complete CAD product framework. Document models, feature trees, commands, tools, undo/redo, snapping, grips and product-specific rules belong above the bridge.

The machine-readable source of truth for version, platform and API counts is `bridge-contract.json`. Conceptual documentation explains behavior and contracts; generated API Reference enumerates the exact public .NET surface.