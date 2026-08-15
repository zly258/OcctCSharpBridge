# Build, Test and Publish

Bridge 3 maintains Windows x64 and Linux x64 from the same ABI5-only source tree. `bridge-contract.json` is the machine-readable source of truth for Bridge, ABI, OCCT, .NET, and platform requirements.

## 1. Prerequisites

Windows x64:

- Windows x64;
- Visual Studio 2022 / MSVC x64 C++ toolchain;
- CMake at or above the minimum declared by `bridge-contract.json`;
- OCCT **7.9.0** x64;
- **.NET SDK 10.0.303 exactly**;
- C# 14;
- PowerShell.

Linux x64:

- Linux x64;
- C++17 compiler;
- CMake;
- OCCT 7.9.0;
- .NET SDK 10.0.303 exactly.

The root `global.json` disables SDK roll-forward. Do not weaken the version requirement or enable roll-forward as a workaround.

The default Windows OCCT path is:

```text
D:\tools\occt-vc144-64
```

You can also set:

```powershell
$env:OCCT_ROOT = "E:\SDK\occt-7.9.0"
```

or pass it explicitly:

```powershell
.\build.ps1 native Release -OcctRoot "E:\SDK\occt-7.9.0"
```

## 2. Windows build.ps1

Syntax:

```powershell
.\build.ps1 [Target] [Configuration] [-OcctRoot <path>]
```

Defaults:

```text
Target        = all
Configuration = Release
```

Supported configurations are `Debug`, `Release`, and `RelWithDebInfo`. `dist` is Release-only.

### Targets

| Target | Behavior | OCCT required | .NET SDK required |
| --- | --- | --- | --- |
| `validate` | Repository-level static contract checks only | No | No dotnet build |
| `native` | Static checks + configure/build `OcctNative.dll` | Yes | No |
| `managed` | Static checks + build Core, WinForms, WPF, Avalonia | No | Yes |
| `test` | Static checks + build/run managed regression tests | No | Yes |
| `smoke` | Static checks + Native + Managed + real Native Smoke | Yes | Yes |
| `dist` | Static checks + Release Native/Managed + produce `dist/win-x64` | Yes | Yes |
| `clean` | Remove CMake/MSBuild outputs; no contract checks | No | No |
| `all` | Full local validation: static checks + Native + Managed + Managed Tests + Native Smoke | Yes | Yes |

Common commands:

```powershell
# Source contract and architecture checks only
.\build.ps1 validate Release

# Native only
.\build.ps1 native Release

# Managed SDK only
.\build.ps1 managed Release

# Managed regression tests
.\build.ps1 test Release

# Real Native smoke
.\build.ps1 smoke Release

# Recommended full validation
.\build.ps1 all Release

# Produce Windows Binary SDK; clean worktree and Release are required
.\build.ps1 dist Release

# Remove generated build outputs
.\build.ps1 clean
```

Every target except `clean` runs the static contract checks first. `all` does not generate a Binary SDK and does not modify `dist`.

### all execution order

```text
Static Contract Checks
        ↓
Native CMake configure/build
        ↓
OcctNet + WinForms + WPF + Avalonia
        ↓
OcctNet.ManagedTests build/test
        ↓
OcctNet.Smoke build/run against the real Native DLL + OCCT runtime
```

## 3. Windows static contract checks

Six long-lived repository invariant checks are retained:

| Script | Responsibility |
| --- | --- |
| `tests/check-version-contract.ps1` | Bridge/ABI/OCCT/.NET/CMake/TFM/platform version contract |
| `tests/check-architecture-boundaries.ps1` | Managed/Native domain boundaries, Interop ownership, UI/Core dependency direction |
| `tests/check-abi5-contract.ps1` | ABI5-only; reject ABI4 files, handles, metadata and Binary SDK residue |
| `tests/check-bulk-abi.ps1` | Keep high-cardinality collections on Snapshot/Buffer ABI; reject N+1 indexed ABI regressions |
| `tests/check-native-build-structure.ps1` | CMake Native source inventory, platform isolation, OCCT 7.9 toolkits and domain layout |
| `tests/check-api-surface.ps1` | Exact Native declaration/definition vs Core `LibraryImport + Cdecl` parity; UI adapters may not bind `occt_*` themselves |

Run them with:

```powershell
.\build.ps1 validate Release
```

