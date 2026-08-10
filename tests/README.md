# Tests and Contract Checks

The `tests` directory contains managed regression tests, native smoke scenarios, and static repository contracts. Validation is designed to be run locally; the authoritative native gate always uses a real Windows x64 OCCT 7.9.0 SDK.

## Managed regression tests

`OcctNet.ManagedTests` does not load OCCT. It validates:

- managed value semantics and owner-aware handles;
- guard behavior and pure managed helpers;
- runtime diagnostics that do not force native loading;
- viewport interaction policy;
- P0 inertia DTO mapping;
- P1 structured Edge/Edge intersection DTO mapping;
- P2 topology-reference DTO/result mapping.

The project does not keep a frozen legacy API snapshot. The library is treated as a clean new API; current public type counts and Native/PInvoke parity are enforced by repository contracts instead.

## Native smoke tests

`OcctNet.Smoke` covers real bridge loading and OCCT operations. Run it on Windows with the contracted SDK:

```powershell
.\build.ps1 smoke Release
```

For a non-default OCCT installation:

```powershell
.\build.ps1 smoke Release -OcctRoot "E:\SDK\occt-7.9.0"
```

## Static contract scripts

| Script | Responsibility |
|---|---|
| `check-api-surface.ps1` | Native declaration/definition/PInvoke parity and current public API counts |
| `check-api-organization.ps1` | Clean owner-aware organization, P0–P3 files and no compatibility layer |
| `check-modeling-bulk-abi.ps1` | Bulk-only modeling collection ABI |
| `check-geometry-api.ps1` | Geometry, B-Spline, mesh provenance, Face analysis and inspection contracts |
| `check-topology-analysis.ps1` | Edge adjacency and free-boundary contracts |
| `check-native-build-structure.ps1` | Native module/CMake layout, P0–P3 modules, OCCT toolkit policy and no-OCAF/XDE boundary |
| `check-runtime-diagnostics.ps1` | Structured runtime diagnostics |
| `check-version-contract.ps1` | Bridge/ABI/OCCT/.NET/CMake/API metadata |
| `check-selection-contract.ps1` | Viewer selection and structured hit semantics |
| `check-viewport-api.ps1` | Reusable viewer/viewport API contract |
| `check-ui-hosts.ps1` | WinForms/WPF/Avalonia host boundaries |
| `check-sdk-package.ps1` | Managed package metadata and content policy |

## Recommended local validation

Without an OCCT SDK:

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
```

With OCCT 7.9.0:

```powershell
.\build.ps1 smoke Release
```

A check should only be removed when its distinct contract is either no longer part of the new library or is covered by a simpler maintained check.
