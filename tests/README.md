# Tests

OcctCSharpBridge uses three test levels:

1. **Managed tests** — pure .NET behavior and ABI layout checks that do not require OCCT runtime loading.
2. **Modeling smoke** — one real Native Bridge + OCCT scenario set.
3. **Viewer smoke** — one Avalonia lifecycle/render scenario. WinForms and WPF are compile-validated by their projects instead of maintaining duplicate GUI smoke applications.

## Windows

```powershell
.\build.ps1 test Release
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 viewport-smoke Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

## Linux

```bash
./build.sh test Release
./build.sh smoke Release
./build.sh avalonia-smoke Release   # graphical environment only
./build.sh all Release
```

`validate` checks only the repository version/ABI/framework contract. Source compilation and tests are the primary correctness gates; implementation details are not enforced through additional source-scanning policy scripts.

`tests/OcctNet.RuntimeSmoke` is reserved for one isolated Stable package check on .NET 10 and is not part of normal development validation.
