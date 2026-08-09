# Test and Contract-Check Layout

The `tests` directory has three verification layers. A small PowerShell file is not redundant merely because it is short; remove a check only when its contract is covered by an equivalent or stronger maintained gate.

## 1. `OcctNet.ManagedTests`

Native-free regression tests that run in cloud CI without an OCCT SDK. They cover:

- managed value semantics and owner/guard behavior;
- pure managed geometry/transform helpers;
- runtime diagnostics that do not force Native loading;
- host-neutral viewport interaction policy;
- **public managed API signature snapshot** across `OcctNet`, WinForms, WPF, and Avalonia.

`PublicApi.approved.txt` is an intentional compatibility baseline. If a public constructor, method, parameter/default value, property, event, field/enum value, base type, or interface changes, the generated snapshot differs. Update the baseline only after reviewing that the API change is intentional. This complements count-based checks: a method signature can change while the number of methods/types remains unchanged.

The snapshot includes the separately tracked Bridge 2.5 compatibility surface as well as the primary 2.6 API. Normal CI always compares against the committed baseline. The `OCCT_UPDATE_PUBLIC_API_SNAPSHOT` environment variable exists only for an explicit review/approval workflow that writes a replacement baseline; it is not enabled by `build.ps1` during ordinary validation.

## 2. `OcctNet.Smoke`

Source-level integration scenarios for the real bridge. Cloud CI **compiles** this project to catch managed API drift, but does not claim real OCCT execution because the repository's OCCT SDK/runtime is unavailable there.

Real execution is a local Windows release gate:

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 root>"
```

When OCCT is installed at the conventional `D:\tools\occt-vc144-64` root, `-OcctRoot` may be omitted.

Smoke scenarios remain capability-oriented: ABI/version loading, topology, B-Spline, free bounds, mesh provenance, Face/Shape inspection, exchange, algorithms, and related native workflows.

## 3. Static contract scripts

| Script | Responsibility |
|---|---|
| `check-api-surface.ps1` | Native declaration/definition/PInvoke parity, ABI counts, primary/compatibility public .NET type counts |
| `check-api-organization.ps1` | Public naming/organization rules and rejection of obsolete/leaking API patterns |
| `check-geometry-api.ps1` | Geometry, B-Spline, mesh provenance, Face analysis and inspection capability contracts |
| `check-topology-analysis.ps1` | Edge adjacency and strict free-boundary contract |
| `check-native-build-structure.ps1` | Native CMake/module completeness, narrow modeling-internal boundaries, OCCT 7.9 toolkit policy, OCAF/XDE exclusion |
| `check-runtime-diagnostics.ps1` | Structured diagnostics plus legacy text-report compatibility |
| `check-version-contract.ps1` | Bridge/ABI/OCCT/.NET/CMake/version metadata and shared main/demo build-SDK consistency |
| `check-selection-contract.ps1` | Selection semantics and native/managed selection contract |
| `check-viewport-api.ps1` | Reusable viewer/viewport API contract |
| `check-ui-hosts.ps1` | WinForms/WPF/Windows-HWND Avalonia host boundaries and core UI-framework independence |
| `check-sdk-package.ps1` | `main`-only four-package NuGet metadata/content policy |

`ContractTestHelpers.psm1` contains repeated assertion plumbing only; it is infrastructure, not another verification layer.

## What cloud CI proves

Cloud CI can prove source/API/package consistency and Managed build behavior. It **cannot** prove that changed C++ code compiles and links against the user's local OCCT 7.9.0 SDK or that real OCCT algorithms execute correctly.

For Native changes, the final release evidence is therefore:

```powershell
.\build.ps1 smoke Release
```

on a Windows x64 machine with the contracted OCCT SDK.

## Cleanup rule

A script/test can be removed only when all of the following are true:

1. every distinct assertion it protects has moved into another maintained check/test;
2. `build.ps1` and CI invoke the replacement;
3. main/demo shared-contract coverage is preserved where applicable;
4. documentation is updated so the verification boundary remains explicit.

This keeps the repository organized without trading away release safety.
