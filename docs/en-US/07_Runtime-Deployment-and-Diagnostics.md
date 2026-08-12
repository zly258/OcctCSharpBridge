# 07 Runtime, Deployment and Diagnostics

The managed SDK and the native runtime are deliberately separated.

## Binary SDK

`dist/win-x64` contains the matching Bridge set:

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

The manifest records version, ABI, target framework, source commit and SHA-256 hashes. Consumers should validate the complete set rather than copying a single `OcctNative.dll` independently.

## OCCT runtime resolution

`OcctRuntime` configures Windows DLL search paths before loading the native bridge. It understands the application/native directory plus OCCT locations derived from `OCCT_ROOT` and `CASROOT`, including standard OCCT and third-party bin folders.

## Win32 error 126

If `OcctNative.dll` exists but loading still fails, the usual cause is a missing dependent OCCT or third-party DLL. Check the resolved OCCT root, `win64/vc14/bin`, `3rdparty-vc14-64` bin directories and architecture consistency.

## Deployment models

A thin consumer can keep OCCT installed separately and provide `OCCT_ROOT`. A portable application publisher can copy the native dependency closure and required resources beside the application.

## Diagnostics

Runtime diagnostic APIs should be usable without mutating global state unexpectedly. Application-level crash/startup logs belong to the consuming application, while Bridge diagnostics report native search configuration and load failures.