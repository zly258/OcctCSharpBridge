# Demo Tests and Local Gates

The `demo` branch keeps only tests and static checks that express durable repository contracts. UI implementation details, command wording, README headings, and source-level coding patterns are not treated as build contracts.

## Real tests

### `OcctNet.ManagedTests`

Managed regression tests do not load OCCT. The project uses `MSTest.Sdk` and .NET 10 Microsoft Testing Platform. `global.json` selects `Microsoft.Testing.Platform` as the `dotnet test` runner.

Run:

```powershell
.\build.ps1 test Release
```

### `OcctNet.Smoke`

Smoke tests load the real `OcctNative.dll` and OCCT runtime. The build script places only the bridge DLL beside the smoke executable and passes the resolved `OCCT_ROOT` to the child process. `OcctRuntime` then configures the OCCT and third-party DLL search directories instead of flattening the entire native runtime into the test output directory.

Run:

```powershell
.\build.ps1 smoke Release
```

## Static checks

Six PowerShell checks remain in the normal build gate:

| Script | Responsibility |
|---|---|
| `check-version-contract.ps1` | Bridge/ABI/OCCT/.NET/CMake version contract and .NET 10 test runner |
| `check-architecture-boundaries.ps1` | Shared Bridge/UI dependency direction and main/demo branch-aware no-compatibility boundaries |
| `check-demo-structure.ps1` | Demo project/reference boundaries, non-packable policy, local tooling, and demo-only structure |
| `check-bulk-abi.ps1` | High-cardinality modeling and selection collections remain bulk-based |
| `check-native-build-structure.ps1` | CMake source inventory, OCCT 7.9 exchange toolkits, no OCAF/XDE |
| `check-api-surface.ps1` | Native declarations/definitions/PInvoke parity and API counts |

These scripts validate repository facts that compilation alone does not reliably express. Behavior belongs in managed tests or native smoke tests.

## Recommended local workflow

Managed-only verification:

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 test Release
```

Full Windows + OCCT verification:

```powershell
.\build.ps1 all Release
.\build.ps1 smoke Release
```

Use `clean` only when a stale generated output is suspected:

```powershell
.\build.ps1 clean Release
.\build.ps1 all Release
```

The repository does not use GitHub Actions as a substitute for these local gates.
