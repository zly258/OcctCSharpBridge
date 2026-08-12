# 01 Getting Started

## Requirements

- Windows x64
- .NET SDK 10.0.302
- Visual Studio 2022 C++ x64 toolchain
- CMake 3.21 or newer
- Open CASCADE Technology 7.9.0 using the VC14 x64 layout

The default OCCT root is `D:\tools\occt-vc144-64`. Override it with `-OcctRoot` or `OCCT_ROOT`.

## Managed-only validation

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 test Release
```

These targets do not require loading the native OCCT runtime.

## Native build and smoke test

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

`all` builds `OcctNative.dll` and the four managed SDK assemblies. `smoke` runs real native modeling scenarios and verifies that `OcctRuntime` can resolve OCCT and third-party runtime dependencies.

## Binary SDK

```powershell
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

The `dist` target is Release-only. It requires a clean Git worktree, executes native build, managed build, managed tests and native smoke, and then writes the validated Binary SDK to `dist/win-x64`.

For the normal main→demo release transaction use:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

## Typical headless use

```csharp
using OcctNet;

using var model = new OcctModelingSession();
var box = model.MakeBox(100, 80, 20);
var cylinder = model.MakeCylinder(new OcctPoint3d(50, 40, -10), OcctVector3d.UnitZ, 8, 40);
var cut = model.Cut(box, cylinder);
model.ExportStep(cut.Shape, "part.step");
```

Use `OcctModelingSession` for headless geometry and topology work. Use `OcctEngine` when an AIS/Viewer context and interactive viewport are required.

## API documentation

```powershell
.\build.ps1 docs Release
```

This generates the complete bilingual Managed + Native API reference:

```text
docs/en-US/api/reference/**
docs/en-US/api/native-abi.md
docs/zh-CN/api/reference/**
docs/zh-CN/api/native-abi.md
```

The project author is **zly258**. Version, ABI, OCCT, .NET and API-count facts are defined by `bridge-contract.json`.