These scripts guard long-lived structural and ABI invariants; they do not replace compiler checks, managed tests, or Native Smoke.

## 4. Managed regression tests

Project:

```text
tests/OcctNet.ManagedTests/OcctNet.ManagedTests.csproj
```

It does not require the OCCT Native runtime. It primarily covers pure managed behavior, DTO/value types, ownership/identity, guards, transforms, runtime diagnostics, viewport policy, and related mappings.

Use the unified entry point:

```powershell
.\build.ps1 test Release
```

The `test` target builds the test project first and then runs tests with `--no-build`, so the test phase does not implicitly change build inputs.

## 5. Native Smoke

Project:

```text
tests/OcctNet.Smoke/OcctNet.Smoke.csproj
```

Smoke uses the just-built `OcctNative.dll` and a real OCCT 7.9.0 runtime. It covers behavior that only real Native execution can verify, including modeling, topology, mesh, exchange, critical Selection/Viewer paths, and BridgeVersion/ABI pairing.

Run:

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

For the complete validation use:

```powershell
.\build.ps1 all Release
```

## 6. Resolving .NET SDK 10.0.303

Managed-dependent targets (`managed`, `test`, `smoke`, `dist`, and `all`) resolve a concrete `dotnet.exe` that can use **10.0.303** from the repository root before the build starts. Candidate sources include `DOTNET_ROOT`, the standard Windows x64 Program Files installation, and the current `PATH`.

A successful run reports values similar to:

```text
SDK contract:  10.0.303
dotnet:        C:\Program Files\dotnet\dotnet.exe
SDK resolved:  10.0.303
```

If the `dotnet.exe` first found on `PATH` cannot see 10.0.303 but the standard x64 installation can, the script uses the working host instead of failing only after the Native build has completed.

Manual diagnostics:

```powershell
& "C:\Program Files\dotnet\dotnet.exe" --list-sdks
& "C:\Program Files\dotnet\dotnet.exe" --version
where.exe dotnet
```

`--list-sdks` must contain `10.0.303`, and `--version` executed from the repository root must return `10.0.303`. If not, install the exact SDK or fix `DOTNET_ROOT/PATH`; do not edit `global.json` to accept another SDK.

## 7. Linux build.sh

Syntax:

```bash
./build.sh [target] [configuration]
```

Supported targets:

```text
validate
native
managed
test
smoke
avalonia-smoke
dist
clean
all
```

Common commands:

```bash
./build.sh validate Release
./build.sh native Release
./build.sh managed Release
./build.sh test Release
./build.sh smoke Release
./build.sh all Release
./build.sh dist Release
```

Linux `managed` builds `OcctNet` and `OcctNet.Avalonia`; WinForms and WPF are Windows-only.

Linux Avalonia viewer smoke requires an X11/XWayland `DISPLAY`:

```bash
./build.sh avalonia-smoke Release
```

Regular `smoke` is the headless modeling/Native smoke and does not require a graphical desktop.

## 8. Binary SDK

Windows:

```powershell
.\build.ps1 dist Release
```

Output:

```text
dist/win-x64
```

Main payload:

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

Linux:

```bash
./build.sh dist Release
```

Output is `dist/linux-x64`.

The Binary SDK manifest uses schema 2 nested ABI5 metadata:

```json
"nativeAbi": {
  "current": 5,
  "minimumSupported": 5
}
```

The retired flat `nativeAbiVersion` field must not return.

## 9. Formal Windows publication validation

Formal publication validation runs only from a clean `main` synchronized with its remote:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

`publish.ps1`:

- requires the current branch to be `main`;
- requires a clean worktree based on the latest `origin/main`;
- invokes `build.ps1 dist Release`;
- verifies the ABI5 contract, manifest, source commit and SHA-256 hashes;
- ensures the process changes nothing outside `dist/win-x64`;
- does **not** run `git add`, create a commit, or push.

Publish the validated output through the normal reviewed Git workflow. This flow does not use GitHub Actions.

## 10. Documentation and API-surface policy

`docs` contains hand-maintained architecture, usage, build, deployment, and design guides only. The repository no longer generates or tracks per-type/per-function API reference pages and no longer contains an API documentation generator.

Native/managed API parity is checked directly from current source declarations, definitions, and `LibraryImport` bindings by:

```text
tests/check-api-surface.ps1
```

This avoids a second generated API-documentation surface and hard-coded API counts that can drift from the source.
