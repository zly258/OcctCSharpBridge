# Demo Binary SDK Dependency

The `demo` branch consumes the validated OcctCSharpBridge Binary SDK from `dist/win-x64` and does not contain Bridge producer source code. The demo source does not pin a specific Bridge version or Native ABI; the copied Contract and Manifest define the exact SDK being consumed.

Current main baseline:

```text
Author: zly258
Bridge: 2.6.0
Native ABI: 4
OCCT: 7.9.0
.NET SDK: 10.0.302
Target: net10.0-windows
C#: 14.0
Native Bridge: C++17
Platform: Windows x64
```

## Current repository state

Until the first successful Windows release publication is completed, this branch may contain only `dist/README.md`. In that state `build.ps1 validate` intentionally stops with a clear message instead of compiling against missing or unverified Bridge binaries.

The first payload is produced and published from `main` by:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

There is no `demo/sync-dist.ps1` and no standalone `main/dist.ps1`. `main/publish.ps1` runs the Release validation gate, updates `main/dist/win-x64`, then synchronizes the exact payload to the demo branch through a temporary worktree.

`dist/win-x64` must contain:

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

The DLLs, Contract and Manifest are intentionally tracked by Git. `bridge-manifest.json` includes Author, Bridge/ABI/OCCT/.NET metadata, the source commit and SHA-256 hashes; the demo validator checks these against `bridge-contract.json` before building.

OCCT runtime DLLs are not stored here; use `OCCT_ROOT`, `CASROOT`, or explicit runtime configuration when running the demos.
