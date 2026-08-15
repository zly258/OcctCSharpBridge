# Build, Test and Publish

Bridge 3 keeps Windows and Linux x64 build flows in the same ABI5-only source branch. Use PowerShell on Windows and `build.sh` on Linux.

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

Static validation checks version/ABI, architecture boundaries, bulk ABI policy, Native source inventory and exact Native/managed API surface parity.

The Windows `managed` target builds:

```text
OcctNet
OcctNet.WinForms
OcctNet.Wpf
OcctNet.Avalonia
```

The Linux managed target builds the cross-platform surfaces `OcctNet` and `OcctNet.Avalonia`; WinForms and WPF remain Windows-only.

Windows `dist` produces `dist/win-x64` with `OcctNative.dll`, core managed assembly and all three Windows-consumable UI adapters, including `OcctNet.Avalonia.dll`. Linux `dist` produces `dist/linux-x64` with `libOcctNative.so`, `OcctNet.dll` and `OcctNet.Avalonia.dll`.

Formal Windows publication from `main`:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

The publication flow validates an ABI5-only package contract and hashes every required SDK payload. The repository does not use a NuGet publishing pipeline or GitHub Actions for this flow.
