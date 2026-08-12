# Test and Contract-Check Layout

The `tests` directory intentionally contains three different verification layers. Do not remove a check only because it is implemented as a small PowerShell file; remove it only after its protection has been merged into an equivalent or stronger gate.

## 1. `OcctNet.ManagedTests`

Native-free regression tests that can run in cloud CI without an OCCT SDK. They cover managed value semantics, ownership/guard behavior, geometry utilities, runtime diagnostics, and other logic that must not require loading `OcctNative.dll`.

## 2. `OcctNet.Smoke`

Source-level integration scenarios for the real bridge. Cloud CI **compiles** this project to catch managed API drift. It does not pretend to execute OCCT because the project OCCT SDK/runtime is not available in cloud CI.

Real execution is a local Windows release gate:

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 root>"
```

Smoke scenarios are kept small and capability-oriented: topology, B-Spline, free bounds, mesh provenance, face/shape inspection, exchange, algorithms, and related native workflows.

## 3. Static contract scripts

| Script | Responsibility |
|---|---|
| `check-api-surface.ps1` | Native declaration/definition/PInvoke parity, ABI counts, public .NET type counts |
| `check-api-organization.ps1` | Public naming/organization rules and rejection of obsolete or leaking API patterns |
| `check-geometry-api.ps1` | Geometry, B-Spline, mesh-provenance, face-analysis and inspection capability contracts |
| `check-topology-analysis.ps1` | Edge adjacency and strict free-boundary contract |
| `check-native-build-structure.ps1` | Native module/CMake completeness and OCAF/XDE exclusion |
| `check-runtime-diagnostics.ps1` | Structured diagnostics plus legacy text-report compatibility |
| `check-version-contract.ps1` | Bridge/ABI/OCCT/.NET/CMake/version metadata consistency |
| `check-selection-contract.ps1` | Selection semantics and native/managed selection contract |
| `check-viewport-api.ps1` | Reusable viewport API contract |
| `check-ui-hosts.ps1` | Host-specific WinForms/WPF/Avalonia expectations where applicable |
| `check-sdk-package.ps1` | Main-branch NuGet/package metadata and package content policy |

The scripts are deliberately separated by responsibility so failures identify the broken contract instead of producing one monolithic validation script. Repeated file/token assertion plumbing lives in `ContractTestHelpers.psm1`; it is infrastructure only and is not a separate validation layer.

## Cleanup rule

A script can be removed when all of the following are true:

1. every assertion it protects has moved into another maintained check/test;
2. `build.ps1` and CI call the replacement;
3. main/demo shared-contract coverage is preserved where relevant;
4. documentation is updated so the verification boundary remains explicit.

This keeps the repository small without trading away release safety.
