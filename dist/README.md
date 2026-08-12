# Bridge Binary SDK

`dist/win-x64` stores the validated Windows x64 Binary SDK produced from `main`. The directory is intentionally tracked by Git and is **not** a normal build-output directory.

## Current contract

```text
Author: zly258
Bridge: 2.6.0
Native ABI: 4
OCCT: 7.9.0
.NET SDK: 10.0.302
Target: net10.0-windows
C#: 14.0
Native Bridge: C++17
Avalonia: 12.1.0
Platform: Windows x64
```

## Create the Binary SDK locally

Generate or refresh `dist/win-x64` only after the full Release validation gate succeeds:

```powershell
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

This performs the real Native/Managed Release build, managed regression tests and Native Smoke validation before replacing the tracked Binary SDK through staging/backup.

## Publish main and demo

For the normal release workflow use:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

`publish.ps1` first generates and commits the complete bilingual API Reference when needed, then builds/validates `dist/win-x64`, commits and pushes `main`, and synchronizes the exact Binary SDK payload to `demo` through a temporary worktree.

There is no standalone `dist.ps1` and no `demo/sync-dist.ps1`.

## Payload

The generated payload contains:

- `OcctNative.dll`
- `OcctNet.dll`
- `OcctNet.WinForms.dll`
- `OcctNet.Wpf.dll`
- `OcctNet.Avalonia.dll`
- `bridge-contract.json`
- `bridge-manifest.json`

`bridge-manifest.json` records the Bridge/ABI/OCCT/.NET contract, source commit, Release configuration and SHA-256 hashes for the distributed files. Consumers such as the `demo` branch should reference this Binary SDK instead of compiling or copying Bridge source code.

OCCT runtime DLLs are intentionally not committed here. Consumers resolve the OCCT 7.9.0 runtime through `OCCT_ROOT`, `CASROOT`, or an explicitly configured runtime location.

At present the repository may contain only this README until the first successful Windows Release publication creates `dist/win-x64`.
