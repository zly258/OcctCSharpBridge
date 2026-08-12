# Bridge Binary SDK

`main/dist/win-x64` stores the **last validated Windows x64 Binary SDK actually published from `main`**. It is intentionally tracked and is not ordinary build output.

## Current published payload

```text
Bridge: 2.6.0
Native ABI: 4
Native exports / PInvoke: 347 / 347
Public .NET types: 110
Viewer / Modeling API: 213 / 134
OCCT: 7.9.0
.NET SDK: 10.0.302
Target: net10.0-windows
C#: 14.0
Platform: Windows x64
Source commit: 960b6b0b0b4cfcbc16af4fb91bf57b8ec146446f
```

The current **source** contract in `../bridge-contract.json` is already **Bridge 2.7.0 / ABI 4 / 349 / 349 / 117 / Viewer 215 / Modeling 134**. Those source changes have not yet been republished into the tracked DLL payload.

Do not manually edit the Binary SDK JSON or replace individual DLLs. Publish the current source on the normal Windows/MSVC + OCCT 7.9 workstation:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

`publish.ps1` generates the bilingual API reference, builds Release, validates contract/manifest/SHA-256, commits the new `dist/win-x64`, and pushes `main`.

## Demo consumption

`demo/dist` is ignored. Demo users copy whichever SDK is actually published on `main`:

```powershell
.\sync.ps1
```

`sync.ps1` prints the synchronized contract so a version lag cannot be hidden.

## License

Non-commercial use is free under the PolyForm Noncommercial License 1.0.0, subject to its terms. Commercial use requires separate authorization from the author; see `../COMMERCIAL.md`.
