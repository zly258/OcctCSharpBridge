# Build, Test and Publish

## Windows

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 test Release
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 viewport-smoke Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

`validate` checks only version, ABI, and basic build configuration. Compilation and tests are the primary correctness gates.

There are only three normal test levels:

1. ManagedTests;
2. modeling smoke;
3. Avalonia viewer smoke.

WinForms and WPF are covered by real project builds instead of duplicate window smoke applications.

Formal Windows publishing uses:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

Publishing runs the Release gate, produces the portable SDK, and runs one isolated .NET 10 package smoke.

## Linux

```bash
./build.sh validate Release
./build.sh managed Release
./build.sh test Release
./build.sh smoke Release
./build.sh all Release
./build.sh avalonia-smoke Release
```
