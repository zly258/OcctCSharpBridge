# Build, Test and Publish

Bridge 3 maintains Windows x64 and Linux x64 from the same ABI5-only source tree. `bridge-contract.json` is the machine-readable source of truth for Bridge, ABI, OCCT, .NET and platform requirements.

## 1. Prerequisites

Windows x64:

- Visual Studio 2022 / MSVC x64 C++ toolchain;
- CMake at or above the minimum declared by `bridge-contract.json`;
- OCCT **7.9.0** x64;
- a stable **.NET 10 SDK** compatible with the `10.0.100` baseline;
- C# 14 and PowerShell.

Linux x64:

- C++17 compiler and CMake;
- OCCT 7.9.0;
- a stable .NET 10 SDK compatible with the `10.0.100` baseline.

The root `global.json` and `bridge-contract.json` use `10.0.100` with `latestFeature` roll-forward and disable prerelease SDKs. Compatible stable .NET 10 feature bands and patches are allowed; the resolver must not implicitly roll to .NET 11.

Default Windows OCCT root:

```text
D:\tools\occt-vc144-64
```

## 2. Windows build.ps1

```powershell
.\build.ps1 [Target] [Configuration] [-OcctRoot <path>]
```

Targets:

| Target | Behavior |
| --- | --- |
| `validate` | repository static contract checks |
| `native` | build `OcctNative.dll` |
| `managed` | build Core, WinForms, WPF and Avalonia |
| `test` | build/run managed regression tests |
| `smoke` | Native + Managed + real Core Native Smoke |
| `viewport-smoke` | run WinForms, WPF and Avalonia native-host smoke tests |
| `dist` | produce Release `dist/win-x64` Binary SDK |
| `clean` | remove generated outputs |
| `all` | full local validation including Core and all three Viewport Host smokes; does not create `dist` |

Recommended full validation:

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

When changing adapter input/lifecycle/first-frame behavior, the focused gate is:

```powershell
.\build.ps1 viewport-smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

Generate a Windows Binary SDK:

```powershell
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

`dist` requires a clean source tree and Release configuration.

## 3. Static contract checks

Every non-`clean` Windows target runs the repository invariant checks first:

| Script | Responsibility |
| --- | --- |
| `tests/check-version-contract.ps1` | Bridge/ABI/OCCT/.NET/CMake/TFM/platform contract |
| `tests/check-architecture-boundaries.ps1` | Native/Managed and UI/Core dependency boundaries |
| `tests/check-abi5-contract.ps1` | ABI5-only; reject pre-ABI5 compatibility residue |
| `tests/check-bulk-abi.ps1` | keep bulk collections on Snapshot/Buffer ABI |
| `tests/check-native-build-structure.ps1` | Native CMake inventory and platform isolation |
| `tests/check-api-surface.ps1` | exact Native declaration/definition and Core `LibraryImport + Cdecl` parity, including additive geometry-query exports |

These checks do not replace compiler validation, managed regression tests or Native Smoke.

## 4. Managed regression and Native/Viewport Smoke

Managed-only regression project:

```text
tests/OcctNet.ManagedTests/OcctNet.ManagedTests.csproj
```

Run:

```powershell
.\build.ps1 test Release
```

Core Native Smoke:

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

Viewport Host Smoke:

```powershell
.\build.ps1 viewport-smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

The Viewport gate creates real WinForms/WPF/Avalonia native hosts and checks host state, engine generation, first-frame readiness, native handle lifecycle and core viewer operations. Avalonia also exercises `ProjectPointToEdge` / `ProjectPointToFace` parameter round trips.

For the complete Windows gate use `build.ps1 all Release`.

## 5. .NET 10 SDK resolution

Managed-dependent targets resolve `dotnet` from the repository root under the SDK contract:

```text
baseline:     10.0.100
rollForward:  latestFeature
prerelease:   disabled
```

A healthy run can therefore report, for example:

```text
SDK contract:  10.0.100 + latestFeature
SDK resolved:  10.0.302
```

or another compatible stable .NET 10 SDK. The resolved SDK must remain on major/minor `10.0` and must be at or above the baseline. If resolution fails, install a stable .NET 10 SDK or fix `DOTNET_ROOT` / `PATH`; do not weaken the contract to allow .NET 11 or prerelease SDKs.

## 6. Linux build.sh

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
./build.sh managed Release
./build.sh test Release
./build.sh all Release
./build.sh avalonia-smoke Release
./build.sh dist Release
```

Linux `managed` builds `OcctNet` and `OcctNet.Avalonia`; WinForms and WPF are Windows-only. `avalonia-smoke` requires an X11/XWayland `DISPLAY`; regular Native Smoke is headless. Linux applies the same .NET 10 baseline and roll-forward contract as Windows.

## 7. Binary SDK layout

Windows `dist/win-x64` contains:

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

Linux `dist/linux-x64` contains:

```text
libOcctNative.so
OcctNet.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

The Binary SDK manifest uses schema 2 with nested ABI5 metadata and records the exact `sourceCommit`, SDK baseline/roll-forward policy and SHA-256 for every payload file. The retired flat `nativeAbiVersion` field must not return.

## 8. Binary SDK source-control policy

`dist/win-x64` and `dist/linux-x64` are **generated artifacts, not tracked source files**. The repository keeps only `dist/README.md`; generated DLL/SO packages stay ignored by Git.

This prevents repository growth, stale source/binary mismatches and platform payload churn. Consumers must validate the package contract, manifest, source commit and hashes rather than infer freshness from Git history.

The unified `demo` branch follows the same policy and treats `dist/` as disposable local cache state. Its Windows/Linux synchronization scripts reuse a Binary SDK only when `manifest.sourceCommit`, SDK policy and all hashes match the selected SDK source revision.

## 9. Formal distribution

Windows publication validation, from a clean current `main`:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

Linux publication validation, from a clean current `main`:

```bash
./publish.sh
```

Both publish scripts are validation/artifact-generation entry points. They verify the formal branch/source revision, Release Binary SDK contract, schema-2 manifest, ABI5 metadata, SDK roll-forward metadata, source commit and SHA-256 hashes. They do **not** run `git add`, create a commit or push a branch. The Linux script must never restore the retired flat `nativeAbiVersion` check or source-controlled `dist/linux-x64` workflow.

After validation, distribute the generated binaries through a reviewed artifact channel such as GitHub Release assets or another controlled package location. Do not commit generated Binary SDK payloads to `main`, `main-dev`, `demo` or `demo-dev`. This workflow does not require GitHub Actions.

## 10. Demo consumer model

The formal `demo` / `demo-dev` branches are the single application-consumer line:

- Windows x64: WinForms, WPF and Avalonia;
- Linux x64: Avalonia only.

Demo consumes Binary SDKs produced from `main` and must not vendor Bridge Native/Core implementation source or declare the `occt_*` ABI directly. During development only, `demo-dev` may explicitly regenerate its local Windows SDK from `main-dev`; the formal default remains `main`.

## 11. Documentation and API-surface policy

`docs` contains hand-maintained architecture, usage, build, deployment and design guides only. The repository no longer generates or tracks per-type/per-function API reference pages and no longer contains an API documentation generator.

Native/managed API parity is checked directly from current source by `tests/check-api-surface.ps1`, avoiding a second generated documentation surface and hard-coded API counts that can drift from the source.
