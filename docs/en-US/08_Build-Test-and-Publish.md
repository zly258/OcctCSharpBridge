# Build, Test and Publish

Bridge 3 maintains Windows x64 and Linux x64 from one ABI5-only source contract. This guide deliberately separates **consumer artifact production** from **Bridge quality validation/publication** so downstream projects do not pay the cost of the full QA gate whenever they refresh an SDK.

`bridge-contract.json` is the machine-readable source of truth for Bridge version, ABI, OCCT, .NET, C# and supported consumer frameworks.

## 1. Toolchain contract

Windows x64:

- Visual Studio 2022 / MSVC x64 C++ toolchain
- CMake at or above the contract minimum
- OCCT 7.9.0 x64
- stable .NET 10 SDK selected from baseline `10.0.100` with `latestFeature` roll-forward
- PowerShell

Linux x64:

- C++17 compiler and CMake
- OCCT 7.9.0
- stable .NET 10 SDK compatible with the same baseline
- standard ELF tooling for Portable SDK packaging (`ldd`, `realpath`, `patchelf`, `sha256sum`, `python3`)

The managed Binary SDK targets .NET 8 (`net8.0` / `net8.0-windows`) as the minimum runtime baseline. The same managed assemblies are intended for .NET 8, .NET 9 and .NET 10 consumers. A stable later .NET 10 SDK such as `10.0.302` is valid; exact `10.0.303` is not required.

## 2. Two production levels

### 2.1 Consumer Artifact Fast Path — `dist`

Use `dist` when the caller needs a current Binary SDK from a known source revision but does **not** need to re-certify Bridge itself.

Windows:

```powershell
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux:

```bash
./build.sh dist Release
```

The fast path performs the static/source checks required by the build script, compiles Native + Managed, creates the platform-specialized contract/manifest, records the exact source commit, and hashes the payload.

It intentionally skips:

- .NET 8/9/10 Consumer Matrix;
- Managed Regression Tests;
- Core Native Smoke;
- WinForms/WPF/Avalonia Viewport Smoke;
- Linux Avalonia graphical smoke.

This is the correct path for Demo SDK refreshes and controlled internal/third-party source builds.

### 2.2 Full Bridge Gate — `sdk`, `all`, `publish`

Use the full gate when validating a Bridge candidate or producing a formal distribution.

Windows validated Binary SDK:

```powershell
.\build.ps1 sdk Release -OcctRoot "D:\tools\occt-vc144-64"
```

Windows publication:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

Linux headless validation/publication:

```bash
./build.sh all Release
./publish.sh
```

The full Windows gate includes Native + Managed, Consumer Matrix, ManagedTests, Core Native Smoke, and all three viewport-host smokes. The formal publish path then creates/validates the Binary SDK and Portable SDK. Linux formal publication runs the headless test/smoke gate; `avalonia-smoke` remains a separate display-dependent test.

**Rule:** a downstream consumer refreshing an SDK must not invoke the full Bridge gate unless it is explicitly acting as a Bridge release validator.

## 3. Windows build.ps1 targets

```powershell
.\build.ps1 [Target] [Configuration] [-OcctRoot <path>]
```

| Target | Purpose |
| --- | --- |
| `validate` | static repository/contract checks |
| `native` | build `OcctNative.dll` |
| `managed` | build Core, WinForms, WPF and Avalonia managed assemblies |
| `consumer` | compile supported .NET 8/9/10 consumer matrix |
| `test` | managed regression tests |
| `smoke` | Core native-backed smoke scenarios |
| `viewport-smoke` | WinForms/WPF/Avalonia native-host smoke tests |
| `dist` | **fast consumer Binary SDK**; Native + Managed + package, no regression/smoke gate |
| `sdk` | **validated Release Binary SDK**; full Windows gate, then package validated outputs |
| `clean` | remove generated outputs |
| `all` | complete local validation without writing the Binary SDK |

`dist` and `sdk` are Release-only.

## 4. Linux build.sh targets

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

Linux `dist` builds Core + Avalonia managed assemblies and `libOcctNative.so`, then writes the minimal linux-x64 SDK. It does not run tests or smokes. `all` runs managed regression and headless Core Native Smoke. `avalonia-smoke` requires an X11/XWayland `DISPLAY` and is not part of ordinary consumer synchronization.

## 5. Static contract checks

Windows targets run repository invariants covering version/TFM policy, architecture boundaries, ABI5-only rules, bulk ABI shape, native build inventory, API declaration/binding parity and the consumer-framework matrix.

These checks are deliberately cheap relative to real native/UI smoke tests. They are not a substitute for the full release gate, but they remain appropriate when generating a deterministic consumer artifact.

## 6. Consumer compatibility matrix

Bridge managed assemblies target one .NET 8 baseline. The repository verifies that this single DLL set can be referenced by:

```text
Core/Avalonia: net8.0; net9.0; net10.0
WinForms/WPF:  net8.0-windows; net9.0-windows; net10.0-windows
```

The matrix is a Bridge QA activity. A Demo or third-party consumer does not need to rebuild the matrix every time it refreshes an already selected Bridge source commit.

## 7. Minimal Binary SDK layout

Windows `dist/win-x64`:

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

Linux `dist/linux-x64`:

```text
libOcctNative.so
OcctNet.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

