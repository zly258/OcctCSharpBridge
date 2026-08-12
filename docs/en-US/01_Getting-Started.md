# 01 Getting Started

## Requirements

- Windows 10/11 x64
- .NET SDK 10.0.302
- CMake 3.21+ and a compatible MSVC toolchain for building the Native Bridge
- Open CASCADE Technology 7.9.0 runtime/toolkit

## Source and published SDK

The current source contract is **Bridge 2.7.0 / ABI 4**. The authoritative published Binary SDK is always the tracked `main/dist/win-x64` payload; read `dist/win-x64/bridge-contract.json` for its actual version and API contract instead of relying on a duplicated hard-coded release value in documentation.

## Build the Bridge from source

```powershell
.\build.ps1 validate Release
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

## Headless modeling

```csharp
using OcctNet;

using var model = new OcctModelingSession();
var box = model.MakeBox(100, 80, 20);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -5), OcctVector3d.UnitZ, 10, 30);
var result = model.Cut(box, hole);
model.ExportStep(result.Shape, "result.step");
```

## 2.7 source: STEP assembly import

```csharp
using var engine = new OcctEngine();
OcctAssemblyDocument doc = engine.ImportStepDocument("assembly.step");
```

Use `ImportStep()` for the legacy shape-oriented path and `ImportStepDocument()` when real assembly hierarchy, instances, transforms and styles matter.

## Publish the Binary SDK

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

Then, on the demo branch:

```powershell
git switch demo
.\sync.ps1
.\build.ps1 all Release
.\run.ps1 wpf Release
```

`sync.ps1` always copies and prints the contract that is **actually** published on `main`.

## License

OcctCSharpBridge is licensed under **GNU LGPL version 2.1 + OcctCSharpBridge Exception 1.0**. Commercial and proprietary applications may use the Bridge under the Exception; GNU LGPL obligations continue to apply to OcctCSharpBridge itself and distributed modifications. See the repository root `LICENSE`, `LICENSE_LGPL_21.txt`, `OcctCSharpBridge_LGPL_EXCEPTION.txt`, and `COMMERCIAL.md`.
