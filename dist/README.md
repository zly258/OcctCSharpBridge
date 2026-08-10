# Demo Binary SDK Dependency

The `demo` branch consumes the validated OcctCSharpBridge **2.6.0** Binary SDK from `dist/win-x64` and does not contain Bridge producer source code.

Current contract:

```text
Author: Liaoyuan Zhang
Bridge: 2.6.0
Native ABI: 4
OCCT: 7.9.0
.NET SDK: 10.0.302
Target: net10.0-windows
C#: 14.0
Native Bridge: C++17
Platform: Windows x64
```

The payload is produced and published from `main` by:

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

The DLLs, Contract and Manifest are intentionally tracked by Git. OCCT runtime DLLs are not stored here; use `OCCT_ROOT`, `CASROOT`, or explicit runtime configuration when running the demos.