The manifest records:

- schema version;
- author and Bridge version;
- ABI `current` / `minimumSupported`;
- OCCT version;
- platform and managed target framework;
- SDK baseline and actual resolved SDK;
- exact `sourceCommit`;
- SHA-256 for every payload file.

The minimal SDK is deliberately small and does not include the OCCT native runtime closure.

## 8. Generated-artifact policy

The entire `dist/` directory is generated state and is ignored by Git. It may be deleted and regenerated at any time. `.cache/`, `build/` and `artifacts/` are also local build state.

Do not commit generated SDK DLL/SO payloads to `main`, `main-dev`, `demo`, or `demo-dev`. Formal external distribution belongs in a reviewed artifact channel such as a release asset or another controlled package store.

## 9. Demo consumer synchronization

The Demo is a real Binary SDK consumer and must not act as a second Bridge release pipeline.

On a cache hit, Demo sync validates the local Binary SDK and Portable SDK against the selected source commit and hashes, then returns without compiling Bridge.

On a cache miss, Demo sync now follows this path:

```text
resolve origin/main or origin/main-dev sourceCommit
        ↓
Bridge dist Release
  Native + Managed + Binary SDK only
        ↓
Bridge-owned Portable SDK packager
        ↓
validate contract / sourceCommit / hashes
        ↓
copy into Demo dist cache
```

It does **not** call Bridge `sdk`, `all`, ManagedTests, Core Smoke, or viewport/window smokes.

When a formal Bridge publish has already produced matching Binary and Portable SDK directories, Demo can skip Bridge compilation entirely and consume them directly through its `-SdkRoot` / `-PortableRoot` (Windows) or `--sdk-root` / `--portable-root` (Linux) options.

## 10. Formal publication

`publish.ps1` / `publish.sh` are the controlled publication entry points for `main-dev` candidate validation and `main` formal artifacts. They require a clean source tree and validate branch ancestry against the matching remote branch.

Recommended promotion model:

```text
main-dev candidate
  ↓ full publish gate
validated exact commit
  ↓ fast-forward only
main
  ↓ formal publish
reviewed release artifacts
```

Do not alter source while promoting an already validated candidate.

## 11. Third-party consumer policy

A third-party project should normally consume a reviewed SDK generated from `main`, not `main-dev`.

Preferred artifact selection:

- Binary SDK: compile-time reference and CI metadata validation;
- Portable SDK: deployment-time Native/OCCT closure and resources.

Third-party projects should validate platform, ABI, Bridge version/source identity and hashes before accepting an SDK, and upgrade Managed + Native + runtime/resources as one coherent unit. Complete examples are in [Third-party SDK Consumption](09_Third-Party-SDK-Consumption.md).

## 12. Linux distribution compatibility

Linux Portable SDK packaging does not make glibc/libstdc++ ABI requirements disappear. The native Bridge and OCCT must be built against an ABI baseline no newer than the oldest Linux distribution the project intends to support. AppImage packaging does not repair a binary already linked against a newer `GLIBC_*`, `GLIBCXX_*`, or `CXXABI_*` baseline.

See [Runtime Deployment and Diagnostics](07_Runtime-Deployment-and-Diagnostics.md) for runtime closure and ABI diagnostics.

## 13. Documentation/API-surface policy

The repository maintains hand-written architecture, integration, build, deployment and migration guides. It does not generate per-type/per-method API reference pages. Native/managed API parity remains a source-level validation responsibility.
