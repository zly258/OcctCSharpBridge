# OcctCSharpBridge Documentation

This directory is the English documentation set for `OcctCSharpBridge/main`.

## Current contract

| Item | Current value |
| --- | --- |
| Author | **Liaoyuan Zhang** |
| Bridge version | **2.6.0** |
| Native ABI | **4** |
| Native exports / P/Invoke | **344 / 344** |
| Public .NET types | **105** |
| Viewer / Modeling API | **210 / 134** |
| OCCT | **7.9.0** |
| .NET SDK | **10.0.302** |
| Target Framework | **`net10.0-windows`** |
| C# | **14.0** |
| Native Bridge | **C++17** |
| Avalonia | **12.1.0** |
| Platform | **Windows x64** |

The machine-readable source of truth for version, platform and API counts is `bridge-contract.json`.

## Reading order

1. [Getting Started](01_Getting-Started.md)
2. [Architecture and Boundaries](02_Architecture-and-Boundaries.md)
3. [API Coverage and Design Conventions](03_API-Coverage-and-Design-Conventions.md)
4. [Geometry, Modeling and Topology](04_Geometry-Modeling-and-Topology.md)
5. [Viewer, Selection and Interaction](05_Viewer-Selection-and-Interaction.md)
6. [Mesh and Data Exchange](06_Mesh-and-Data-Exchange.md)
7. [Runtime, Deployment and Diagnostics](07_Runtime-Deployment-and-Diagnostics.md)
8. [Build, Test and Publish](08_Build-Test-and-Publish.md)
9. [Complete Managed + Native API Reference](api/README.md)

`main` is a reusable OCCT bridge and Binary SDK producer, not a complete CAD product framework. Document models, feature trees, commands, tools, undo/redo, snapping, grips and product-specific rules belong above the Bridge.

Conceptual documentation explains behavior and contracts; the generated API Reference enumerates the exact managed public surface and Native C ABI. The author name is always written as **Liaoyuan Zhang** in both language trees.
