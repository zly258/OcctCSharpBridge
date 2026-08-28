# Build, Test and Publish

## Windows

```powershell
.\build.ps1 build Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 test Release
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

There are only two normal validation levels:

1. ManagedTests;
2. one minimal native/runtime smoke.

Formal Windows publishing uses:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

Publishing runs the Release gate, produces the portable SDK, and runs one isolated .NET 10 package smoke.

## Linux

```bash
./build.sh build Release
./build.sh test Release
./build.sh smoke Release
```
