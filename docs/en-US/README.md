# OcctCSharpBridge Documentation

This documentation describes the supported Bridge 3 ABI5-only architecture, SDK consumption, runtime deployment, build/test boundaries, and Stable compatibility rules. `bridge-contract.json` is the machine-readable source of truth.

## Current 3.0 Stable contract

- Bridge: `3.0.0`
- Native ABI: **5 only** (`current = 5`, `minimumSupported = 5`)
- API policy: `abi5-only`
- OCCT: `7.9.0`
- Build SDK: stable .NET 10, baseline `10.0.100`, `rollForward=latestFeature`
- Managed Binary SDK: `net8.0` Core/Avalonia and `net8.0-windows` WinForms/WPF
- Supported consumers: .NET 8 / 9 / 10
- Public managed assemblies: `OcctNet`, `OcctNet.WinForms`, `OcctNet.Wpf`, `OcctNet.Avalonia`
- Official prebuilt distribution: **Windows x64**
- Source-build support: Windows x64 / Linux x64
- Linux UI: Avalonia, source build only; no official prebuilt asset

`main` is the formal SDK source line. `demo` is the reference SDK consumer rather than a second Bridge implementation; the `website` branch hosts the project site.

## Guide

1. [Getting Started](01_Getting-Started.md) — choose the correct SDK and run a minimal consumer.
2. [Architecture and Boundaries](02_Architecture-and-Boundaries.md) — ownership and layering rules.
3. [API Coverage and Design Conventions](03_API-Coverage-and-Design-Conventions.md) — API organization and conventions.
4. [Geometry, Modeling and Topology](04_Geometry-Modeling-and-Topology.md) — modeling-side concepts.
5. [Viewer, Selection and Interaction](05_Viewer-Selection-and-Interaction.md) — host lifecycle and interaction contract.
6. [Mesh and Data Exchange](06_Mesh-and-Data-Exchange.md) — mesh and exchange boundaries.
7. [Runtime Deployment and Diagnostics](07_Runtime-Deployment-and-Diagnostics.md) — Native/OCCT deployment and troubleshooting.
8. [Build, Test and Publish](08_Build-Test-and-Publish.md) — fast consumer artifacts, Windows Stable gate, and Linux source validation.
9. [Third-party SDK Consumption](09_Third-Party-SDK-Consumption.md) — external Core, WinForms, WPF and Avalonia integration, deployment, and upgrades.
10. [Stable Support and Compatibility](10_Stable-Support-and-Compatibility.md) — platform, .NET, ABI, threading, lifetime, unit, tolerance, and version compatibility boundaries.
11. [Demo previews](../../README.md#demo-previews) — canonical screenshots of the unified demo branch.
