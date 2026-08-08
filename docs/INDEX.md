# OcctCSharpBridge Documentation

This index covers the reusable bridge content shared by `main` and `demo`. NuGet production remains a `main`-branch policy; runnable desktop applications and publish workflows remain a `demo`-branch policy.

## Start here

| Document | Use it for |
|---|---|
| [API Coverage](API_COVERAGE.md) | Current Bridge/ABI/API scope, ownership rules, façade responsibilities, validation boundaries |
| [Managed Geometry and Transform Utilities](GEOMETRY_UTILITIES.md) | Point/vector math, bounds, UV ranges, affine matrices, locations and transforms |
| [B-Spline Curve and Surface Inspection](BSPLINE_CURVES.md) | Degree, poles, weights, knots, multiplicities and surface control grids |
| [Topology Adjacency and Free-Boundary Analysis](TOPOLOGY_ANALYSIS.md) | Batched adjacency, manifold/non-manifold checks and strict free-boundary analysis |
| [Batch Face Analysis and Shape Inspection](SHAPE_INSPECTION.md) | Batched Face metadata and structured model-audit snapshots without application-specific pass/fail rules |
| [Shape Mesh Face Provenance](MESH_PROVENANCE.md) | Combined-mesh source-Face ranges, picking and CAD/BIM property mapping |
| [Structured Runtime Diagnostics](RUNTIME_DIAGNOSTICS.md) | Startup/runtime troubleshooting, configured paths, loaded modules and Win32 126 diagnostics |

## API layers

The bridge intentionally separates three responsibilities:

1. **Interactive viewer/document layer** — `OcctEngine` and reusable UI hosts.
2. **Headless modeling layer** — `OcctModelingSession` for geometry, topology, algorithms, meshing, analysis and exchange.
3. **Pure managed utility layer** — immutable/value-oriented helpers that do not require loading OCCT.

Do not move application-level Document, Entity, Command, Tool, Undo/Redo or JSON persistence responsibilities into the reusable bridge.

## Verification levels

The repository distinguishes checks by what they can prove:

- **Static contract checks** validate file organization, C ABI/PInvoke parity, naming, counts and required documentation.
- **Managed regression tests** run without an OCCT SDK and cover ownership/value/runtime utility behavior.
- **Smoke project compilation** ensures the native integration scenarios remain source-compatible with the managed API.
- **Local Native Smoke** is the release gate that actually loads OCCT 7.9.0 and executes geometry/topology algorithms.

The responsibilities of the individual test projects and contract scripts are documented in [`tests/README.md`](../tests/README.md).

Run the cloud-equivalent managed gate:

```powershell
.\build.ps1 ci Release
```

Run the real native release gate on a Windows machine with the OCCT SDK:

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 root>"
```

## Branch responsibilities

### `main`

Reusable native/.NET bridge, WinForms/WPF hosts, tests, API documentation, and main-only NuGet production.

### `demo`

The same reusable bridge source plus CadCommon and the complete WinForms/WPF/Avalonia reference applications, run/publish scripts, and application packaging validation. The reusable projects remain non-packable on this branch.

### `website`

Static project site. Its public API statistics are validated against `main/bridge-contract.json`.

## Compatibility rule

Bridge `2.6.0` uses Native ABI `3`. New Native capabilities in this expansion are additive ABI 3 functions; existing ABI 3 signatures are not silently repurposed. Managed-only additions such as mesh provenance and structured inspection composition do not change the Native ABI. Managed callers should deploy `OcctNet`, UI host assemblies, `OcctNative.dll`, OCCT runtime DLLs, and third-party dependencies from one compatible build.
