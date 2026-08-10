# Bridge Binary SDK

`dist/win-x64` stores the validated Windows x64 binary SDK produced from `main`.

The directory is intentionally tracked by Git. It is not a normal build-output directory.

Create or refresh it only after the full Release validation succeeds:

```powershell
.\dist.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

The generated payload contains:

- `OcctNative.dll`
- `OcctNet.dll`
- `OcctNet.WinForms.dll`
- `OcctNet.Wpf.dll`
- `OcctNet.Avalonia.dll`
- `bridge-contract.json`
- `bridge-manifest.json`

`bridge-manifest.json` records the Bridge/ABI/OCCT/.NET contract, source commit, configuration, and SHA-256 hashes for the distributed files.

Consumers such as the `demo` branch should reference this binary SDK instead of compiling or copying Bridge source code.

OCCT runtime DLLs are intentionally not committed here. Consumers still resolve the OCCT 7.9.0 runtime through `OCCT_ROOT`/`CASROOT` or an explicitly configured runtime location.
