# Build, Test and Publish

Bridge 3 maintains Windows x64 and Linux x64 from the same ABI5-only source tree. `bridge-contract.json` is the machine-readable source of truth for Bridge, ABI, OCCT, .NET and platform requirements.

## 1. Prerequisites

Windows x64:

- Visual Studio 2022 / MSVC x64 C++ toolchain;
- CMake at or above the minimum declared by `bridge-contract.json`;
- OCCT **7.9.0** x64;
- a stable **.NET 10 SDK** compatible with the repository baseline (`10.0.100` or later in the 10.0 line);
- C# 14 and PowerShell.

Linux x64:

- C++17 compiler and CMake;
- OCCT 7.9.0;
- a stable .NET 10 SDK compatible with the repository baseline.

The root `global.json` uses `version: 10.0.100`, `rollForward: latestFeature` and `allowPrerelease: false`. The build therefore accepts later stable .NET 10 feature bands and patches, such as `10.0.302`, instead of requiring one exact SDK patch.

The managed Binary SDK targets **.NET 8** (`net8.0` / `net8.0-windows`) as its minimum runtime baseline. The same managed SDK is intended for .NET 8, .NET 9 and .NET 10 consumers. The repository still builds with .NET 10 because the source language contract is C# 14.

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
| `consumer` | compile Core/Avalonia and WinForms/WPF consumers for every supported .NET 8/9/10 TFM |
| `test` | build/run managed regression tests |
| `smoke` | Native + Managed + real Core Native Smoke |
| `viewport-smoke` | run WinForms, WPF and Avalonia native-host smoke tests |
| `dist` | lower-level Release Binary SDK packaging; does not run consumer/regression/smoke gates |
| `sdk` | validated Release Binary SDK: full Windows gate, then package already validated Native/Managed outputs into `dist/win-x64` |
| `clean` | remove generated outputs |
| `all` | full local validation including consumer matrix, Core Smoke and all three Viewport Host smokes; does not create `dist` |

Recommended full validation without writing a Binary SDK:

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

Focused compatibility and viewport gates:

```powershell
.\build.ps1 consumer Release
.\build.ps1 viewport-smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

Generate a **validated Windows Binary SDK**:

```powershell
.\build.ps1 sdk Release -OcctRoot "D:\tools\occt-vc144-64"
```

`sdk` is Release-only and requires a clean source tree when the Binary SDK is written. It runs a clean-tree preflight before the expensive Native/Smoke work begins and prints the exact `git status --porcelain` entries when local source/configuration changes block reproducible packaging. After the preflight it runs Native/Managed build, the .NET 8/9/10 consumer matrix, ManagedTests, Core Native Smoke and all three Viewport Host smokes before packaging. The package step reuses those validated Native/Managed outputs rather than rebuilding them, then verifies that the tree and source commit are still unchanged before writing `dist/win-x64`.

The lower-level `dist` target remains available when only packaging is needed, but it does not establish the full release gate and is not the preferred entry point for Demo source synchronization or formal Windows SDK validation.

## 3. Static contract checks

Every non-`clean` Windows target runs the repository invariant checks first:

| Script | Responsibility |
| --- | --- |
| `tests/check-version-contract.ps1` | Bridge/ABI/OCCT/.NET/CMake/TFM/platform contract and rolling SDK policy |
| `tests/check-architecture-boundaries.ps1` | Native/Managed and UI/Core dependency boundaries |
| `tests/check-abi5-contract.ps1` | ABI5-only; reject pre-ABI5 compatibility residue |
| `tests/check-bulk-abi.ps1` | keep bulk collections on Snapshot/Buffer ABI |
| `tests/check-native-build-structure.ps1` | Native CMake inventory and platform isolation |
| `tests/check-api-surface.ps1` | exact Native declaration/definition and Core `LibraryImport + Cdecl` parity |
| `tests/check-consumer-matrix.ps1` | require the matrix projects to exactly match `supportedConsumerFrameworks` and `supportedDesktopConsumerFrameworks` from `bridge-contract.json` |

These checks do not replace compiler validation, managed regression tests or Native Smoke.

## 4. Consumer compatibility, Managed regression and Native/Viewport Smoke

The compatibility projects are compile-only consumers. They do not load OCCT or create a native viewer:

```text
tests/OcctNet.ConsumerMatrix/OcctNet.ConsumerMatrix.csproj
  net8.0;net9.0;net10.0
  references OcctNet + OcctNet.Avalonia

tests/OcctNet.DesktopConsumerMatrix/OcctNet.DesktopConsumerMatrix.csproj
  net8.0-windows;net9.0-windows;net10.0-windows
  references OcctNet.WinForms + OcctNet.Wpf
