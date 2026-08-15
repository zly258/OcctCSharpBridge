# Build, Test and Publish

Bridge 3 keeps Windows and Linux x64 build flows in the same ABI5-only source tree. The source contract requires **.NET SDK 10.0.302 exactly**; `global.json` disables SDK roll-forward. Use PowerShell on Windows and `build.sh` on Linux.

Windows x64:

```powershell
.\build.ps1 validate Release
.\build.ps1 native Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 managed Release
.\build.ps1 test Release
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 docs Release
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux x64:

```bash
./build.sh validate Release
./build.sh managed Release
./build.sh native Release
./build.sh test Release
./build.sh smoke Release
./build.sh dist Release
```

Static validation checks the exact Bridge/.NET/OCCT/ABI contract, architecture boundaries, bulk snapshot/buffer policy, Native source inventory and exact Native/managed API-surface parity. ABI 4 compatibility files, handles, exports, manifests and consumer tests are not part of Bridge 3.

The Windows `managed` target builds:

```text
OcctNet
OcctNet.WinForms
OcctNet.Wpf
OcctNet.Avalonia
```

The Linux managed target builds the cross-platform surfaces `OcctNet` and `OcctNet.Avalonia`; WinForms and WPF remain Windows-only.

Windows `dist` produces `dist/win-x64` with `OcctNative.dll`, the core managed assembly and all three Windows-consumable UI adapters, including `OcctNet.Avalonia.dll`. Linux `dist` produces `dist/linux-x64` with `libOcctNative.so`, `OcctNet.dll` and `OcctNet.Avalonia.dll`.

Both Binary SDKs carry the Bridge 3 schema-3 `bridge-contract.json` and a schema-2 `bridge-manifest.json`. The manifest uses nested ABI5 metadata:

```json
"nativeAbi": {
  "current": 5,
  "minimumSupported": 5
}
```

The retired flat `nativeAbiVersion` field is forbidden.

## Formal Windows publication validation

Run from a clean, synchronized formal `main` branch:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

`publish.ps1` is intentionally **validation-only**. It verifies generated API-documentation freshness, builds the Release Binary SDK, checks the ABI5 contract/manifest, exact source commit and SHA-256 hashes, and ensures no unexpected files are changed outside `dist/win-x64`.

It does **not** run `git add`, create commits, or push `main`. Review and publish generated files through the normal reviewed Git workflow. The repository does not use GitHub Actions or a NuGet publishing pipeline for this flow.
