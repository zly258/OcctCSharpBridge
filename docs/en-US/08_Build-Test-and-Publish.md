# Build, Test and Publish

Bridge 3.0 maintains one ABI5-only source line for Windows x64 and Linux x64, but **official prebuilt 3.x Releases target Windows x64 only**. Linux remains a maintained source-build, test, and Avalonia runtime platform.

`bridge-contract.json` is the machine-readable source of truth for version, ABI, OCCT, .NET, platform, and distribution policy.

## 1. .NET policy

Bridge 3.0 intentionally separates the **published compatibility baseline** from the **default execution runtime**:

```text
Binary SDK compatibility baseline
  Core/Avalonia: net8.0
  WinForms/WPF:   net8.0-windows

Default development/regression/smoke execution
  Core/Avalonia: net10.0
  WinForms/WPF:   net10.0-windows

Supported consumers
  .NET 8 / .NET 9 / .NET 10
```

The net8 managed assemblies remain the flat Binary SDK payload so .NET 8, 9 and 10 consumers can reference the same SDK. Routine project validation runs on .NET 10. Stable release validation separately executes Native Runtime Smoke on actual .NET 8, 9 and 10 runtimes.

Build SDK baseline: stable .NET 10 SDK `10.0.100` with `latestFeature` roll-forward.

## 2. Consumer fast path: `dist`

Windows:

```powershell
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux:

```bash
./build.sh dist Release
```

`dist` performs contract checks, builds Native + Managed, and produces the Binary SDK plus manifest/source identity/hashes. It intentionally skips the Consumer Matrix, regression tests and smoke tests.

## 3. Normal Windows validation

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

The normal complete gate covers:

```text
repository/static contracts
Native Release (/W4 /WX)
Managed Release (warnings as errors)
.NET 8/9/10 consumer compilation matrix
ManagedTests on .NET 10
Core native smoke on .NET 10
WinForms smoke on .NET 10
WPF smoke on .NET 10
Avalonia Windows smoke on .NET 10
```

`build.ps1 sdk Release` performs the same validated SDK path and produces the flat Binary SDK.

## 4. Formal Windows Stable publish

The single formal release entry point is:

```powershell
.\publish.ps1 `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -Zip
```

For a Stable contract, `publish.ps1` automatically performs:

1. Stable contract validation;
2. frozen Managed API / Native ABI compatibility validation;
3. the complete `build.ps1 sdk Release` gate;
4. Binary SDK source identity/hash validation;
5. Windows Portable SDK and ZIP generation;
6. actual .NET 8, 9 and 10 Native Runtime Smoke;
7. isolated execution from the extracted Portable ZIP after development OCCT paths/environment variables are removed.

The validation machine must have Microsoft.NETCore.App 8.x, 9.x and 10.x x64 runtimes installed. Runtime Matrix execution uses patch-only roll-forward and also verifies `Environment.Version.Major`.

`tools/validate-stable-release.ps1` is retained only as a deprecated compatibility wrapper around `publish.ps1` and is not a second release implementation.

## 5. Windows targets

| Target | Responsibility |
| --- | --- |
| `validate` | repository/contract static checks |
| `native` | build `OcctNative.dll` |
| `managed` | build published Core + UI adapter assemblies |
| `consumer` | .NET 8/9/10 consumer compilation matrix |
| `test` | Managed regression tests, default .NET 10 |
| `smoke` | Core native smoke, default .NET 10 |
| `viewport-smoke` | WinForms/WPF/Avalonia smoke, default .NET 10 |
| `dist` | fast Binary SDK, no regression/smoke |
| `sdk` | complete Windows SDK gate + Binary SDK |
| `all` | complete local validation without formal Portable packaging |
| `clean` | remove generated output |

## 6. Linux source validation

Linux is not an official prebuilt Release platform, but the source line remains buildable and runnable:

```bash
./build.sh validate Release
./build.sh all Release
```

With a graphical environment:

```bash
./build.sh avalonia-smoke Release
```

Linux regression/smoke projects also default to `net10.0`. Published Linux consumer-built Binary SDK assemblies keep the `net8.0` compatibility baseline.

`publish.sh` and `tools/package-portable-sdk.sh` remain developer/internal tools; their Linux output is not an official project Release asset and does not promise cross-distribution glibc/libstdc++ compatibility.

## 7. SDK layouts

Windows Binary SDK:

```text
dist/win-x64/
  OcctNative.dll
  OcctNet.dll
  OcctNet.WinForms.dll
  OcctNet.Wpf.dll
  OcctNet.Avalonia.dll
  bridge-contract.json
  bridge-manifest.json
```

Windows Portable SDK:

```text
OcctCSharpBridge-<version>-win-x64-portable/
  OcctNet*.dll
  bridge-contract.json
  bridge-manifest.json
  package-manifest.json
  runtime/
  occt/resources/
  licenses / notices
```

`dist/`, `build/`, and `artifacts/` are generated and must not be committed.

## 8. Formal release sequence

```text
main-dev Stable candidate
        ↓
publish.ps1 -Zip
        ↓
0 warnings / 0 test failures / 0 smoke failures
        ↓
promote the exact validated commit to main
        ↓
run publish.ps1 -Zip again on main
        ↓
tag v3.x.y
        ↓
publish Windows x64 Portable ZIP + checksum / release notes
```

No official Linux prebuilt asset is uploaded.
