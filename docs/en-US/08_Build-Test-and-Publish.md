# Build, Test and Publish

`main` uses PowerShell because it is the Windows x64 Bridge branch.

```powershell
.\build.ps1 validate Release
.\build.ps1 native Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 managed Release
.\build.ps1 test Release
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 docs Release
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

Static validation checks version/ABI, architecture boundaries, bulk ABI policy, Native source inventory and exact API surface parity.

`managed` builds only:

```text
OcctNet
OcctNet.WinForms
OcctNet.Wpf
```

`dist` produces the Windows Bridge SDK under `dist/win-x64`. It does not contain Avalonia.

Formal publication:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

The repository does not use a NuGet publishing pipeline or GitHub Actions for this flow. Cross-platform Avalonia builds and Linux shell scripts belong to the `avalonia` branch.