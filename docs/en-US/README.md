# OcctCSharpBridge Documentation

This documentation describes the supported Bridge 3 ABI5-only architecture, SDK consumption, runtime deployment, build/test boundaries, and migration rules. `bridge-contract.json` is the machine-readable source of truth.

## Current contract

- Bridge: `3.0.0-preview.1`
- Native ABI: **5 only** (`current = 5`, `minimumSupported = 5`)
- API policy: `abi5-only`
- OCCT: `7.9.0`
- Build SDK: stable **.NET 10**, baseline `10.0.100`, `rollForward=latestFeature`, no prerelease SDKs
- Managed Binary SDK: `net8.0` Core/Avalonia and `net8.0-windows` WinForms/WPF
- Supported consumer TFMs: .NET 8 / 9 / 10; desktop .NET 8 / 9 / 10 on Windows
- Public managed assemblies: `OcctNet`, `OcctNet.WinForms`, `OcctNet.Wpf`, `OcctNet.Avalonia`
- Platforms: Windows x64 / Linux x64

`main` / `main-dev` are the Bridge source lines. `demo` / `demo-dev` are reference Binary SDK consumers, not a second Bridge implementation.

## Guide

1. [Getting Started](01_Getting-Started.md) — choose the correct SDK and run a minimal consumer.
2. [Architecture and Boundaries](02_Architecture-and-Boundaries.md) — ownership and layering rules.
3. [API Coverage and Design Conventions](03_API-Coverage-and-Design-Conventions.md) — API organization and conventions.
4. [Geometry, Modeling and Topology](04_Geometry-Modeling-and-Topology.md) — modeling-side concepts.
5. [Viewer, Selection and Interaction](05_Viewer-Selection-and-Interaction.md) — host lifecycle and interaction contract.
6. [Mesh and Data Exchange](06_Mesh-and-Data-Exchange.md) — mesh and exchange boundaries.
7. [Runtime Deployment and Diagnostics](07_Runtime-Deployment-and-Diagnostics.md) — Native/OCCT deployment and troubleshooting.
8. [Build, Test and Publish](08_Build-Test-and-Publish.md) — fast consumer artifact production versus full release validation.
9. [Third-party SDK Consumption](09_Third-Party-SDK-Consumption.md) — detailed external project integration for Core, WinForms, WPF and Avalonia.
10. [Bridge 3 ABI5 Migration](bridge-migration.md) — migration from older Bridge contracts.

## Documentation policy

The repository maintains architectural and operational documentation rather than generated per-type/per-method API pages. Native/managed API-surface parity is validated from current source so documentation does not become a second, stale source of truth.

For most application teams, start with **09 Third-party SDK Consumption**. Bridge contributors should additionally read **08 Build, Test and Publish** before changing SDK production or release behavior.
