# Bridge Binary SDK

`dist/<rid>` contains only validated Binary SDK payloads produced from the current **ABI5-only** source contract.

Development branches must not retain ABI4 packages or compatibility payloads. If a development branch has no tracked platform package, generate one locally with the corresponding `build` target; formal package tracking happens only after Release validation.

Windows x64:

```powershell
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux x64:

```bash
./build.sh dist Release
```

A valid package must contain a package `bridge-contract.json` that still declares ABI 5 only, plus a `bridge-manifest.json` whose source commit and SHA-256 entries match the produced files.

Do not copy an older ABI payload forward and do not manually edit package metadata to make an old Binary SDK appear current.
