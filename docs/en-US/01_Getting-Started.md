# Getting Started

The `main` branch is the Windows x64 edition of OcctCSharpBridge. It contains the Core plus independent WinForms and WPF hosts; Avalonia is maintained on the standalone `avalonia` branch.

## Requirements

- Windows x64
- Visual Studio 2022 C++ toolchain
- .NET SDK 10.0.302
- OCCT 7.9.0
- CMake 3.21+

Default OCCT SDK path:

```text
D:\tools\occt-vc144-64
```

## Build

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 test Release
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

## Minimal modeling use

```csharp
using OcctNet;

using var model = new OcctModelingSession();
var box = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -5), OcctVector3d.UnitZ, 8, 20);
var result = model.Cut(box, hole);
model.ExportStep(result.Shape, "plate.step");
```

For cross-platform Avalonia applications, switch to `avalonia`; there is no sync step between the two source branches.