# Demo Binary SDK Dependency

The `demo` branch does not contain Bridge source code. It consumes the validated Binary SDK from `dist/win-x64`.

The payload is produced and committed on `main` by:

```powershell
.\dist.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

Then sync the exact binary payload into `demo`:

```powershell
.\sync-dist.ps1
```

`dist/win-x64` must contain `OcctNative.dll`, all `OcctNet*.dll` hosts, `bridge-contract.json`, and `bridge-manifest.json`.

These files are intentionally tracked by Git. OCCT runtime DLLs are not stored here; set `OCCT_ROOT` or `CASROOT` when running the demos.
