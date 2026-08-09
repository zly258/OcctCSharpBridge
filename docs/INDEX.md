# OcctCSharpBridge Documentation

This index covers the reusable **OCCT bridge** documentation shared by `main` and `demo`. NuGet production belongs to `main`; complete CAD applications, CadCommon, and run/publish workflows belong to `demo`.

## Start here

| Document | Use it for |
|---|---|
| [Architecture Boundaries](ARCHITECTURE_BOUNDARIES.md) | `main`/`demo` responsibilities, why Document/Command/Tool stay outside the bridge, and UI-host sharing rules |
| [API Coverage](API_COVERAGE.md) | Current Bridge/ABI/API scope, ownership rules, façade responsibilities, validation boundaries |
| [Getting Started](GETTING_STARTED.md) | Core and WinForms/WPF/Avalonia host references and runtime deployment |
| [Packaging](PACKAGING.md) | main-only NuGet packages, package contents, and native-runtime boundary |
| [Structured Viewer Selection Hits](SELECTION_HITS.md) | Registered object/subshape identity for selected and detected AIS entities |
| [Managed Geometry and Transform Utilities](GEOMETRY_UTILITIES.md) | Point/vector math, bounds, UV ranges, affine matrices, locations and transforms |
| [B-Spline Curve and Surface Inspection](BSPLINE_CURVES.md) | Degree, poles, weights, knots, multiplicities and surface control grids |
| [Topology Adjacency and Free-Boundary Analysis](TOPOLOGY_ANALYSIS.md) | Batched adjacency, manifold/non-manifold checks and strict free-boundary analysis |
| [Batch Face Analysis and Shape Inspection](SHAPE_INSPECTION.md) | Batched Face metadata and structured model-audit snapshots without application-specific pass/fail rules |
| [Shape Mesh Face Provenance](MESH_PROVENANCE.md) | Combined-mesh source-Face ranges, picking and CAD/BIM property mapping |
| [Structured Runtime Diagnostics](RUNTIME_DIAGNOSTICS.md) | Startup/runtime troubleshooting, configured paths, loaded modules and Win32 126 diagnostics |

## API layers

The bridge intentionally separates three responsibilities:

1. **Interactive Viewer/Object layer** — `OcctEngine`.
2. **Headless modeling layer** — `OcctModelingSession` for geometry, topology, algorithms, meshing, analysis and exchange.
3. **Reusable UI-host layer** — WinForms, WPF, and Windows-HWND Avalonia adapters that connect framework windows/input to `OcctEngine`.

Application Document, Entity, Feature Tree, Command, Tool, Undo/Redo, Snap/Grip, and JSON persistence responsibilities do not belong in the reusable bridge.

## Verification levels

- **Static contract checks** validate file organization, C ABI/PInvoke parity, naming, counts, branch boundaries, and documentation.
- **Managed regression tests** run without loading OCCT and cover ownership, value/runtime utilities, host-neutral interaction rules, and a public managed API signature snapshot.
- **Smoke project compilation** keeps real native integration scenarios source-compatible with the managed API.
- **Local Native Smoke** actually loads OCCT 7.9.0 and executes geometry/topology algorithms and remains the release gate.

See [`tests/README.md`](../tests/README.md) for individual test responsibilities.

```powershell
.\build.ps1 ci Release
```

Real native release gate:

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 root>"
```

## Branch responsibilities

### `main`

Reusable native/.NET bridge, `OcctNet.WinForms`, `OcctNet.Wpf`, `OcctNet.Avalonia`, tests, API documentation, and main-only NuGet production. It must not contain CadCommon, complete CAD applications, or application Document/Command/Tool frameworks.

### `demo`

The same reusable bridge source plus `CadCommon`, complete WinForms/WPF/Avalonia reference applications, run/publish scripts, and application packaging validation. Reusable projects remain non-packable on this branch.

### `website`

Static project site. Public API statistics are validated against `main/bridge-contract.json`.

## Compatibility rule

Bridge `2.6.0` uses Native ABI `3`. Internal cpp/header reorganization does not change existing ABI 3 signatures. Bridge 2.5's `OcctObject` compatibility type remains during 2.x but receives no new legacy surface; new code uses owner-aware object APIs. Deploy `OcctNet`, the selected UI host, `OcctNative.dll`, OCCT runtime DLLs, and third-party dependencies from one compatible build.
