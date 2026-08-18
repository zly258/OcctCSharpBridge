# Bridge Binary SDK

`dist/<rid>` is local generated Release output for the current **ABI5-only** source contract. It is intentionally the **minimal Binary SDK** consumed by automated validation and Demo synchronization; platform payloads are not committed to the source branches.

Windows x64:

```powershell
.\build.ps1 sdk Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux x64:

```bash
./build.sh dist Release
```

A valid minimal Binary SDK contains the platform-specialized `bridge-contract.json` plus the schema-2 `bridge-manifest.json` whose `sourceCommit` and SHA-256 entries match the generated Bridge files. The minimal `dist/<rid>` payload deliberately does **not** contain the OCCT runtime closure so that its machine-readable contract stays small and stable for Consumer/Demo synchronization.

For human distribution, use the platform `publish` entry point instead. It validates the minimal Binary SDK and then creates a **Portable SDK** under `artifacts/publish` containing:

- Bridge managed assemblies;
- `runtime/` with `OcctNative` plus the resolved OCCT/third-party native dependency closure;
- `occt/resources/` with the OCCT resources required by the Bridge;
- license/notice files;
- `package-manifest.json` with SHA-256 for the portable payload.

Windows example:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

Linux example:

```bash
./publish.sh origin
```

`demo` / `demo-dev` continue to treat `dist/` as disposable local cache state. Their synchronization scripts reuse the minimal Binary SDK only when its contract, manifest, hashes and source commit are valid; Demo application publishing currently builds its own application runtime closure.

Formal distribution should use a reviewed artifact channel such as GitHub Release assets or another controlled package location. Do not add generated Binary SDK or Portable SDK DLL/SO payloads to `main`, `main-dev`, `demo`, or `demo-dev`.

Do not copy an older ABI payload forward and do not manually edit package metadata to make an old Binary SDK appear current.
