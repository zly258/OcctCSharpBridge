# OcctCSharpBridge

[简体中文](README.zh-CN.md) · [English Docs](docs/en-US/README.md) · [中文文档](docs/zh-CN/README.md) · [Third-party SDK Guide](docs/en-US/09_Third-Party-SDK-Consumption.md) · [Stable Support Policy](docs/en-US/10_Stable-Support-and-Compatibility.md) · [Unified Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge 3.0 is a reusable **Open CASCADE Technology 7.9.0 → .NET** bridge for CAD/BIM/engineering applications. The source tree supports Windows x64 and Linux x64. **Official prebuilt 3.x SDK assets are published for Windows x64 only**; Linux remains a supported source-build and Avalonia runtime platform.

Bridge 3 supports **Native ABI 5 only**. ABI 4 exports, compatibility shims, legacy handles, and retired Binary SDK payloads are outside the stable 3.x contract.

## 3.0 Stable contract

| Item | Contract |
| --- | --- |
| Bridge | **3.0.0** |
| Native ABI | **ABI 5 only** |
| OCCT | **7.9.0 exact** |
| Build SDK | stable .NET 10 SDK, baseline `10.0.100`, `latestFeature` |
| Default regression/smoke runtime | **.NET 10** |
| Managed Binary compatibility TFM | Core/Avalonia `net8.0`; WinForms/WPF `net8.0-windows` |
| Consumers | **.NET 8 / 9 / 10** |
| Windows UI | WinForms / WPF / Avalonia |
| Linux UI | Avalonia, source build |
| Official prebuilt release | **Windows x64** |
| Source-build support | Windows x64 / Linux x64 |
| Native / C# | C++17 / C# 14 |

`bridge-contract.json` is the machine-readable source of truth. **.NET 10 is the normal development, regression-test, and smoke-test execution target.** The published managed assemblies keep the .NET 8 TFM as the minimum compatibility baseline so one flat Binary SDK remains consumable by .NET 8, 9, and 10 applications. Stable release validation additionally executes native-backed smoke tests on actual .NET 8, 9, and 10 runtimes.

## Platform distribution

### Windows x64 — official prebuilt support

The formal Release provides a Windows Portable SDK containing the managed assemblies, `runtime/` native closure, `occt/resources/`, manifests, and license/notice material. Managed assemblies, native runtime files, resources, and manifests from different builds or `sourceCommit` values must not be mixed.

### Linux x64 — source-build support

Linux source, Core, Avalonia adapter, tests, and build scripts remain maintained:

```bash
./build.sh validate Release
./build.sh all Release
./build.sh avalonia-smoke Release   # requires a usable DISPLAY
```

Official 3.x Releases do **not** publish Linux Binary/Portable assets. Linux consumers should build against OCCT 7.9.0 and the C/C++ runtime baseline appropriate for their target distribution instead of assuming a prebuilt glibc/libstdc++ ABI is portable across distributions.

## SDK production levels

### Fast consumer artifact

For Demo synchronization, internal consumers, or controlled third-party source builds:

```powershell
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

```bash
./build.sh dist Release
```

`dist` performs the required contract checks, builds Native + Managed, and writes the Binary SDK plus source identity/hashes. It intentionally skips ManagedTests, the consumer matrix, Core smoke, and viewport/window smoke tests.

### Complete Windows Bridge gate

Normal local validation:

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

Formal Stable publishing uses the single release entry point:

```powershell
.\publish.ps1 `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -Zip
```

For a Stable contract, `publish.ps1` covers:

1. Windows Native Release under `/W4 /WX`;
2. managed warnings-as-errors;
3. default ManagedTests/Core/WinForms/WPF/Avalonia smoke on **.NET 10**;
4. .NET 8/9/10 consumer compilation matrix;
5. Windows Binary and Portable SDK creation;
6. native execution on actual .NET 8, 9, and 10 runtimes;
7. isolated execution from the extracted Portable ZIP with development OCCT paths removed;
8. frozen Stable Managed API / Native ABI compatibility validation.

The Stable gate requires Microsoft.NETCore.App 8.x, 9.x, and 10.x x64 runtimes to be installed. A missing runtime fails the gate instead of being hidden by major-version roll-forward.

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

Call `OcctRuntime.Configure()` before the first `OcctEngine` or `OcctModelingSession` when using the Portable SDK layout.

For complete MSBuild references, deployment, source identity validation, and upgrade rules, see [Third-party SDK Consumption](docs/en-US/09_Third-Party-SDK-Consumption.md).

## Stable compatibility boundaries

- `OcctEngine`, `OcctModelingSession`, and their owned native objects are **not concurrent thread-safe objects by default**; calls against one instance must be serialized.
- WinForms/WPF/Avalonia viewer hosts follow the UI-thread lifecycle rules of their framework.
- Modeling values use the application's consistent unit convention; ordinary modeling APIs do not silently switch project units.
- Handles/IDs are owner-bound and must not be mixed across engines or modeling sessions.
- 3.x does not remove or break already released managed public APIs. Existing ABI 5 entry points and ABI layouts do not receive breaking changes; such changes require a new major/ABI strategy.

See [Stable Support and Compatibility](docs/en-US/10_Stable-Support-and-Compatibility.md).

## Branch responsibilities

- `main` — stable source and official Windows Release SDK producer.
- `main-dev` — Bridge development and Stable candidate validation.
- `demo` — formal SDK consumer.
- `demo-dev` — development consumer.
- `website` — project website.

Generated `dist/`, `artifacts/`, Portable SDKs, and release archives are build artifacts and are not committed to source branches.