```

Run the matrix directly through the repository gate:

```powershell
.\build.ps1 consumer Release
```

This is intentionally a **consumer compilation matrix**, not three Binary SDK builds. `OcctNet*` still targets the single .NET 8 baseline and produces one managed DLL set.

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

For the complete Windows validation gate use `build.ps1 all Release`. For a complete gate followed by Binary SDK production use `build.ps1 sdk Release`.

## 5. Rolling .NET 10 SDK resolution

Managed-dependent targets resolve a `dotnet` host from the repository root and validate it against the SDK baseline and roll-forward policy:

```text
SDK contract:  10.0.100 + latestFeature
SDK resolved:  <installed stable 10.0.x SDK>
```

The resolved SDK must be a stable .NET 10 SDK at or above the `10.0.100` baseline. Exact feature-band/patch equality is intentionally not required. For example, `10.0.302` satisfies the contract.

If resolution fails, install a stable .NET 10 SDK or fix `DOTNET_ROOT` / `PATH`. Do not enable prerelease SDKs as a workaround.

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

Linux `managed` builds `OcctNet` and `OcctNet.Avalonia`; WinForms and WPF are Windows-only. `avalonia-smoke` requires an X11/XWayland `DISPLAY`; regular Native Smoke is headless. The new desktop .NET 8/9/10 compilation matrix is a Windows gate because it references WinForms and WPF.

## 7. Binary SDK layout

Windows `dist/win-x64` contains exactly:

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

The Binary SDK managed assemblies target the .NET 8 baseline. The manifest uses schema 2 with nested ABI5 metadata and records the exact `sourceCommit`, SHA-256 for every payload file, the SDK baseline, and the actual `resolvedSdkVersion` used to build the package. The retired flat `nativeAbiVersion` field must not return.

## 8. Binary SDK source-control and consumer-sync policy

`dist/win-x64` and `dist/linux-x64` are **generated artifacts, not tracked source files**. The repository keeps only `dist/README.md`; generated DLL/SO packages stay ignored by Git.

`.cache/` is also ignored on both the Bridge and Demo source lines. The Demo Windows synchronizer keeps its reusable Bridge clone under `.cache/main-sdk-source/`; because the same local checkout may switch between `demo-dev` and `main-dev`, that cache must remain local build state and must never make the Bridge source tree appear dirty.

This prevents repository growth, stale source/binary mismatches and platform payload churn. Consumers must validate the package contract, manifest, source commit and hashes rather than infer freshness from Git history.

The unified `demo` branch follows the same policy and treats `dist/` as disposable local cache state. Windows source synchronization runs the Bridge `sdk` Release gate and accepts only the seven exact `win-x64` SDK files above. Extra files or directories in an externally supplied `-SdkRoot` are rejected so an old/unhashed DLL cannot be mixed into an otherwise valid manifest-controlled SDK.

A synchronized SDK is reused only when `manifest.sourceCommit`, contract compatibility and all hashes match the selected SDK source revision.

## 9. Formal distribution

Windows publication validation, from a clean current `main`:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

Linux publication validation, from a clean current `main`:

```bash
./publish.sh
```

Both publish scripts are validation/artifact-generation entry points. They verify the formal branch/source revision, Release Binary SDK contract, schema-2 manifest, ABI5 metadata, source commit and SHA-256 hashes. They do **not** run `git add`, create a commit or push a branch. The Linux script must never restore the retired flat `nativeAbiVersion` check or source-controlled `dist/linux-x64` workflow.

After validation, distribute the generated binaries through a reviewed artifact channel such as GitHub Release assets or another controlled package location. Do not commit generated Binary SDK payloads to `main`, `main-dev`, `demo` or `demo-dev`. This workflow does not require GitHub Actions.

## 10. Demo consumer model

The formal `demo` / `demo-dev` branches are the single application-consumer line:

- Windows x64: WinForms, WPF and Avalonia;
- Linux x64: Avalonia only.

Demo currently targets .NET 10 so it exercises the latest supported consumer runtime, while the Bridge Binary SDK itself targets .NET 8 and is also valid for .NET 8/9 applications. Demo validates its own `net10.0` / `net10.0-windows` targets against the Bridge contract's supported-consumer lists instead of inferring the Demo runtime path from the Bridge minimum TFM. Demo must not vendor Bridge Native/Core implementation source or declare the `occt_*` ABI directly.

During development only, `demo-dev` may explicitly regenerate its local Windows SDK from `main-dev`; the formal default remains `main`.

## 11. Documentation and API-surface policy

`docs` contains hand-maintained architecture, usage, build, deployment and design guides only. The repository no longer generates or tracks per-type/per-function API reference pages and no longer contains an API documentation generator.

Native/managed API parity is checked directly from current source by `tests/check-api-surface.ps1`, avoiding a second generated documentation surface and hard-coded API counts that can drift from the source.
