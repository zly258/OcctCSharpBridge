# Bridge Binary SDK

`dist/<rid>` is local generated Release output for the current **ABI5-only** source contract. Platform Binary SDK payloads are intentionally not committed to the source branches.

Windows x64:

```powershell
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux x64:

```bash
./build.sh dist Release
```

A valid package contains a platform-specialized `bridge-contract.json` that remains ABI5-only plus a schema-2 `bridge-manifest.json` whose `sourceCommit` and SHA-256 entries match the generated files.

`demo` / `demo-dev` treat `dist/` as disposable local cache state. Their synchronization scripts reuse a package only when its contract, manifest, hashes and source commit are valid.

Formal binary distribution should use a reviewed artifact channel such as GitHub Release assets or another controlled package location. Do not add generated Binary SDK DLL/SO payloads to `main`, `main-dev`, `demo`, or `demo-dev`.

Do not copy an older ABI payload forward and do not manually edit package metadata to make an old Binary SDK appear current.
