# OcctCSharpBridge Documentation

This documentation describes the **`main` branch**, the sole formal SDK source based on OCCT 7.9.0, .NET 10 and C# 14.

Current source contract:

- Bridge 3.0.0-preview.1 / Native ABI 5 current, ABI 4 compatible
- 431 Native exports / 431 P/Invoke mappings
- 145 public .NET types
- Viewer / Modeling API: 292 / 139
- Target frameworks: `net10.0` core/Avalonia; `net10.0-windows` WinForms/WPF
- Public assemblies: `OcctNet`, `OcctNet.WinForms`, `OcctNet.Wpf`, `OcctNet.Avalonia`

The `demo` and `avalonia` branches contain consumer examples and packaging; SDK implementations remain in `main`.

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
10. [Bridge Migration](bridge-migration.md)

`bridge-contract.json` is the machine-readable source contract. `dist/win-x64/bridge-manifest.json` describes the concrete SDK that was actually published from `main`.