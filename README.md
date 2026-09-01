# OcctCSharpBridge

[简体中文](README.zh-CN.md) · [English Docs](docs/en-US/README.md) · [中文文档](docs/zh-CN/README.md) · [Third-party SDK Guide](docs/en-US/09_Third-Party-SDK-Consumption.md) · [Unified Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge 3.0 is a reusable **Open CASCADE Technology 7.9.0 → .NET** bridge for CAD/BIM/engineering applications.

## Current baseline

| Item | Value |
| --- | --- |
| Bridge | 3.0.0 |
| Native ABI | 5 only |
| OCCT | 7.9.0 |
| Build SDK | stable .NET 10 |
| Managed targets | .NET 8 / 9 / 10 |
| Windows UI | WinForms / WPF / Avalonia |
| Linux UI | Avalonia source build |
| Official prebuilt | Windows x64 |

`bridge-contract.json` is the machine-readable build/distribution contract. It does not maintain a separate frozen API inventory.

## Build

Windows:

```powershell
.\build.ps1 build Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux:

```bash
./build.sh build Release
```

## Distribution

Build only:

```powershell
.\build.ps1 build Release -OcctRoot "D:\tools\occt-vc144-64"
```

Install/update the shared Windows x64 SDK:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

Default SDK root: `C:\Program Files\OcctCSharpBridge\SDK\3.0\win-x64`. Override with `OCCTCSHARPBRIDGE_SDK` or `-InstallRoot`. Windows consumers reference this installed SDK directly; no repository sync copy is required.

WPF/Avalonia `HostState == Ready` now represents a usable native size that has completed `ResizeSurface + Redraw`. Applications should not add startup mouse/dispatcher-delay redraw workarounds.

## Minimal use

```csharp
using OcctNet;

OcctRuntime.Configure();

using var model = new OcctModelingSession();
var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -5), OcctVector3d.UnitZ, 8, 20);
var cut = model.Cut(plate, hole);
model.ExportStep(cut.Shape, "plate.step");
```

The Bridge remains a low-level geometry/modeling/viewer wrapper. Documents, feature trees, commands, undo/redo, snapping, grips, catalog/business semantics, and project persistence belong to applications built on top of it.

## Demo previews

The [demo](https://github.com/zly258/OcctCSharpBridge/tree/demo) branch contains reference WinForms, WPF, and Avalonia hosts.

Generated `dist/`, `artifacts/`, portable SDKs, and release archives are build artifacts and are not committed to source branches.
