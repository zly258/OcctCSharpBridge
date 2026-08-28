# Tests

OcctCSharpBridge uses two validation levels:

1. **Managed tests** — pure .NET behavior and ABI layout checks that do not require OCCT runtime loading.
2. **Native/runtime smoke** — native loading, ABI/version agreement, runtime initialization, and one real OCCT modeling operation.

## Windows

```powershell
.\build.ps1 test Release
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 build Release -OcctRoot "D:\tools\occt-vc144-64"
```

## Linux

```bash
./build.sh test Release
./build.sh smoke Release
./build.sh build Release
./build.sh build Release
```

Publishing reuses `OcctNet.Smoke` for its isolated portable-package check; there is no second package-smoke project.
