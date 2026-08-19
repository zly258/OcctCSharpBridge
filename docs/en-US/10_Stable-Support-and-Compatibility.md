# 3.x Stable Support and Compatibility

This document defines the long-term compatibility boundaries for OcctCSharpBridge 3.x consumers. Machine-readable version, ABI, OCCT, .NET and platform facts remain in `bridge-contract.json`.

## 1. Platform support

### Windows x64

Windows x64 is the **official prebuilt release platform** for 3.x. Formal Releases provide a Windows Portable SDK validated by Native/Managed builds, default .NET 10 regression/smoke execution, the .NET 8/9/10 Runtime Matrix, UI host smoke, and isolated Portable SDK smoke.

### Linux x64

Linux x64 is a **source-build platform**. Native Core, `OcctNet`, `OcctNet.Avalonia`, build scripts, headless smoke and display-dependent Avalonia smoke remain maintained.

Official 3.x Releases do not promise Linux prebuilt Binary/Portable assets. Linux consumers build against OCCT 7.9.0 and the system ABI/runtime baseline appropriate for their deployment environment.

## 2. .NET support

Published managed assembly compatibility baseline:

```text
Core/Avalonia: net8.0
WinForms/WPF:   net8.0-windows
```

Default repository execution:

```text
ManagedTests/Core/Avalonia smoke: net10.0
WinForms/WPF smoke:                net10.0-windows
```

Supported consumer runtimes:

```text
.NET 8
.NET 9
.NET 10
```

The net8 TFM is intentionally the minimum Binary SDK baseline; it is not the default development/runtime choice. Routine validation runs on .NET 10. Stable publishing additionally requires actual Microsoft.NETCore.App 8.x, 9.x and 10.x runtimes and executes a native-backed smoke on each major.

## 3. Native ABI

Bridge 3.x supports ABI 5 only:

```text
current = 5
minimumSupported = 5
```

Stable rules:

- released ABI 5 exports are not removed;
- parameters, calling convention, return types and established semantics are not changed incompatibly;
- existing fields in released ABI structs keep compatible layout;
- extensible structs continue to use their `structSize` / `apiVersion` contract;
- new capabilities prefer additive entry points;
- ABI-breaking work requires a new major/ABI strategy.

Managed and Native payloads must come from the same Bridge build.

## 4. Managed API compatibility

Within 3.x:

- patch releases focus on bug fixes, stability, performance, diagnostics and non-breaking improvements;
- released public types/members are not removed incompatibly;
- additive APIs are allowed;
- changes requiring incompatible public semantics belong in a new major-version evaluation.

Internal source layout and private implementation details are not third-party compatibility contracts.

## 5. Threading

`OcctRuntime.Configure()` should run before the first native-backed object is created.

A single `OcctModelingSession` and its resources are not concurrent thread-safe by default; applications must serialize access to one Session.

A single `OcctEngine` and its Viewer/Scene objects are not concurrent thread-safe by default. WinForms/WPF/Avalonia hosts follow the UI-thread rules of their framework. The Bridge does not automatically marshal background calls to the UI thread.

## 6. Ownership and lifetime

- `OcctEngine` owns Viewer/Scene object IDs; IDs from different Engines must not be mixed.
- `OcctModelingSession` owns normal modeling Shape/Operation IDs and related resources; IDs from different Sessions must not be mixed.
- managed `IDisposable`/owned resources must be disposed according to their API contract.
- host recreation invalidates temporary native bindings; consumers should rebuild them from host lifecycle/generation state instead of caching stale handles.

## 7. Units, coordinates and tolerance

Ordinary Modeling/Viewer values use the application's consistent unit convention; the Bridge does not silently convert project units for normal modeling calls.

STEP/IGES unit behavior depends on translator/file metadata and should be validated at exchange boundaries when strict project-unit consistency is required.

The Bridge uses OCCT Cartesian geometry semantics. Engineering coordinate systems/project origins are application responsibilities.

Tolerance follows OCCT precision/topology semantics unless a public API explicitly exposes a tolerance parameter.

## 8. Binary and Portable SDK upgrades

Upgrade the payload as one build:

```text
OcctNet*.dll
runtime/
occt/resources/
bridge-contract.json
bridge-manifest.json
package-manifest.json
```

Do not mix versions or `sourceCommit` values.

## 9. Stable release gate

Formal Windows Stable publishing uses:

```powershell
.\publish.ps1 `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -Zip
```

For a Stable contract this performs the normal .NET 10 regression/smoke gate, Stable API/ABI baseline checks, actual .NET 8/9/10 Native Runtime Matrix execution, Portable SDK packaging and isolated execution after development OCCT paths are removed.

`tools/validate-stable-release.ps1` remains only as a deprecated compatibility wrapper around `publish.ps1`.

## 10. Linux validation boundary

Linux does not receive an official prebuilt asset, but the source line should continue to pass:

```bash
./build.sh validate Release
./build.sh all Release
```

With a graphical environment:

```bash
./build.sh avalonia-smoke Release
```

Linux regression/smoke execution defaults to .NET 10. Distribution ABI and deployment policy remain the source consumer's responsibility for the intended target environment.
