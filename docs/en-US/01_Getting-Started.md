# Getting Started

The formal Bridge 3 SDK source maintains the Native Core, `OcctNet`, WinForms, WPF, and Avalonia adapters together. The source contract supports Windows x64 and Linux x64; the `demo` / `avalonia` branches are consumers and packaging examples rather than private copies of Bridge Core/Native.

## Requirements

Windows:

- Windows x64
- Visual Studio 2022 C++ toolchain
- **.NET SDK 10.0.302 exactly**
- OCCT 7.9.0
- CMake 3.21+

Default OCCT SDK path:

```text
D:\tools\occt-vc144-64
```

Linux x64 uses the same ABI5-only Core and the Avalonia managed adapter. See [Build, Test and Publish](08_Build-Test-and-Publish.md) for complete requirements and commands.

## Windows build

Recommended full validation:

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

Run individual stages as needed:

```powershell
.\build.ps1 validate Release
.\build.ps1 native Release
.\build.ps1 managed Release
.\build.ps1 test Release
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

`managed` builds `OcctNet`, `OcctNet.WinForms`, `OcctNet.Wpf`, and `OcctNet.Avalonia`. Managed-dependent targets first resolve a `dotnet.exe` that can use .NET SDK 10.0.302 exactly.

## Minimal modeling use

```csharp
using OcctNet;

using var model = new OcctModelingSession();
var box = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -5), OcctVector3d.UnitZ, 8, 20);
var result = model.Cut(box, hole);
model.ExportStep(result.Shape, "plate.step");
```

## Next steps

- Architecture: [Architecture and Boundaries](02_Architecture-and-Boundaries.md)
- API conventions: [API Coverage and Design Conventions](03_API-Coverage-and-Design-Conventions.md)
- Runtime diagnostics: [Runtime Deployment and Diagnostics](07_Runtime-Deployment-and-Diagnostics.md)
- Complete Build/Test/Publish flow: [Build, Test and Publish](08_Build-Test-and-Publish.md)
