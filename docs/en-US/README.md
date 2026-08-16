# OcctCSharpBridge Documentation

This directory documents the architecture, usage, build, deployment, and migration conventions for the Bridge 3 ABI5-only SDK. `bridge-contract.json` is the source-contract source of truth.

Current source contract:

- Bridge: `3.0.0-preview.1`;
- Native ABI: **ABI 5 only**, with `current = 5` and `minimumSupported = 5`;
- API policy: `abi5-only`;
- OCCT: `7.9.0`;
- .NET SDK: **`10.0.303` exactly**, with roll-forward disabled;
- target frameworks: `net10.0` for Core/Avalonia and `net10.0-windows` for WinForms/WPF;
- public managed assemblies: `OcctNet`, `OcctNet.WinForms`, `OcctNet.Wpf`, `OcctNet.Avalonia`;
- source platforms: Windows x64 / Linux x64.

`main` / `main-dev` are the SDK source line. The unified `demo` / `demo-dev` branches are Binary SDK consumers: Windows x64 provides WinForms, WPF and Avalonia hosts; Linux x64 provides Avalonia only. No standalone Avalonia consumer branch is part of the supported architecture.

## Guide

1. [Getting Started](01_Getting-Started.md)
2. [Architecture and Boundaries](02_Architecture-and-Boundaries.md)
3. [API Coverage and Design Conventions](03_API-Coverage-and-Design-Conventions.md)
4. [Geometry, Modeling and Topology](04_Geometry-Modeling-and-Topology.md)
5. [Viewer, Selection and Interaction](05_Viewer-Selection-and-Interaction.md)
6. [Mesh and Data Exchange](06_Mesh-and-Data-Exchange.md)
7. [Runtime Deployment and Diagnostics](07_Runtime-Deployment-and-Diagnostics.md)
8. [Build, Test and Publish](08_Build-Test-and-Publish.md)
9. [Bridge 3 ABI5 Migration](bridge-migration.md)

This directory no longer tracks generated per-type/per-function API reference pages. Native/managed API-surface integrity is validated directly from current source by `tests/check-api-surface.ps1`; hard-coded API counts are intentionally not maintained.
